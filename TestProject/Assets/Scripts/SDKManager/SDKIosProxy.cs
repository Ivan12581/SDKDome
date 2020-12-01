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
        private static extern void CallFromUnity(int sDKResultType, string param);
#endif
        public SDKIosProxy()
        {

        }
        public override void CallSDKMethod(SDKResultType sDKResultType, string param)
        {
#if UNITY_IOS
            CallFromUnity((int)sDKResultType, param);
#endif
        }
    }
}