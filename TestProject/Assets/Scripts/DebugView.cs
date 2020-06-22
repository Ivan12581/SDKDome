using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

namespace celia.game
{
    public class DebugView : MonoBehaviour
    {
        private void Awake()
        {
            Application.logMessageReceived += (e, b, c) =>
            {
                if (msgQueue.Count > 50)
                {
                    msgQueue.Dequeue();
                }
                
                msgQueue.Enqueue(e + b + c.ToString());
            };
        }
        
        Queue<string> msgQueue = new Queue<string>();

        private bool inRight = true;
        private Vector2 scrollViewPos = Vector2.zero;
        private bool isLog = true;
       
        //#if UNITY_EDITOR
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUIStyle guiStyle = new GUIStyle("button");
            guiStyle.fontSize = 45;

            GUIStyle logStyle = new GUIStyle(GUI.skin.textField);
            logStyle.fontSize = 24;

            if (inRight)
            {
                GUILayout.BeginArea(new Rect(0.5f * Screen.width, 0, 0.5f * Screen.width, Screen.height));
            }
            else
            {
                GUILayout.BeginArea(new Rect(0, 0, 0.5f * Screen.width, Screen.height));
            }

            isLog = GUILayout.Toggle(isLog, "开LOG关Crash", guiStyle);
            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos, GUILayout.Width(0.5f * Screen.width), GUILayout.Height(Screen.height));
            if (isLog)
            {
                if (GUILayout.Button("查看自定义Log", guiStyle))
                {
                    LogHelper.isShow = true;
                }
                if (GUILayout.Button("关闭自定义Log", guiStyle))
                {
                    LogHelper.isShow = false;
                }
                if (GUILayout.Button("自定义Log可拖动", guiStyle))
                {
                    Messenger.DispatchEvent("LOG_Drag", true);
                    LogHelper.autoUpdate = false;
                }
                if (GUILayout.Button("自定义Log不可拖动", guiStyle))
                {
                    Messenger.DispatchEvent("LOG_Drag", false);
                    LogHelper.autoUpdate = true;
                }
                if (GUILayout.Button("清除自定义Log", guiStyle))
                {
                    Messenger.DispatchEvent("LOG_Clear");
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        //#endif
    }
}