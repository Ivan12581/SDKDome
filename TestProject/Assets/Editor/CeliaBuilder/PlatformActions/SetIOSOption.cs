using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace celia.game.editor
{
    public class SetIOSOption : PlatformAction, IIOSAction
    {
        public override void PreExcute(CeliaBuildOption option)
        {            
            PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
            if (option.ReleaseLevel == ReleaseLevel.Alpha)
            {
                option.PlayerOption.options = BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
            }
            else
            {
                option.PlayerOption.options = BuildOptions.None;
            }
            PlayerSettings.iOS.hideHomeButton = true;
            PlayerSettings.iOS.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.BottomEdge;
            PlayerSettings.iOS.allowHTTPDownload = true;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Automatic;
            Debug.Log("SetIOSOption PreExcuted!");
        }
        public override void PostExcute(CeliaBuildOption option)
        {
            //Debug.Log("SetIOSOption PostExcuted!");
        }
    }
}
