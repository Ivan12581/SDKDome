#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using UnityEngine;
namespace celia.game
{
    public class SDKBridgeHelper : SingleClass<SDKBridgeHelper>
    {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

#elif UNITY_ANDROID
        private static AndroidJavaObject currentActivity;
#elif UNITY_IOS
        [DllImport("__Internal")]
        private static extern void Call(int type,string jsonString);  
#endif
        public SDKBridgeHelper() {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

#elif UNITY_ANDROID
            var unityPlayer = new AndroidJavaClass("com.elex.girlsthrone.tw.gp.MainActivity");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#elif UNITY_IOS
           
            
#endif

        }
        /// <summary>
        /// SDK调用入口
        /// </summary>
        /// <param name="type">接口类型 如：登陆 支付 分享</param>
        /// <param name="jsonString">json转化的字符串</param>
        public void CallSDK(SDKResultType type, string jsonString = "")
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

#elif UNITY_ANDROID
            //Android需要一个统一的接口
            //currentActivity.Call("CallAndroidAPI",(int)type, jsonString);
            currentActivity.Call(type.ToString(), jsonString);
#elif UNITY_IOS
           CallFromUnity((int)type,jsonString);
#endif

        }


    }

}
