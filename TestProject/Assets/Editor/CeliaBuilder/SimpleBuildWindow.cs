using UnityEditor;
using UnityEngine;
//using System.IO;
//using UnityEditor.iOS.Xcode;
//using Newtonsoft.Json.Linq;

namespace celia.game.editor
{
    public class SimpleBuildWindow : EditorWindow
    {
        string inputText = "";
        void OnGUI()
        {
            EditorGUILayout.Space();
            inputText = EditorGUILayout.TextField("Input Build Params: ", inputText);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(inputText))
                {
                    string[] paramArray = inputText.Split(' ');
                    foreach (var param in paramArray)
                    {
                        if (!param.Contains(":"))
                        {
                            Debug.LogWarning("参数错误: " + param);
                            return;
                        }
                    }

                    Debug.Log("Params: " + string.Join(" ", paramArray));
                    CeliaBuilder.StartBuild(paramArray);
                }
            }
        }

        [MenuItem("Tools/Build/Params Build", false, 1)]
        static void CreateProjectCreationWindow()
        {
            SimpleBuildWindow window = new SimpleBuildWindow();
            window.minSize = new Vector2(800, 100);
            window.maxSize = new Vector2(800, 100);
            window.ShowUtility();
        }
    }
}
