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

        public override void Init()
        {
            JObject jObj = new JObject();
            jObj.Add("msgID", (int)SDKResultType.Init);
            jObj.Add("state", 1);
            SDKManager.gi.OnResult(jObj.ToString());
        }

        public override void Login(SDKLoginType type = SDKLoginType.None)
        {

        }

        public override void Switch()
        {

        }

        public override void Pay(string jsonString)
        {

        }

        public override void GetConfigInfo()
        {

        }

        public override void UploadInfo(string jsonString)
        {

        }

        public override void ExitGame()
        {

        }
    }
}
