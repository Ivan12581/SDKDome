using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace celia.game
{

    public class UIState : MonoBehaviour
    {
        //用于增加一个标题头
        [Header("                        一个标题头")]
        public string header;

        public float speed;

        public int length = 10;


        //会在 Inspector 中隐藏字段
        [HideInInspector]
        public string hide;

        [SerializeField]
        [TooltipAttribute("这是这个字段的提示信息1")]
        private string serializeField = "abc";


        //创建一个显示3行的文本框
        [Multiline(3)]
        public string multiline;

        //使值变成滑动条的方式，并限制大小
        [Range(0, 10)]
        public float range;


        //将一个字段变为颜色原则
        [ColorUsage(true)]
        public Color color;

        //创建一个文本区域，文本区域会单独一行存在
        [TextArea] 
        public string textArea;
        //创造一个高度为10的空白区域，可以用做分割线，高度单位估计是像素
        [Space(10)]
        //当字段获得焦点后，鼠标指向字段，会获得的提示信息
        [TooltipAttribute("这是这个字段的提示信息")]
        
        public Image img;
        public void Reset()
        {
            speed = 0;
            length = 0;
        }

        //没有发现产生的影响
        [GUITarget(0, 1)]
        void OnGUI() { 
            GUI.Label(new Rect(10, 10, 300, 100), "Visible on TV and Wii U GamePad only"); 
        }

        //加载时初始化运行函数
        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        { 
            Debug.Log("After scene is loaded and game is running"); 
        }

        //脚本管理的地方增加一个菜单
        [ContextMenu("Do Something")]
        void DoSomething() {
            Debug.Log("Perform operation");
        }

        //字段名称处，增加一个右键菜单。第一个参数为菜单名称，第二个参数为功能的函数名 [ContextMenuItem("Reset", "ResetBiography")]
        [Multiline(2)]
        public string playerBiography = "";
        void ResetBiography() { 
            playerBiography = ""; 
        }

        //该值，只有在点击Enter键、丢失焦点时才会被返回
        [Delayed]
        public float delay;
    }
}

