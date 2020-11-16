using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class IPSetting : MonoBehaviour
{
    public InputField serverIP;
    public InputField serverPort;
    public TMP_Dropdown dropDown;
    // Start is called before the first frame update
    void Start()
    {
        List<string> ipLists = new List<string>() { 
            "192.168.102.175" ,
            "106.52.28.97",
            "sndwz-login.rastargame.com",
            "182.254.192.79",
            "129.226.13.160",
            "34.80.39.1",
        };
        dropDown.ClearOptions();
        for (int i = 0; i < ipLists.Count; i++)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
            data.text = ipLists[i];
            //data.image = "指定一个图片做背景不指定则使用默认"；
            dropDown.options.Add(data);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// 当点击后值改变是触发 (切换下拉选项)
    /// </summary>
    /// <param name="v">是点击的选项在OptionData下的索引值</param>
    public void OnValueChange(int v)
    {
        //切换选项 时处理其他的逻辑...
        Debug.Log("点击下拉控件的索引是..." + v);
        serverIP.text = dropDown.options[v].text;
        GameSetting.gi.ip = serverIP.text;
        GameSetting.gi.port = uint.Parse(serverPort.text);
    }
}
