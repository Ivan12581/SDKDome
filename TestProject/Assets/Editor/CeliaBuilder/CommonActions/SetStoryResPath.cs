using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace celia.game.editor
{
    public class SetStoryResPath : CommonAction
    {
        //bool recordIsStoryInEditor = false;
        //bool recordIsWechatDebug = false;

        public override void PreExcute(CeliaBuildOption option)
        {
            //var storyResPathSettings = Resources.Load<StoryResPathSettings>("ScriptableObjects/GameStory/StoryResPathSettings");
            //recordIsStoryInEditor = storyResPathSettings.isEiditor;
            //recordIsWechatDebug = storyResPathSettings.wechatDebug;

            //storyResPathSettings.isEiditor = false;
            //storyResPathSettings.wechatDebug = false;

            //EditorUtility.SetDirty(storyResPathSettings);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            //Debug.Log("SetStoryResPath PreExcuted!");
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            //var storyResPathSettings = Resources.Load<StoryResPathSettings>("ScriptableObjects/GameStory/StoryResPathSettings");
            //storyResPathSettings.isEiditor = recordIsStoryInEditor;
            //storyResPathSettings.wechatDebug = recordIsWechatDebug;

            //EditorUtility.SetDirty(storyResPathSettings);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            //Debug.Log("SetStoryResPath PostExcuted!");
        }
    }
}
