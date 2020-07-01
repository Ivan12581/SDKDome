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
            if (option.SDKType == SDKType.OverseaELEX)
            {
                string newDefines = recordDefines;
                newDefines = "HOTFIX_ENABLE;CELIA_RELEASE;AOT;ELEX";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
            }
            else
            {
                if (option.ReleaseLevel == ReleaseLevel.Beta)
                {
                    string newDefines = recordDefines;
                    newDefines = "HOTFIX_ENABLE;CELIA_RELEASE;AOT";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                }
                else if (option.ReleaseLevel == ReleaseLevel.Alpha)
                {
                    string newDefines = recordDefines;
                    newDefines = "AOT";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                }
            }
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, recordDefines);
        }
    }
}