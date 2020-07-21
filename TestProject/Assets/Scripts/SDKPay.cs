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
        private List<string> ServerOrders;//从服务器那边拿到的所有需要删除的订单
        private string VoucherData;//支付凭证
        private Action<bool> CallBack;

        public SDKPay()
        {
            AppleOrders = new List<c2l_ios_recharge.Types.transaction_info>();
            ServerOrders = new List<string>();
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
            Google.Protobuf.Collections.RepeatedField<PTGameElement> eles = msg.Eles;

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
                foreach (var item in msg.Eles)
                {
                    //int count = item.NCount;
                    //int id = item.NID;
                    //GameElementType type = item.EType;
                    //Debug.Log("---Eles--item.NCount:" + item.NCount + " item.NID:" + item.NID + " item.EType:" + item.EType);
                }
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
                foreach (var item in TransactionIds)
                {
                    ServerOrders.Add(item);
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
                CallBack?.Invoke(false);
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
                if (msg.Able)
                {
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
                    CallBack?.Invoke(false);
                    Debug.Log("----不能购买此商品---");
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
                    string Uid = Extras[0];
                    orderIndex = Extras[1];
                    if (Uid.Equals(AuthProcessor.gi.ID.ToString()))
                    {
                        Debug.Log("---该订单不是自己的---");
                    }
                }
                else
                {
                    //
                    Debug.Log("---钥匙串保存Extra is error---");
                    return;
                }
            }
            else
            {
                Debug.Log("---钥匙串中没有---");
                return;
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
            switch (applePayState)
            {
                case ApplePayState.Success:
                    if (data.ContainsKey("encodeStr"))
                    {
                        CallBack?.Invoke(true);
                        VoucherData = data["encodeStr"];
                        VerifyVoucherData();
                    }
                    else
                    {
                        AddAppleOrders(data);
                    }
                    return;
                case ApplePayState.Fail:
                    Debug.Log("---苹果 交易失败---");
                    break;
                case ApplePayState.Cancel:
                    Debug.Log("---苹果 交易取消---");
                    break;
                case ApplePayState.NotFound:
                    Debug.Log("---苹果后台那边没有查到该商品---");
                    break;
                case ApplePayState.NotAllow:
                    Debug.Log("--InitCallBack-用户不允许内购---");
                    break;
                default:
                    break;
            }
            CallBack?.Invoke(false);

        }

        /// <summary>
        /// 开始把交易凭证发给服务器验证
        /// </summary>
        private void VerifyVoucherData()
        {
            if (string.IsNullOrEmpty(VoucherData) || AppleOrders.Count < 1)
            {
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
        /// 对比服务器和苹果的订单状态
        /// </summary>
        private void CheckOrderState()
        {
            if (AppleOrders.Count == 0 && ServerOrders.Count == 0)
            {
                Debug.Log("---岁月静好--->");
                return;
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
                VerifyVoucherData();
            }
            else
            {
                List<string> needDelOrders = new List<string>();
                List<c2l_ios_recharge.Types.transaction_info> needVerifyData = new List<c2l_ios_recharge.Types.transaction_info>();
                //需要check
                bool IsFind = false;
                foreach (c2l_ios_recharge.Types.transaction_info aInfo in AppleOrders)
                {
                    IsFind = false;
                    foreach (string sInfo in ServerOrders)
                    {
                        if (string.Equals(aInfo, sInfo))
                        {
                            needDelOrders.Add(sInfo);
                            IsFind = true;
                            break;
                        }
                    }
                    if (!IsFind)
                    {
                        needVerifyData.Add(aInfo);
                    }
                }

                c2l_ios_recharge_closed pkg = new c2l_ios_recharge_closed();
                foreach (var order in ServerOrders)
                {
                    //还有服务器有 Apple没有的
                    if (!needDelOrders.Contains(order))
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
                VerifyVoucherData();
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
        }
    }
}