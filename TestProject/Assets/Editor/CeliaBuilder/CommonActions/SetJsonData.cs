using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace celia.game.editor
{
    public class SetJsonData : CommonAction
    {
        public override void PreExcute(CeliaBuildOption option)
        {
            EditorScript.EncryptData();
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            EditorScript.DecryptData();
        }
    }
}
