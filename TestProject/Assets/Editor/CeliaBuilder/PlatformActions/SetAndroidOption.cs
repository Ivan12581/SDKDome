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


            PlayerSettings.Android.forceSDCardPermission = true;
            Debug.Log("SetAndroidOption PreExcuted!");
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            Debug.Log("SetAndroidOption PostExcuted!");
        }
    }
}
