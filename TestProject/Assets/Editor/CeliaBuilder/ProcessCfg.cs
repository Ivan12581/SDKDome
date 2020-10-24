using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace celia.game.editor
{
    [CreateAssetMenu(menuName = "ProcessCfg")]
    public class ProcessCfg : ScriptableObject
    {
        public BuildTarget Target = BuildTarget.Android;
        public CommonAction[] CommonActions;
        public PlatformAction[] PlatformActions;
    }


    [CustomEditor(typeof(ProcessCfg))]
    public class ProcessCfgInspectorEditor : Editor
    {
        ProcessCfg processCfg;

        public List<Type> commonActions;
        public List<Type> platformActions;

        private BuildTarget lastTarget;
        private int index = 0;

        void OnEnable()
        {
            processCfg = target as ProcessCfg;
            lastTarget = processCfg.Target;
            BuildActionList();
        }

        void BuildActionList()
        {
            commonActions = new List<Type>();
            platformActions = new List<Type>();

            Type commonT = typeof(CommonAction);
            Type platT = typeof(PlatformAction);
            Type androidT = typeof(IAndroidAction);
            Type iOST = typeof(IIOSAction);

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (commonT.IsAssignableFrom(t) && commonT != t)
                    {
                        commonActions.Add(t);
                    }
                    else if (platT.IsAssignableFrom(t) && platT != t)
                    {
                        if (processCfg.Target == BuildTarget.Android && androidT.IsAssignableFrom(t) 
                            || processCfg.Target == BuildTarget.iOS && iOST.IsAssignableFrom(t))
                        {
                            platformActions.Add(t);
                        }
                    }
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (processCfg.Target != lastTarget)
            {
                BuildActionList();
                lastTarget = processCfg.Target;
            }

            if (processCfg.CommonActions == null || processCfg.PlatformActions == null)
            {
                processCfg.CommonActions = new CommonAction[0];
                processCfg.PlatformActions = new PlatformAction[0];
            }


            GUIStyle dropdownContentStyle = new GUIStyle(GUI.skin.button);
            dropdownContentStyle.alignment = TextAnchor.MiddleLeft;
            dropdownContentStyle.fontStyle = FontStyle.Bold;
            dropdownContentStyle.margin = new RectOffset(5, 5, 0, 0);
            EditorGUILayout.BeginVertical(dropdownContentStyle);
            
            DrawActions(processCfg.CommonActions);
            if (processCfg.PlatformActions.Length > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            DrawActionList(serializedObject.FindProperty("CommonActions"), commonActions);

            EditorGUILayout.Space();

            DrawActions(processCfg.PlatformActions);
            if (processCfg.PlatformActions.Length > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            DrawActionList(serializedObject.FindProperty("PlatformActions"), platformActions);

            EditorGUILayout.EndVertical();
        }
        
        private void DrawActions(UnityEngine.Object[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                object value = list.GetValue(i);
                if (value == null)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                
                GUILayout.TextArea(value.GetType().ToString() + i);


                GUIStyle helpButtonStyle = new GUIStyle(GUI.skin.button);
                helpButtonStyle.alignment = TextAnchor.MiddleCenter;
                helpButtonStyle.fontStyle = FontStyle.Normal;
                helpButtonStyle.margin = new RectOffset(0, 5, 0, 0);
                helpButtonStyle.fixedWidth = 30;

                EditorGUI.BeginDisabledGroup(i == 0);
                if (GUILayout.Button("↑↑", helpButtonStyle))
                {
                    var v = list[i];
                    list[i] = list[0];
                    list[0] = v;
                }
                if (GUILayout.Button("↑", helpButtonStyle))
                {
                    var v = list[i];
                    list[i] = list[i - 1];
                    list[i - 1] = v;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(i == list.Length - 1);
                if (GUILayout.Button("↓", helpButtonStyle))
                {
                    var v = list[i];
                    list[i] = list[i + 1];
                    list[i + 1] = v;
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("X", helpButtonStyle))
                {
                    List<UnityEngine.Object> tempList = list.ToList<UnityEngine.Object>();
                    tempList.RemoveAt(i);

                    var toDelValue = list[i];
                    DestroyImmediate(toDelValue, true);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                    if (toDelValue is CommonAction)
                    {
                        processCfg.CommonActions = tempList.Cast<CommonAction>().ToArray();
                    }
                    else if (toDelValue is PlatformAction)
                    {
                        processCfg.PlatformActions = tempList.Cast<PlatformAction>().ToArray();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawActionList(SerializedProperty list, List<Type> actionTypes)
        {

            if (actionTypes.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();

                string[] buildActionNameList = new string[actionTypes.Count];
                for (int i = 0; i < buildActionNameList.Length; i++)
                {
                    buildActionNameList[i] = actionTypes[i].Name;
                }

                GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
                popupStyle.fontSize = 11;
                popupStyle.alignment = TextAnchor.MiddleLeft;
                popupStyle.margin = new RectOffset(0, 0, 4, 0);
                popupStyle.fixedHeight = 18;
                index = EditorGUILayout.Popup(index, buildActionNameList, popupStyle, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Add Build Action", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(150)) && index < actionTypes.Count)
                {
                    Type addedType = actionTypes[index];

                    int addedIndex = list.arraySize;
                    list.InsertArrayElementAtIndex(addedIndex);
                    list.serializedObject.ApplyModifiedProperties();

                    if (typeof(CommonAction).IsAssignableFrom(addedType))
                    {
                        CommonAction[] actions = processCfg.CommonActions;
                        actions[addedIndex] = ScriptableObject.CreateInstance(addedType) as CommonAction;
                        actions[addedIndex].name = addedType.Name;
                        AssetDatabase.AddObjectToAsset(actions[addedIndex], target);
                    }
                    else if(typeof(PlatformAction).IsAssignableFrom(addedType))
                    {
                        PlatformAction[] actions = processCfg.PlatformActions;
                        actions[addedIndex] = ScriptableObject.CreateInstance(addedType) as PlatformAction;
                        actions[addedIndex].name = addedType.Name;
                        AssetDatabase.AddObjectToAsset(actions[addedIndex], target);
                    }

                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));

                    index = 0;
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No Build Actions found.", MessageType.Info);
            }
        }
    }
}