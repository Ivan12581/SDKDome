using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace celia.game
{
    public class SDKIosProxy : SDKProxy
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void cInit();
        [DllImport("__Internal")]
        private static extern void cLogin(string jsonString);
        [DllImport("__Internal")]
        private static extern void cSwitch();
        [DllImport("__Internal")]
        private static extern void cPay(string jsonString);
        [DllImport("__Internal")]
        private static extern void cUpLoadInfo(string jsonString);
        [DllImport("__Internal")]
        private static extern void cOpenService();
        [DllImport("__Internal")]
        private static extern void cGetConfigInfo();
#endif
        public SDKIosProxy()
        {

        }

        public override void Init()
        {
#if UNITY_IOS
            cInit();
#endif
        }

        public override void Login(SDKLoginType type = SDKLoginType.Rastar)
        {
            //TODO:需要修改之前IOS那边的接口
#if UNITY_IOS
            cLogin(type.ToString());
#endif
        }

        public override void Switch()
        {
#if UNITY_IOS
            cSwitch();
#endif
        }

        public override void Pay(string jsonString)
        {
#if UNITY_IOS
            cPay(jsonString);
#endif
        }

        public override void GetConfigInfo()
        {

#if UNITY_IOS
            cGetConfigInfo();
#endif
        }

        public override void UploadInfo(string jsonString)
        {
#if UNITY_IOS
            cUpLoadInfo(jsonString);
#endif
        }

        public override void ExitGame()
        {

        }
    }
}