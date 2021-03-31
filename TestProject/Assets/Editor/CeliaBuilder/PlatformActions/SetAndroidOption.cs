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
            if (option.SDKType == SDKType.Native)
            {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            }
            else
            {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            }
            PlayerSettings.Android.forceSDCardPermission = true;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            //google审核需要targetSdkVersion不小于29 但是打包机版本不全可能有问题
            //PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            EditorUserBuildSettings.androidCreateSymbolsZip = true;

            Debug.Log("-SetAndroidOption--PlayerSettings.Android.targetSdkVersion--->" + PlayerSettings.Android.targetSdkVersion.ToString());
            Debug.Log("SetAndroidOption PreExcuted!");
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            Debug.Log("SetAndroidOption PostExcuted!");
        }
    }
}
