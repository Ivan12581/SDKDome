using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Log
{
    public static void Info(string str) {

        Debug.Log(string.Format("<color=yellow>{0}</color>", str));
    }
    public static void Info_yellow(string str)
    {
        Debug.Log(string.Format("<color=yellow>{0}</color>", str));
    }
    public static void Info_green(string str)
    {
        Debug.Log(string.Format("<color=green>{0}</color>", str));
    }
    public static void Info_blue(string str)
    {
        Debug.Log(string.Format("<color=blue>{0}</color>", str));
    }
}
