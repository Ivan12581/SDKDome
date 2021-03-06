﻿using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

namespace celia.game
{
    public class ApplePurchaseProxy : SingleClass<ApplePurchaseProxy>
    {
        private Action onSuccess, onFail;
        private List<c2l_ios_recharge.Types.transaction_info> AppleOrders;//从Apple那边拿到的所有订单
        private List<string> ServerOrders;//从服务器那边拿到的所有需要删除Apple的订单
        private List<string> ServerOrders2;//从服务器那边拿到的服务器的订单
        private string VoucherData;//支付凭证

        public ApplePurchaseProxy()
        {
            AppleOrders = new List<c2l_ios_recharge.Types.transaction_info>();
            ServerOrders = new List<string>();
            ServerOrders2 = new List<string>();
            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CIosRechargeRep,
            new EventHandler<TcpClientEventArgs>(IosRechargeRep));
        }

        /// <summary>
        /// 游戏启动去apple那边拿未发送至服务器的订单
        /// </summary>
        public void ApplePurchaseInit()
        {
            Debug.Log("---Unity---ApplePayInit---");
            JObject jObj = new JObject();
            jObj.Add("PayType", ((int)ApplePayType.Init).ToString());
            SDKManager.gi.Pay(jObj.ToString());
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
                ApplePurchaseInit();
            });
        }

        /// <summary>
        /// IOS返回
        /// </summary>
        /// <param name="state"></param>
        /// <param name="data"></param>
        public void SDKPayCallBack(int _state, Dictionary<string, string> data)
        {
            if (data.TryGetValue("PayType", out string _PayType) && data.TryGetValue("PayState", out string _PayState))
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

        private void ResetData()
        {
            AppleOrders.Clear();
            ServerOrders.Clear();
            ServerOrders2.Clear();
            VoucherData = "";
        }

        private void SetLoadingViewState(bool state)
        {
            if (state)
            {
                Debug.Log("开启支付遮罩");
            }
            else
            {
                Debug.Log("关闭支付遮罩");
            }
        }

        public void Pay(int productID, Action success = null, Action fail = null)
        {
            //"Id": 20,
            //  "PID": "throneofgirl.gem.2",
            //  "TWD": 170.0,
            //  "Type": 0,
            //  "Price": 170.0,
            //  "Diamond": 300,
            //  "ExtraDiamond": 15,
            //  "Name": "300幻水晶",
            //  "Desc": "您想以NT$170.00的價格購買300幻水晶嗎?",
            //  "RechargeType": 0,
            //  "RechargePoint": 170,
            //  "FirstRechargeReturn": 600,
            //  "NormalRechargeReturn": 410
            onSuccess = success;
            onFail = fail;
            //请求充值商品
            c2l_recharge_commodity_ask pkg = new c2l_recharge_commodity_ask
            {
                CommodityId = productID.ToString(),
                Qutity = 1
            };
            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LRechargeCommodityAsk, pkg, LogicMsgID.LogicMsgL2CRechargeCommodityRep, (args) =>
            {
                SetLoadingViewState(false);
                l2c_recharge_commodity_rep msg = l2c_recharge_commodity_rep.Parser.ParseFrom(args.msg);
                Debug.Log("---LogicMsgL2CRechargeCommodityRep--->" + JsonConvert.SerializeObject(msg));
                if (msg.Result == IOSRechargeAskResult.RechargeAskSuccess)
                {
                    SetLoadingViewState(true);//开启遮罩
                    ResetData();
                    JObject jObj = new JObject();
                    jObj.Add("PayType", ((int)ApplePayType.Pay).ToString());
                    jObj.Add("GoodID", "throneofgirl.gem.2");
                    jObj.Add("GoodNum", msg.Qutity.ToString());
                    jObj.Add("Extra", $"{msg.OrderIndex}&{AuthProcessor.gi.ID}");
                    SDKManager.gi.Pay(jObj.ToString());
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
                    JObject jObj = new JObject();
                    jObj.Add("PayType", ((int)ApplePayType.DelOrder).ToString());
                    jObj.Add("Order", Order);
                    SDKManager.gi.Pay(jObj.ToString());
                }
                //DelOrderInApple(TransactionIds);
                //状态重置
                AppleOrders.Clear();
                ServerOrders.Clear();
                VoucherData = "";
                SetLoadingViewState(false);
            }
            else if (result == IOSRechargeResult.RechargeSendGoods)
            {
            }
            else if (result == IOSRechargeResult.RechargeError)
            {
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
                    AddAppleOrders(data);
                    CheckOrderState();
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
            VoucherData = data["encodeStr"];
            data.TryGetValue("product_id", out string product_id);
            data.TryGetValue("transaction_id", out string transaction_id);
            data.TryGetValue("quantity", out string quantity);
            data.TryGetValue("Extra", out string Extra);
            if (!string.IsNullOrEmpty(Extra))
            {
                string[] Extras = Extra.Split('&');
                if (Extras.Length == 2)
                {
                    string orderIndex = Extras[0];
                    string Uid = Extras[1];
                    if (!Uid.Equals(AuthProcessor.gi.ID.ToString()))
                    {
                        Debug.Log("---该订单不是自己的---");
                    }
                    c2l_ios_recharge.Types.transaction_info Info = new c2l_ios_recharge.Types.transaction_info
                    {
                        TransactionId = transaction_id,
                        CommodityId = product_id,
                        OrderIndex = orderIndex,
                        Num = int.Parse(quantity)
                    };
                    AppleOrders.Clear();
                    AppleOrders.Add(Info);
                }
                else
                {
                    Debug.LogError("---钥匙串保存Extra is error---");
                }
            }
            else
            {
                //初始化有的时候会返回2次 第二次是钥匙串中没有Extra 是因为前面一次已经正常处理过了
                Debug.LogError("---钥匙串中没有Extra---");
            }
        }

        /// <summary>
        /// Apple支付返回
        /// </summary>
        private void PayCallBack(ApplePayState applePayState, Dictionary<string, string> data)
        {
            Debug.Log("---applePayState--->" + applePayState.ToString());
            if (applePayState == ApplePayState.Purchasing)
            {
                //第一次拉起支付或者再次请求IPA时会返回购买中这个状态
                return;
            }
            if (applePayState == ApplePayState.Success)
            {
                onSuccess?.Invoke();
                AddAppleOrders(data);

                VerifyVoucherData();

            }
            else
            {
                //apple支付失败
                data.TryGetValue("Extra", out string Extra);
                data.TryGetValue("transaction_id", out string transaction_id);
                string orderIndex = "";
                if (string.IsNullOrEmpty(Extra))
                {
                    if (data.ContainsKey("Order"))
                    {
                        Extra = data["Order"];
                    }
                }
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
                onFail?.Invoke();
                //PopupMessageManager.gi.ShowInfo(2533);
            }
            SetLoadingViewState(false);
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
            //appleOrder del
            if (ServerOrders.Count > 0)
            {
                foreach (string sInfo in ServerOrders)
                {
                    DelOrderInApple(sInfo);
                }
                ServerOrders.Clear();
            }
            //serverIndex del
            if (ServerOrders2.Count > 0)
            {
                foreach (var item in ServerOrders2)
                {
                    FailOrderToServer(item);
                }
                ServerOrders2.Clear();
            }
            //send appleOrder to server
            if (AppleOrders.Count > 0)
            {
                Debug.LogError("---开始自动补单---UID:" + AuthProcessor.gi.ID);
                VerifyVoucherData();
            }
        }

        /// <summary>
        /// 删除Apple订单
        /// </summary>
        /// <param name="Orders"></param>
        private void DelOrderInApple(string AppleOrder)
        {
            Debug.Log("---Unity---DelOrderInApple---" + AppleOrder);
            JObject jObj = new JObject
            {
                { "PayType", ((int)ApplePayType.DelOrder).ToString() },
                { "Order", AppleOrder }
            };
            SDKManager.gi.Pay(jObj.ToString());
        }

        private void FailOrderToServer(string order)
        {
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