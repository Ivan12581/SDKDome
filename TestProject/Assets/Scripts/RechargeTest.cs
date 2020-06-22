using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace celia.game
{
	public class RechargeTest : MonoBehaviour
	{
        public InputField OutputTop;
        public void Init()
        {
            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LRechargeGoodsInit, new c2l_recharge_goods_init(), LogicMsgID.LogicMsgL2CRechargeGoodsInit, (args) =>
            {
                l2c_recharge_goods_init msg = l2c_recharge_goods_init.Parser.ParseFrom(args.msg);
           //     LogHelper.Log($"TotalCost : {msg.Goods.TotalCost}");

                foreach (var item in msg.Goods.DoubleGoods)
                {
                    LogHelper.Log($"DoubleGoods : {item}");
                }
            });
        }

        public InputField inputField;
        public void Buy()
        {
            int id = int.Parse(inputField.text);
            c2l_generate_recharge_order pkg = new c2l_generate_recharge_order();
            pkg.CommodityId = id;

            LogHelper.Log("buying : " + pkg.CommodityId);

            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LGenerateRechargeOrder, pkg, LogicMsgID.LogicMsgL2CGenerateRechargeOrder, (args) =>
            {
                l2c_generate_recharge_order msg = l2c_generate_recharge_order.Parser.ParseFrom(args.msg);
                LogHelper.Log(Newtonsoft.Json.JsonConvert.SerializeObject(msg));
                OutputTop.text = OutputTop.text + Newtonsoft.Json.JsonConvert.SerializeObject(msg);

                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RoleID", AuthProcessor.gi.ID.ToString());
                data.Add("RoleName", AccountDataService.gi.getAccountData().name);
                data.Add("RoleLevel", "60");
                data.Add("ServerID", "1");
                data.Add("ServerName", "1");
                data.Add("PayMoney",GetCost(id));
                data.Add("OrderID", msg.AppOrderNo);
                data.Add("OrderName","TestOrderName");
                data.Add("GoodsName", GetGoodsName(id));
                data.Add("GoodsDesc", GetGoodsDesc(id));
                data.Add("MoneySymbol", "TWD");
                data.Add("Extra", msg.ThroughParam);
                SDKManager.gi.Pay(data ,(s,v)=>
                {
                    LogHelper.Log("SDK Pay Callback state : " + s);
                    foreach (var item in v)
                    {
                        LogHelper.Log("SDK Pay Callback info : " + item.Key + "/" + item.Value);
                    }
                });
            });

            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CRechargeOrderUpd, (args) => 
            {
                l2c_recharge_order_upd msg = l2c_recharge_order_upd.Parser.ParseFrom(args.msg);
                LogHelper.Log(Newtonsoft.Json.JsonConvert.SerializeObject(msg));
                LogHelper.Log("-----------------Buy success-----------------");
                OutputTop.text = OutputTop.text + Newtonsoft.Json.JsonConvert.SerializeObject(msg);
            });
        }

        string GetGoodsName(int id)
        {
            switch (id)
            {
                case 10:
                    return "60幻水晶";

                case 20:
                    return "300幻水晶";

                case 30:
                    return "680幻水晶";

                case 40:
                    return "980幻水晶";

                case 50:
                    return "1980幻水晶";

                case 60:
                    return "3280幻水晶";

                case 70:
                    return "6480幻水晶";

                default:
                    return "60幻水晶";
            }
        }

        string GetGoodsDesc(int id)
        {
            switch (id)
            {
                case 10:
                    return "您想以NT$30.00的价格购买60幻水晶吗？";

                case 20:
                    return "您想以NT$150.00的价格购买300幻水晶吗？";

                case 30:
                    return "您想以NT$300.00的价格购买680幻水晶吗？";

                case 40:
                    return "您想以NT$490.00的价格购买980幻水晶吗？";

                case 50:
                    return "您想以NT$990.00的价格购买1980幻水晶吗？";

                case 60:
                    return "您想以NT$1490.00的价格购买3280幻水晶吗？";

                case 70:
                    return "您想以NT$2490.00的价格购买6480幻水晶吗？";

                default:
                    return "您想以NT$30.00的价格购买60幻水晶吗？";
            }
        }

        string GetCost(int id)
        {
            switch (id)
            {
                case 10:
                    return "30";

                case 20:
                    return "150";

                case 30:
                    return "300";

                case 40:
                    return "490";

                case 50:
                    return "990";

                case 60:
                    return "1490";

                case 70:
                    return "2990";

                default:
                    return "60";
            }
        }

	}
}