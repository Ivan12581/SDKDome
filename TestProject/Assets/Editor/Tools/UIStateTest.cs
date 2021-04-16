using System.Collections;
using System.Collections.Generic;

using celia.game;

using UnityEditor;

using UnityEngine;

[CanEditMultipleObjects]

[CustomEditor(typeof(UIState))]
public class UIStateTest:Editor
{
    public override void OnInspectorGUI()
    {


        UIState uIState = target as UIState;
        Undo.RecordObject(uIState, "RecordTest");

        base.OnInspectorGUI();

        //在默认视图下方添加一行
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Button 1"))
        {
            //点击按钮触发
            Debug.Log("name:" + uIState.name);

            uIState.speed = 100;

            //通知Prefab属性变化，5.3版本前使用下面这句，之后使用Undo.RecordObject
            //EditorUtility.SetDirty(test);
        }


        if (GUILayout.Button("Button 2"))
        {
            //targets 表示选中的多个组件
            foreach (Object obj in targets)
            {
                Debug.Log("name:" + obj.name);
                UIState item = obj as UIState;
                item.Reset();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            //如果属性改变时调用

        }
    }
}
