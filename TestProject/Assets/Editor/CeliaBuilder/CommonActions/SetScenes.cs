using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace celia.game.editor
{
    public class SetScenes : CommonAction
    {
        public override void PreExcute(CeliaBuildOption option)
        {
            option.PlayerOption.scenes = ProcessScenes().ToArray();
            Debug.Log("SetScenes PreExcuted!");
        }
        public override void PostExcute(CeliaBuildOption option)
        {
            Debug.Log("SetScenes PostExcuted!");
        }

        private static List<string> ProcessScenes()
        {
            List<string> scenes = new List<string>();
            System.Type sceneMgrType = typeof(SceneMgr);


            string entryScene = "";

            foreach (var fieldInfo in sceneMgrType.GetFields())
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    string scenePath = "Assets/Scenes/" + fieldInfo.GetValue(null) as string + ".unity";

                    EntryScene entrySceneAttr = fieldInfo.GetCustomAttribute<EntryScene>();
                    if (entrySceneAttr != null)
                    {
                        entryScene = scenePath;
                    }
                    else
                    {
                        scenes.Add(scenePath);
                        Debug.Log("Add Scene With Path:" + scenePath + "Into Scene List");
                    }
                }
            }

            Debug.Log("Entry Scene Is" + entryScene);

            scenes.Add(entryScene);
            scenes.Reverse();

            return scenes;
        }
    }
}
