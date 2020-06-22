using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace celia.game.editor
{
    public class BuildAction : ScriptableObject
    {
        public virtual void PreExcute(CeliaBuildOption option){ }
        public virtual void PostExcute(CeliaBuildOption option) { }
    }
    public interface IAndroidAction { }
    public interface IIOSAction { }

    public class CommonAction : BuildAction
    {
        public static ActionLevel Level = ActionLevel.Common;

        public override void PostExcute(CeliaBuildOption option)
        {
        }

        public override void PreExcute(CeliaBuildOption option)
        {
        }
    }
    
    public class PlatformAction : BuildAction
    {
        public static ActionLevel Level = ActionLevel.Platform;

        public override void PostExcute(CeliaBuildOption option)
        {
        }

        public override void PreExcute(CeliaBuildOption option)
        {
        }
    }
}
