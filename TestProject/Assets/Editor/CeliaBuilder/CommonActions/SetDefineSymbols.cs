using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace celia.game.editor
{
    public class SetDefineSymbols : CommonAction
    {
        private BuildTargetGroup group;
        private string recordDefines;
        public override void PreExcute(CeliaBuildOption option)
        {
            group = BuildPipeline.GetBuildTargetGroup(option.ProcessCfg.Target);
            recordDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string newDefines = recordDefines;
            newDefines = "HOTFIX_ENABLE;AOT";
            if (option.ReleaseLevel == ReleaseLevel.Beta)
            {
                newDefines += ";CELIA_RELEASE";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(group, recordDefines);
        }
    }
}