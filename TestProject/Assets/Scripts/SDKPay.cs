using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;

namespace celia.game
{
    /// <summary>
    /// SDK支付单例
    /// </summary>
    public class SDKPay : SingleClass<SDKPay>
    {
        private Dictionary<string, string> data;
        private List<c2l_ios_recharge.Types.transaction_info> AppleOrders;//从Apple那边拿到的所有订单
        private List<string> ServerOrders;//从服务器那边拿到的所有需要删除Apple的订单
        private List<string> ServerOrders2;//从服务器那边拿到的服务器的订单
        private string VoucherData;//支付凭证
        private Action<bool> CallBack;
        public SDKPay()
        {
            AppleOrders = new List<c2l_ios_recharge.Types.transaction_info>();
            ServerOrders = new List<string>();
            ServerOrders2 = new List<string>();
            data = new Dictionary<string, string>
            {
                { "PayType", "1" },
                { "MoneySymbol", "TWD" },
                { "Extra", "test" },
                { "GoodID", "test1" },
                { "GoodNum", "1" },
                { "Order", "" }
            };

            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CIosRechargeRep,
            new EventHandler<TcpClientEventArgs>(IosRechargeRep));
        }

        /// <summary>
        /// 收到服务器验证结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void IosRechargeRep(object sender, TcpClientEventArgs args)
        {
            l2c_ios_recharge_rep msg = l2c_ios_recharge_rep.Parser.ParseFrom(args.msg);
            Debug.Log("---Server validation results received--->" + JsonConvert.SerializeObject(msg));
            IOSRechargeResult result = msg.RechargeResult;
            Google.Protobuf.Collections.RepeatedField<string> TransactionIds = msg.TransactionIds;

            if (result == IOSRechargeResult.RechargeReceive)
            {
                foreach (string Order in TransactionIds)
                {
                    data["PayType"] = ((int)ApplePayType.DelOrder).ToString();
                    data["Order"] = Order;
                    SDKManager.gi.Pay(data);
                }
                //状态重置
                AppleOrders.Clear();
                ServerOrders.Clear();
                VoucherData = "";
            }
            else if (result == IOSRechargeResult.RechargeSendGoods)
            {
                data["PayType"] = "2";
                //foreach (string item in TransactionIds)
                //{
                //    data["tran"] = item;

                //    SDKManager.gi.Pay(data);
                //}
            }
            else if (result == IOSRechargeResult.RechargeError)
            {
            }
        }

        /// <summary>
        /// Apple支付初始化 应该程序启动就开启
        /// </summary>
        public void ApplePayInit()
        {
            Debug.Log("---Unity---ApplePayInit---");
            data["PayType"] = ((int)ApplePayType.Init).ToString();
            AppleOrders.Clear();
            SDKManager.gi.Pay(data);
        }

        /// <summary>
        /// 从服务器初始化订单信息 应该连接逻辑服后就请求
        /// </summary>
        public void GetServerPayInfo()
        {
            Debug.Log("---GetServerPayInfo---");
            ServerOrders.Clear();
            c2l_ios_recharge_init pkg = new c2l_ios_recharge_init();
            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LIosRechargeInit, pkg, LogicMsgID.LogicMsgL2CIosRechargeInit, (args) =>
            {
                l2c_ios_recharge_init msg = l2c_ios_recharge_init.Parser.ParseFrom(args.msg);
                Debug.Log("---GetServerPayInfo--->" + JsonConvert.SerializeObject(msg));
                Google.Protobuf.Collections.RepeatedField<string> TransactionIds = msg.IdToClose;
                Google.Protobuf.Collections.RepeatedField<string> TransactionIds2 = msg.OrderIndex;
                foreach (var item in TransactionIds)
                {
                    ServerOrders.Add(item);
                }
                foreach (var item in TransactionIds2)
                {
                    ServerOrders2.Add(item);
                }
                CheckOrderState();
            });
        }

        /// <summary>
        /// Apple支付外部调用接口
        /// </summary>
        /// <param name="commodityId">Apple配置的商品id</param>
        /// <param name="qutity">The default value is 1, the minimum value is 1, and the maximum value is 10</param>
        public void Pay(string goodID, Action<bool> callBack)
        {
            CallBack = callBack;
            if (string.IsNullOrEmpty(goodID))
            {
                Debug.LogError("--SDKPay-goodid is null---");
                return;
            }

            //请求充值商品
            c2l_recharge_commodity_ask pkg = new c2l_recharge_commodity_ask
            {
                CommodityId = goodID,
                Qutity = 1
            };
            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LRechargeCommodityAsk, pkg, LogicMsgID.LogicMsgL2CRechargeCommodityRep, (args) =>
            {
                l2c_recharge_commodity_rep msg = l2c_recharge_commodity_rep.Parser.ParseFrom(args.msg);
                Debug.Log("---LogicMsgL2CRechargeCommodityRep--->" + JsonConvert.SerializeObject(msg));
                if (msg.Result == IOSRechargeAskResult.RechargeAskSuccess)
                {
                    CallBack?.Invoke(true);//开启遮罩
                    data["PayType"] = ((int)ApplePayType.Pay).ToString();
                    data["GoodID"] = SwitchGoodID(msg.CommodityId).ToString();
                    data["GoodNum"] = msg.Qutity.ToString();
                    data["Extra"] = $"{msg.OrderIndex}&{AuthProcessor.gi.ID}";
                    AppleOrders.Clear();
                    SDKManager.gi.Pay(data);
                    NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CRechargeOrderUpd, HandleServerBuyCallback, true);
                }
                else
                {
                    if (msg.Result == IOSRechargeAskResult.RechargeAskNoCommodity)
                    {
                        Debug.Log("----没有该商品---");
                    }
                    if (msg.Result == IOSRechargeAskResult.RechargeAskLastNotEnd)
                    {
                        Debug.Log("----有未完成订单---");
                    }
                    if (msg.Result == IOSRechargeAskResult.RechargeAskNotAllow)
                    {
                        Debug.Log("----不符合购买条件---");
                    }
                    if (msg.Result == IOSRechargeAskResult.RechargeAskError)
                    {
                        Debug.Log("----购买异常---");
                    }
                }
            });
        }

        public void HandleServerBuyCallback(TcpClientEventArgs args)
        {
            if (args.hasErrors)
            {
                Debug.LogError("支付模块服务器回复支付结果出错");
                return;
            }
            l2c_recharge_order_upd result = l2c_recharge_order_upd.Parser.ParseFrom(args.msg);
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(result));
            Debug.Log("-----------------Buy success-----------------");
        }

        public string SwitchGoodID(string str)
        {
            int ID = int.Parse(str);

            return "throneofgirl.gem." + ID / 10;
        }
        /// <summary>
        /// IOS返回
        /// </summary>
        /// <param name="state"></param>
        /// <param name="data"></param>
        public void SDKPayCallBack(Dictionary<string, string> data)
        {
            if (data.TryGetValue("PayType", out string _PayType)&& data.TryGetValue("PayState", out string _PayState))
            {
                ApplePayType payType = (ApplePayType)int.Parse(_PayType);
                ApplePayState payState = (ApplePayState)int.Parse(_PayState);
                switch (payType)
                {
                    case ApplePayType.Init:
                        InitCallBack(payState, data);
                        break;
                    case ApplePayType.Pay:
                        PayCallBack(payState, data);
                        break;
                    case ApplePayType.DelOrder:
                        DelOrderCallBack(payState, data);
                        break;
                }
            }
            else
            {
                Debug.LogError("PayType is null From SDK");
            }
        }

        /// <summary>
        /// Apple支付初始化返回
        /// </summary>
        /// <param name="state"></param>
        /// <param name="data"></param>
        private void InitCallBack(ApplePayState applePayState, Dictionary<string, string> data)
        {
            switch (applePayState)
            {
                case ApplePayState.Success:
                    Debug.Log("-InitCallBack--有未删除订单---");
                    if (data.ContainsKey("encodeStr"))
                    {
                        Debug.Log("---凭证收集完毕 等待连接逻辑服后 向服务器请求服务器订单 然后对比---");
                        VoucherData = data["encodeStr"];
                    }
                    else {
                        AddAppleOrders(data);
                    }
                    break;
                case ApplePayState.Fail:
                    break;
                case ApplePayState.Cancel:
                    break;
                case ApplePayState.NotFound:
                    break;
                case ApplePayState.NotAllow:
                    Debug.Log("--InitCallBack-用户不允许内购---");
                    break;
                default:
                    break;
            }
        }

        private void AddAppleOrders(Dictionary<string, string> data)
        {
            data.TryGetValue("product_id", out string product_id);
            data.TryGetValue("transaction_id", out string transaction_id);
            data.TryGetValue("quantity", out string quantity);
            data.TryGetValue("Extra", out string Extra);
            string orderIndex = "";
            if (!string.IsNullOrEmpty(Extra))
            {
                string[] Extras = Extra.Split('&');
                if (Extras.Length == 2)
                {
                    orderIndex = Extras[0];
                    string Uid = Extras[1];
                    Debug.Log("---orderIndex---" + orderIndex);
                    Debug.Log("---Uid---" + Uid);
                    Debug.Log("---AuthProcessor.gi.ID---" + AuthProcessor.gi.ID);
                    //if (!Uid.Equals(AuthProcessor.gi.ID.ToString()))
                    //{
                    //    Debug.Log("---该订单不是自己的-应服务器强烈要求 不要发给服务器--");
                    //    return;
                    //}
                }
                else
                {
                    Debug.Log("---钥匙串保存Extra is error---");
                }
            }
            else
            {
                Debug.Log("---钥匙串中没有---");
            }

            c2l_ios_recharge.Types.transaction_info Info = new c2l_ios_recharge.Types.transaction_info
            {
                TransactionId = transaction_id,
                CommodityId = product_id,
                OrderIndex = orderIndex,
                Num = int.Parse(quantity)
            };
            AppleOrders.Add(Info);
        }

        /// <summary>
        /// Apple支付返回
        /// </summary>
        private void PayCallBack(ApplePayState applePayState, Dictionary<string, string> data)
        {
            Debug.Log("---applePayState--->" + applePayState.ToString());
            if (applePayState == ApplePayState.Success)
            {
                if (data.ContainsKey("encodeStr"))
                {
                    CallBack?.Invoke(false);
                    VoucherData = data["encodeStr"];
                    VerifyVoucherData();
                }
                else
                {
                    AddAppleOrders(data);
                }
            }
            else {
                //apple支付失败
                data.TryGetValue("Extra", out string Extra);
                data.TryGetValue("transaction_id", out string transaction_id);
                string orderIndex = "";
                if (!string.IsNullOrEmpty(Extra))
                {
                    string[] Extras = Extra.Split('&');
                    if (Extras.Length == 2)
                    {
                        orderIndex = Extras[0];
                    }
                    else
                    {
                        Debug.Log("---钥匙串保存Extra is error---");
                    }
                }
                else
                {
                    Debug.Log("---钥匙串中没有---");
                }
                //orderIndex这个是可能为空的 服务器要求为空时 就传Apple的订单号
                if (string.IsNullOrEmpty(orderIndex))
                {
                    orderIndex = transaction_id;
                }
                FailOrderToServer(orderIndex);
                CallBack?.Invoke(false);
            }

            //switch (applePayState)
            //{
            //    case ApplePayState.Success:
            //        if (data.ContainsKey("encodeStr"))
            //        {
            //            CallBack?.Invoke(true);
            //            VoucherData = data["encodeStr"];
            //            VerifyVoucherData();
            //        }
            //        else
            //        {
            //            AddAppleOrders(data);
            //        }
            //        return;
            //    case ApplePayState.Fail:
            //        Debug.Log("---苹果 交易失败---");
            //        FailOrderToServer("");
            //        break;
            //    case ApplePayState.Cancel:
            //        Debug.Log("---苹果 交易取消---");
            //        break;
            //    case ApplePayState.NotFound:
            //        Debug.Log("---苹果后台那边没有查到该商品---");
            //        break;
            //    case ApplePayState.NotAllow:
            //        Debug.Log("--苹果 不允许购买---");
            //        break;
            //    case ApplePayState.Purchasing:
            //        Debug.Log("--苹果 购买中---");
            //        break;
            //    default:
            //        break;
            //}
            //CallBack?.Invoke(false);

        }

        /// <summary>
        /// 开始把交易凭证发给服务器验证
        /// </summary>
        private void VerifyVoucherData()
        {
            if (string.IsNullOrEmpty(VoucherData) || AppleOrders.Count < 1)
            {
                VoucherData = "";
                AppleOrders.Clear();
                Debug.LogError("VoucherData is nil or AppleOrders is nil");
                return;
            }
            int TotalCount = VoucherData.Length;
            int TotalPackage = (int)Math.Ceiling((double)TotalCount / 2000);
            Debug.Log("---TotalCount--->" + TotalCount);
            Debug.Log("===TotalPackage=" + TotalPackage);
            c2l_ios_recharge pkg = new c2l_ios_recharge();
            for (int PackageIndex = 1; PackageIndex <= TotalPackage; PackageIndex++)
            {
                pkg.TotalPackage = TotalPackage;
                pkg.PackageIndex = PackageIndex;
                if (TotalPackage == PackageIndex)
                {
                    if (AppleOrders.Count == 0)
                    {
                        Debug.Log("---AppleOrders.Count == 0--->");
                        return;
                    }
                    foreach (c2l_ios_recharge.Types.transaction_info info in AppleOrders)
                    {
                        pkg.IdClosed.Add(info);
                    }

                    pkg.PayToken = VoucherData.Substring((PackageIndex - 1) * 2000, TotalCount - (PackageIndex - 1) * 2000);
                    NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRecharge, pkg);
                }
                else
                {
                    pkg.PayToken = VoucherData.Substring((PackageIndex - 1) * 2000, 2000);
                    NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRecharge, pkg);
                }
            }
        }

        /// <summary>
        /// Apple删除订单结果返回
        /// </summary>
        /// <param name="state"></param>
        /// <param name="data"></param>
        private void DelOrderCallBack(ApplePayState applePayState, Dictionary<string, string> data)
        {
            data.TryGetValue("Order", out string order);
            if (string.IsNullOrEmpty(order))
            {
                return;
            }
            switch (applePayState)
            {
                case ApplePayState.Success:
                    Debug.Log("---删除订单成功---" + order);
                    break;
                case ApplePayState.Fail:
                    Debug.Log("---Apple为空订单 --" + order);
                    break;
                case ApplePayState.Cancel:
                    break;
                case ApplePayState.NotFound:
                    Debug.Log("---删除订单失败-Apple没有该订单---" + order);
                    break;
                case ApplePayState.NotAllow:
                    break;
                default:
                    break;
            }

            c2l_ios_recharge_closed pkg = new c2l_ios_recharge_closed();
            pkg.OrderDeleted.Add(order);
            NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRechargeClosed, pkg);
        }

        /// <summary>
        /// 连接逻辑服后对比服务器和苹果的订单状态
        /// </summary>
        private void CheckOrderState()
        {
            //其实应该以apple支付的为准 但是服务器非要这么做
            bool NeedVoucher = false;
            if (AppleOrders.Count == 0 && ServerOrders.Count == 0)
            {
                Debug.Log("---岁月静好--->");
            }
            else if (AppleOrders.Count == 0)
            {
                //直接服务器数据原路返回
                c2l_ios_recharge_closed pkg = new c2l_ios_recharge_closed();
                foreach (string order in ServerOrders)
                {
                    pkg.OrderDeleted.Add(order);
                }
                NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRechargeClosed, pkg);
            }
            else if (ServerOrders.Count == 0)
            {
                //需要将AppleOrders全部发服务器就好了
                NeedVoucher = true;
                //VerifyVoucherData();
            }
            else
            {
                List<string> needDelOrders = new List<string>();
                List<c2l_ios_recharge.Types.transaction_info> needVerifyData = new List<c2l_ios_recharge.Types.transaction_info>();
                //需要check
                bool IsFind = false;
                foreach (var aInfo in AppleOrders)
                {
                    IsFind = false;
                    foreach (string sInfo in ServerOrders)//服务器有 客户端也有 就是apple那边删掉apple订单
                    {
                        if (string.Equals(aInfo.TransactionId, sInfo))
                        {
                            needDelOrders.Add(sInfo);
                            IsFind = true;
                            break;
                        }
                    }
                    if (!IsFind)//客户端有 服务器没有 发过去验证
                    {
                        needVerifyData.Add(aInfo);
                    }
                }

                c2l_ios_recharge_closed pkg = new c2l_ios_recharge_closed();
                foreach (var order in ServerOrders)
                {
                    IsFind = false;
                    foreach (var sInfo in AppleOrders)//还有服务器有 Apple没有的
                    {
                        if (string.Equals(order, sInfo.TransactionId))
                        {
                            IsFind = true;
                            break;
                        }
                    }
                    if (!IsFind)
                    {
                        pkg.OrderDeleted.Add(order);
                    }
                }
                if (pkg.OrderDeleted.Count > 0)
                {
                    NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRechargeClosed, pkg);
                }

                DelOrderInApple(needDelOrders);
                AppleOrders = needVerifyData;

                //发给服务器
                NeedVoucher = true;
                //VerifyVoucherData();
            }
            //应服务器智障要求 还有另外操作他们自己的订单
            if (ServerOrders2.Count>0)
            {
                if (NeedVoucher)
                {
                    bool IsFind = false;
                    List<string> needDelOrders = new List<string>();
                    List<c2l_ios_recharge.Types.transaction_info> needVerifyData = new List<c2l_ios_recharge.Types.transaction_info>();
                    foreach (var item in AppleOrders)
                    {
                        IsFind = false;
                        foreach (var item2 in ServerOrders2)
                        {
                            if (string.Equals(item2, item.OrderIndex))//服务器有 客户端也有的就发过去验证
                            {
                                needVerifyData.Add(item);
                                IsFind = true;
                                break;
                            }
                        }
                        if (!IsFind)//客户端有但是服务器没有  服务器强烈要求直接删掉 会漏单
                        {
                            needDelOrders.Add(item.TransactionId);
                        }
                    }
                    foreach (var item in ServerOrders2)
                    {
                        IsFind = false;
                        foreach (var item2 in AppleOrders)
                        {
                            if (string.Equals(item, item2.OrderIndex))
                            {
                                IsFind = true;
                                break;
                            }
                        }
                        if (!IsFind) //服务器有 客户端没有 就让服务器去删除
                        {
                            FailOrderToServer(item);
                        }
                    }
                    AppleOrders = needVerifyData;
                    DelOrderInApple(needDelOrders);
                    VerifyVoucherData();
                }
                else {
                    foreach (var item in ServerOrders2)
                    {
                        FailOrderToServer(item);
                    }
                }
            }
        }

        /// <summary>
        /// 删除Apple订单
        /// </summary>
        /// <param name="Orders"></param>
        private void DelOrderInApple(List<string> Orders)
        {
            if (Orders.Count > 0)
            {
                foreach (string item in Orders)
                {
                    data["PayType"] = ((int)ApplePayType.DelOrder).ToString();
                    data["Order"] = item;
                    SDKManager.gi.Pay(data);
                }
            }
        }
        /// <summary>
        /// 将服务器订单取消掉
        /// </summary>
        /// <param name="order"></param>
        private void FailOrderToServer(string order) {
            if (string.IsNullOrEmpty(order))
            {
                return;
            }
            c2l_ios_recharge_fail pkg = new c2l_ios_recharge_fail
            {
                OrderIndex = order
            };
            NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRechargeFail, pkg);
        }
        private enum ApplePayType
        {
            /// <summary>
            /// 程序启动打开apple支付监听接口
            /// </summary>
            Init,

            /// <summary>
            /// 去Apple那边去支付
            /// </summary>
            Pay,

            /// <summary>
            /// 关闭Apple的订单状态
            /// </summary>
            DelOrder,
        }
        /// <summary>
        /// 苹果支付状态返回
        /// </summary>
        private enum ApplePayState
        {
            Success,
            Fail,
            Cancel,
            NotFound,
            NotAllow,
            Purchasing,
        }
    }
}