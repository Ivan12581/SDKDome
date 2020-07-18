using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace celia.game
{
	public class RechargeTest : MonoBehaviour
	{
        public void Init()
        {
                
        }

        public InputField inputField;
        public void Buy()
        {
            string gooid = inputField.text;
            
            if (string.IsNullOrEmpty(gooid))
            {
                Debug.LogError("---goodid is null---");
                gooid = "test1";
            }
            SDKPay.gi.Pay("throneofgirl.gem.2");
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