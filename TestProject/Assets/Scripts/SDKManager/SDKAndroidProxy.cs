﻿using System;
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

        public override void Init()
        {
            currentActivity.Call("Init");
        }

        public override void Login()
        {
            currentActivity.Call("Login");
        }

        public override void Switch()
        {
            currentActivity.Call("Switch");
        }

        public override void Pay(string jsonString)
        {
            currentActivity.Call("Pay", jsonString);
        }

        public override void GetConfigInfo()
        {
            currentActivity.Call("GetConfigInfo");
        }

        public override void UploadInfo(string jsonString)
        {
            currentActivity.Call("UploadInfo", jsonString);
        }

        public override void ExitGame()
        {
            currentActivity.Call("ExitGame");
        }
    }
}