using System;
using System.Collections.Generic;

using UnityEngine;

namespace celia.game
{
    public class SDKProxy
    {
        public virtual void CallSDKMethod(SDKResultType sDKResultType, string param = "") { }
    }
}