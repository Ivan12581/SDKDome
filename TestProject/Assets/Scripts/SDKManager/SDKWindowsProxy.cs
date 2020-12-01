using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace celia.game
{
    public class SDKWindowsProxy : SDKProxy
    {
        public SDKWindowsProxy()
        {
        }

        public override void CallSDKMethod(SDKResultType sDKResultType, string param)
        {
            switch (sDKResultType)
            {
                case SDKResultType.Init:
                    JObject jObj_1 = new JObject();
                    jObj_1.Add("msgID", (int)SDKResultType.Init);
                    jObj_1.Add("state", 1);
                    SDKManager.gi.OnResult(jObj_1.ToString());
                    break;
                case SDKResultType.Login:
                    JObject jObj_2 = new JObject();
                    jObj_2.Add("msgID", (int)SDKResultType.Login);
                    jObj_2.Add("state", 1);
                    jObj_2.Add("token", SDKManager.gi.SDKParams.PayKey);
                    jObj_2.Add("uid", "-");
                    SDKManager.gi.OnResult(jObj_2.ToString());
                    break;
                case SDKResultType.ConfigInfo:
                    JObject jObj_3 = new JObject();
                    jObj_3.Add("msgID", (int)SDKResultType.ConfigInfo);
                    jObj_3.Add("state", 0);
                    SDKManager.gi.OnResult(jObj_3.ToString());
                    break;
                case SDKResultType.Share:
                    JObject jObj_4 = new JObject();
                    jObj_4.Add("msgID", (int)SDKResultType.Share);
                    jObj_4.Add("state", 1);
                    SDKManager.gi.OnResult(jObj_4.ToString());
                    break;
                default:
                    break;
            }
        }
    }
}
