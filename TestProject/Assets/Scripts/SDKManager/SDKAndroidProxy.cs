using System;
using System.Collections.Generic;

using UnityEngine;

namespace celia.game
{
    public class SDKAndroidProxy : SDKProxy
    {
        private static AndroidJavaObject currentActivity;

        public SDKAndroidProxy()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        public override void CallSDKMethod(SDKResultType sDKResultType, string param)
        {
            currentActivity.Call("CallFromUnity", (int)sDKResultType, param);
        }
    }
}