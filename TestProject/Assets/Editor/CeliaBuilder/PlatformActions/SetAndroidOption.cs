using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace celia.game.editor
{
    public class SetAndroidOption : PlatformAction, IAndroidAction
    {

        public override void PreExcute(CeliaBuildOption option)
        {
            bool isBetaVersion = option.ReleaseLevel == ReleaseLevel.Beta;
            // 设置PlayerOption.options
            option.PlayerOption.options = isBetaVersion ? BuildOptions.None : (BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging);
            if (option.OutputProject)
            {
                option.PlayerOption.options = option.PlayerOption.options | BuildOptions.AcceptExternalModificationsToPlayer;
            }

            SetAndroidConfigForDiffSDK(option);
            Debug.Log("SetAndroidOption PreExcuted!");
        }

        void SetAndroidConfigForDiffSDK(CeliaBuildOption option)
        {
            if (option.SDKType == SDKType.Oversea)
            {
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            }
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel27;
            // PlayerSettings.Android.useAPKExpansionFiles = false;
            Debug.Log("SetAndroidOption PostExcuted!");
        }
    }
}
