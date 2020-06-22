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
                option.PlayerOption.options = BuildOptions.StrictMode;
            }
            Debug.Log("SetIOSOption PreExcuted!");
        }
        public override void PostExcute(CeliaBuildOption option)
        {
            Debug.Log("SetIOSOption PostExcuted!");
        }
    }
}
