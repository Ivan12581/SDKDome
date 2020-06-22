using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace celia.game
{
    public class LogHelper : MonoBehaviour
    {
#if !CELIA_RELEASE
        public static bool isEnable = true;
#else
        public static bool isEnable = false;
#endif

        public GameObject itemRenderer;
        private static GameObject _rendererPrefab;
        private ScrollRect scrollRect;
        private Transform content;
        private static Transform _content;

        private static bool _isShow;
        public static bool isShow
        {
            get
            {
                return _isShow;
            }
            set
            {
                _isShow = value;
                Messenger.DispatchEvent("LOG_SHOW", value);
            }
        }

        void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            scrollRect = this.GetComponent<ScrollRect>();
            content = scrollRect.content;
            _rendererPrefab = itemRenderer;
            _content = content;

            Messenger.AddEventListener<bool>("LOG_SHOW", delegate (bool value)
             {
                 this.gameObject.SetActive(value);
             });
            isShow = false;
            Messenger.AddEventListener<bool>("LOG_Drag", (value) =>
            {
                this.GetComponent<GraphicRaycaster>().enabled = value;
            });
            this.GetComponent<GraphicRaycaster>().enabled = false;
            Messenger.AddEventListener("LOG_Clear", () =>
            {
                while (_content.childCount > 0)
                {
                    DestroyImmediate(_content.GetChild(0).gameObject);
                }
            });

            Application.logMessageReceived += (e, b, c) =>
            {
                string info = e + b + c.ToString();
                if (c == LogType.Error)
                {
                    LogError(info);
                }
                else if (c == LogType.Warning)
                {
                    Log(info, "F9FF00");
                }
                else
                {
                    Log(info);
                }
            };
        }

        public static bool autoUpdate = true;
        private async void Update()
        {
            if (infos.Count != 0)
            {
                var info = infos.Dequeue();
                TextRenderer go = Instantiate(_rendererPrefab, _content).GetComponent<TextRenderer>();
                go.transform.localScale = Vector3.one;
                go.name = "TextRenderer";
                go.log.text = "<color=#" + info.color + ">" + info.info + "</color>";
                //foreach (InfoModel info in infos)
                //{
                //    TextRenderer go = Instantiate(_rendererPrefab, _content).GetComponent<TextRenderer>();
                //    go.transform.localScale = Vector3.one;
                //    go.name = "TextRenderer";
                //    go.log.text = "<color=#" + info.color + ">" + info.info + "</color>";
                //}
                //infos.Clear();

                await new WaitForEndOfFrame();

                if (autoUpdate)
                {
                    Vector2 pos = _content.GetComponent<RectTransform>().anchoredPosition;
                    float y = Mathf.Max(0, _content.GetComponent<RectTransform>().sizeDelta.y - this.GetComponent<RectTransform>().sizeDelta.y);
                    _content.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, y);
                }
            }
        }

        private class InfoModel
        {
            public string info;
            public string color;
        }

        private static Queue<InfoModel> infos = new Queue<InfoModel>();
        //private static List<InfoModel> infos = new List<InfoModel>();

        public static void Log(string info, string color = "FFFFFF")
        {
            if (isEnable)
            {
                infos.Enqueue(new InfoModel { info = info, color = color });
                //infos.Add(new InfoModel { info = info, color = color });
            }
        }
        private static TextRenderer Log(string info)
        {
            if (isEnable)
            {
                TextRenderer go = Instantiate(_rendererPrefab, _content).GetComponent<TextRenderer>();
                go.transform.localScale = Vector3.one;
                go.name = "TextRenderer";
                go.log.text = info;
                return go;
            }
            return null;
        }

        public static void LogStack(string info)
        {
            if (isEnable)
            {
                Log(info).log.text += "<color=#FFFFFF>" + GetStackTraceModelName() + "</color>";
            }
        }

        public static void LogError(string info)
        {
            Log(info, "FF0000");
        }

        public static void LogErrorStack(string info)
        {
            if (isEnable)
            {
                TextRenderer rt = Log(info);
                info = rt.log.text;
                info = "<color=#FF0000>"+ info + "</color>"+ "<color=#FFFFFF>" + GetStackTraceModelName() + "</color>";
                rt.log.text = info;
            }
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
        }

        static string GetStackTraceModelName()
        {
            string info = null;
            StackTrace st = new StackTrace(true);
            //得到当前的所以堆栈
            StackFrame[] sf = st.GetFrames();
            for (int i = 0; i < sf.Length; ++i)
            {
                info = info + " FileName=" + sf[i].GetFileName() + "\n" +
                    " Fullname=" + sf[i].GetMethod().DeclaringType.FullName + "\n" +
                    " Function=" + sf[i].GetMethod().Name + "\n" +
                    " FileLineNo=" + sf[i].GetFileLineNumber() + "\n" +
                    " --------------------------------------------------" + "\n";
            }
            return info + "\n<color=#00FF00>#################################</color>\n";
        }
    }
}
