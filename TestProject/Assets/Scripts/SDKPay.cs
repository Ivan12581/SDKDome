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
        private Dictionary<string, string> data;
        private List<c2l_ios_recharge.Types.transaction_info> AppleOrders;//从Apple那边拿到的所有订单
        private List<string> ServerOrders;//从服务器那边拿到的所有需要删除的订单
        private string VoucherData;//支付凭证

        public SDKPay() {
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
        private void IosRechargeRep(object sender, TcpClientEventArgs args) {
            l2c_ios_recharge_rep msg = l2c_ios_recharge_rep.Parser.ParseFrom(args.msg);
            Debug.Log("---Server validation results received--->" + JsonConvert.SerializeObject(msg));
            IOSRechargeResult result = msg.RechargeResult;
            Google.Protobuf.Collections.RepeatedField<string> TransactionIds = msg.TransactionIds;
            Google.Protobuf.Collections.RepeatedField<PTGameElement> eles = msg.Eles;
            
            if (result == IOSRechargeResult.RechargeReceive){
                foreach (string Order in TransactionIds)
                {
                    data["PayType"] = ((int)PayType.DelOrder).ToString();
                    data["Order"] = Order;
                    SDKManager.gi.Pay(data);
                }
                //状态重置
                AppleOrders.Clear();
                ServerOrders.Clear();
                VoucherData = "";
            }else if (result == IOSRechargeResult.RechargeSendGoods){
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
            else if (result == IOSRechargeResult.RechargeError){

            }
        }
        /// <summary>
        /// Apple支付初始化 应该程序启动就开启
        /// </summary>
        public void ApplePayInit() {
            Debug.Log("---Unity---ApplePayInit---");
            data["PayType"] = ((int)PayType.Init).ToString();
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
        public void Pay(string goodID = "throneofgirl.gem.2",int qutity = 1)
        {
            if (string.IsNullOrEmpty(goodID)){
                Debug.LogError("--SDKPay-goodid is null---");
                return;
            }
            //请求充值商品
            c2l_recharge_commodity_ask pkg = new c2l_recharge_commodity_ask{
                CommodityId = goodID,
                Qutity = qutity
            };
            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LRechargeCommodityAsk, pkg, LogicMsgID.LogicMsgL2CRechargeCommodityRep, (args) =>{
                l2c_recharge_commodity_rep msg = l2c_recharge_commodity_rep.Parser.ParseFrom(args.msg);
                Debug.Log("---LogicMsgL2CRechargeCommodityRep--->" + JsonConvert.SerializeObject(msg));
                if (msg.Able)
                {
                    data["PayType"] = ((int)PayType.Pay).ToString();
                    data["GoodID"] = SwitchGoodID(msg.CommodityId).ToString();
                    data["GoodNum"] = msg.Qutity.ToString();
                    data["Extra"] = $"{msg.OrderIndex}&{AuthProcessor.gi.ID}";
                    AppleOrders.Clear();
                    SDKManager.gi.Pay(data);
                    NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CRechargeOrderUpd, HandleServerBuyCallback, true);
                }
                else {
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
        public string SwitchGoodID(string str) {
            int ID = int.Parse(str);
            
            return "throneofgirl.gem."+ ID/10;
        }
        public void SDKPayCallBack(int state,Dictionary<string, string> data)
        {

            if (data.TryGetValue("PayType", out string _PayType))
            {
                PayType payType = (PayType)int.Parse(_PayType);
                switch (payType)
                {
                    case PayType.None:
                        break;
                    case PayType.Init:
                        InitCallBack(state, data);
                        break;
                    case PayType.Pay:
                        PayCallBack(state, data);
                        break;
                    case PayType.DelOrder:
                        DelOrderCallBack(state, data);
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
        private void InitCallBack(int state, Dictionary<string, string> data) {
            if (state == 0){
                Debug.Log("--InitCallBack-用户不允许内购---");
            } else if (state == 1) {
                Debug.Log("-InitCallBack--有未删除订单---");
                AddAppleOrders(data);
            }
            else if (state == 2){
                if (data.ContainsKey("encodeStr")){
                    Debug.Log("---凭证收集完毕 等待连接逻辑服后 向服务器请求服务器订单 然后对比---");
                    VoucherData = data["encodeStr"];
                }else{
                    Debug.Log("---未收到凭证-没有-key:encodeStr-");
                }

            }else if (state == 4){

            }
        }
        void AddAppleOrders(Dictionary<string, string> data) {
            data.TryGetValue("product_id", out string product_id);
            data.TryGetValue("transaction_id", out string transaction_id);
            data.TryGetValue("quantity", out string quantity);
            data.TryGetValue("Extra", out string Extra);
            if (string.IsNullOrEmpty(Extra))
            {
                Debug.Log("---uid is null---");

            }
            string[] Extras = Extra.Split('&');
            if (Extras.Length!=2)
            {
                Debug.Log("---bug---");
            }
            string Uid = Extras[0];
            string orderIndex = Extras[1];
            if (Uid.Equals(AuthProcessor.gi.ID.ToString()))
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
            AppleOrders.Add(Info);
        }
        /// <summary>
        /// 发送订单和凭证到服务器
        /// </summary>
        private void PayCallBack(int state, Dictionary<string, string> data) {
            if (state == 1) {
                AddAppleOrders(data);

            } else if (state == 2) {
                if (data.ContainsKey("encodeStr")){
                    Debug.Log("---收到凭证-- 开始发送服务器验证-");
                    VoucherData = data["encodeStr"];
                    VerifyVoucherData();
                }else{
                    Debug.Log("---未收到凭证-没有-key:encodeStr-");
                }
            }else if (state == 3){
                Debug.Log("---苹果后台那边没有查到该商品---");
            }

        }
        /// <summary>
        /// 开始把交易凭证发给服务器验证
        /// </summary>
        private void VerifyVoucherData() {
            if (string.IsNullOrEmpty(VoucherData)|| AppleOrders.Count<1)
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
        private void DelOrderCallBack(int state, Dictionary<string, string> data)
        {
            data.TryGetValue("Order", out string order);
            if (state == 0){

            } else if (state == 1) {

                Debug.Log("---删除订单成功---"+ order);
            } else if (state == 2){
                Debug.Log("---删除订单失败---" + order);
            }
            else if (state == 3){
                Debug.Log("---Error:Apple 那边没有订单---"+order);
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
            if (AppleOrders.Count == 0 && ServerOrders.Count == 0){
                Debug.Log("---岁月静好--->");
                return;
            }
            else if (AppleOrders.Count == 0){
                //直接服务器数据原路返回
                c2l_ios_recharge_closed pkg = new c2l_ios_recharge_closed();
                foreach (string order in ServerOrders){
                    pkg.OrderDeleted.Add(order);
                }
                NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRechargeClosed, pkg);
            }
            else if (ServerOrders.Count == 0){
                //需要将AppleOrders全部发服务器就好了
                VerifyVoucherData();
            }
            else {
                List<string> needDelOrders = new List<string>();
                List<c2l_ios_recharge.Types.transaction_info> needVerifyData = new List<c2l_ios_recharge.Types.transaction_info>();
                //需要check
                bool IsFind = false;
                foreach (c2l_ios_recharge.Types.transaction_info aInfo in AppleOrders){
                    IsFind = false;
                    foreach (string sInfo in ServerOrders){
                        if (string.Equals(aInfo, sInfo)){
                            needDelOrders.Add(sInfo);
                            IsFind = true;
                            break;
                        }
                    }
                    if (!IsFind){
                        needVerifyData.Add(aInfo);
                    }
                }

                c2l_ios_recharge_closed pkg = new c2l_ios_recharge_closed();
                foreach (var order in ServerOrders){
                    //还有服务器有 Apple没有的
                    if (!needDelOrders.Contains(order)){
                        pkg.OrderDeleted.Add(order);
                    }
                }
                if (pkg.OrderDeleted.Count>0){
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
        private void DelOrderInApple(List<string> Orders) {
            if (Orders.Count>0)
            {
                foreach (string item in Orders)
                {
                    data["PayType"] = ((int)PayType.DelOrder).ToString();
                    data["Order"] = item;
                    SDKManager.gi.Pay(data);
                }
            }
        }
        private enum PayType
        {
            None,
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
    }
}

