using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace celia.game {
    /// <summary>
    /// SDK支付单例
    /// </summary>
    public class SDKPay : SingleClass<SDKPay>
    {
        private string Serverinfo = "";//PayInfo From Server
        private string Appleinfo = "";//PayInfo From Apple

        private bool GetServerInfo = false;
        private bool GetSDKInfo = false;
        private Dictionary<string, string> data;
        private Dictionary<string, string> AppleOrders;//从Apple那边拿到的所有订单
        private bool IsInit = false;
        public void Init()
        {
            if (IsInit)
                return;
            IsInit = true;
            AppleOrders = new Dictionary<string,string>();
            data = new Dictionary<string, string>();
            data.Add("PayType", "1");//这里定义支付类型 1去Apple为支付 2为服务器返回支付验证结果
            data.Add("MoneySymbol", "TWD");
            data.Add("Extra", "test");
            data.Add("GoodID", "test1");
            data.Add("GoodNum", "1");

            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CIosRechargeRep,
            new EventHandler<TcpClientEventArgs>(IosRechargeRep));
        }
        /// <summary>
        /// 收到服务器验证结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void IosRechargeRep(object sender, TcpClientEventArgs args) {
            l2c_ios_recharge_rep msg = l2c_ios_recharge_rep.Parser.ParseFrom(args.msg);
            Debug.Log("===收到服务器验证结果===" + JsonConvert.SerializeObject(msg));
            IOSRechargeResult result = msg.RechargeResult;
            Google.Protobuf.Collections.RepeatedField<string> TransactionIds = msg.TransactionIds;
            Google.Protobuf.Collections.RepeatedField<PTGameElement> eles = msg.Eles;
            if (result == IOSRechargeResult.RechargeReceive)
            {
                // finishTransaction:tran];
                data.Add("tran", "");
                data["PayType"] = "2";

                foreach (var item in TransactionIds)
                {
                    data["tran"] = item;
                    SDKManager.gi.Pay(data);
                }
            }
            else if (result == IOSRechargeResult.RechargeSendGoods)
            {
                data.Add("tran", TransactionIds[0]);
                data["PayType"] = "2";
                SDKManager.gi.Pay(data);
                foreach (var item in msg.Eles)
                {
                    //int count = item.NCount;
                    //int id = item.NID;
                    //GameElementType type = item.EType;
                    //Debug.Log("---Eles--item.NCount:" + item.NCount + " item.NID:" + item.NID + " item.EType:" + item.EType);
                }
            }
        }
        public void GetSDKPayInfo() {
            data["PayType"] = "0";
            SDKManager.gi.Pay(data);
        }
        public void GetServerPayInfo()
        {

        }
        public void Pay()
        {
            //请求充值商品
            //l2c_recharge_commodity_ask pkg = new l2c_recharge_commodity_ask
            //{
            //    CommodityId = "111",
            //    Qutity = 1
            //};
            //NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgL2CRechargeCommodityAsk, pkg, LogicMsgID.LogicMsgL2CRechargeCommodityRep, (args) =>{ 
            
            //});


            data["PayType"] = "1";
            SDKManager.gi.Pay(data);
        }

        public void PayCallBack(Dictionary<string, string> data)
        {
            data.TryGetValue("type",out string type);
            type = "1";
            //PayType payType = PayType.Init;
            //switch (payType) {
            //    case PayType.Init:
            //        break;
            //    case PayType.Pay:
            //        break;
            //    case PayType.DelOrder:
            //        break;
            //}
            if (type == "0")
            {
                data.TryGetValue("encodeStr", out string receiptData);
                data.TryGetValue("product_id", out string product_id);
                data.TryGetValue("transaction_id", out string transaction_id);
                AppleOrders.Add(transaction_id, receiptData);
            } else if (type == "2") { 
            
            }
            else if (type == "1") {
                //开始把交易凭证发给服务器验证
                data.TryGetValue("encodeStr", out string receiptData);
                data.TryGetValue("product_id", out string product_id);
                data.TryGetValue("transaction_id", out string transaction_id);

                int TotalCount = receiptData.Length;
                int TotalPackage = (int)Math.Ceiling((double)TotalCount / 2000);
                Debug.Log("---凭证总长度--->" + TotalCount);
                Debug.Log("===TotalPackage=" + TotalPackage);
                c2l_ios_recharge pkg = new c2l_ios_recharge();
                for (int PackageIndex = 1; PackageIndex <= TotalPackage; PackageIndex++)
                {
                    pkg.TotalPackage = TotalPackage;
                    pkg.PackageIndex = PackageIndex;
                    if (TotalPackage == PackageIndex)
                    {
                        pkg.RechargeOrderNo = receiptData.Substring((PackageIndex - 1) * 2000, TotalCount - (PackageIndex - 1) * 2000);
                        pkg.TransactionId = transaction_id;
                        pkg.CommodityId = product_id;
                        pkg.Num = 1;
                        NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRecharge, pkg);
                    }
                    else
                    {
                        pkg.RechargeOrderNo = receiptData.Substring((PackageIndex - 1) * 2000, 2000);
                        NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRecharge, pkg);
                    }


                }
            }
        }
        /// <summary>
        /// 对比服务器和苹果的订单状态
        /// </summary>
        public void CheckOrderState()
        {
            if (GetSDKInfo&&GetServerInfo)
            {
                //对比服务器和苹果的订单状态
                if (string.IsNullOrEmpty(Appleinfo) && string.IsNullOrEmpty(Serverinfo))
                {
                    return;
                }
                else if (string.IsNullOrEmpty(Appleinfo))
                {
                    return;
                }
                else if (string.IsNullOrEmpty(Serverinfo))
                {
                    foreach (var item in AppleOrders)
                    {
                        //item.
                    }
                }
                else { 
                    
                }
            }
        }

        public enum PayType
        {
            /// <summary>
            /// 程序启动打开apple支付监听接口
            /// </summary>
            Init,
            /// <summary>
            /// 去Apple那边去制度
            /// </summary>
            Pay,
            /// <summary>
            /// 关闭Apple的订单状态
            /// </summary>
            DelOrder,
        }
        /// <summary>
        /// Apple支付返回信息
        /// </summary>
        public class PayInfo { 
            /// <summary>
            /// Apple订单号
            /// </summary>
            string AppleOrder;
            /// <summary>
            /// 商品ID
            /// </summary>
            string IAP;
            /// <summary>
            /// 商品数量
            /// </summary>
            string Num;
            /// <summary>
            /// Apple支付凭证 
            /// </summary>
            string Token;
        }
    }
}

