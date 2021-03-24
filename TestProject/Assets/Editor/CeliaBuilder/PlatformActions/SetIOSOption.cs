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
            PlayerSettings.iOS.hideHomeButton = false;
            PlayerSettings.iOS.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.BottomEdge;
            PlayerSettings.iOS.allowHTTPDownload = true;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Automatic;
            PlayerSettings.accelerometerFrequency = 60;
            if (option.SDKType == SDKType.CeliaOversea)
            {
                PlayerSettings.iOS.appleDeveloperTeamID = "5HK243M76T";
                PlayerSettings.iOS.targetOSVersionString = "10.0";
                PlayerSettings.iOS.iOSManualProvisioningProfileID = "51f549e4-874d-4cb1-b528-78995ba62873";
            }
            else if (option.SDKType == SDKType.Native)
            {//星辉SDK要求
                PlayerSettings.iOS.appleDeveloperTeamID = "Z67D6RDGWU";
                PlayerSettings.iOS.targetOSVersionString = "11.0";
                PlayerSettings.iOS.iOSManualProvisioningProfileID = "287a425a-d3c6-4b79-b27d-d581df0f906c";
            }
            Debug.Log("SetIOSOption PreExcuted!");
        }
        public override void PostExcute(CeliaBuildOption option)
        {
            //Debug.Log("SetIOSOption PostExcuted!");
        }
    }
}
