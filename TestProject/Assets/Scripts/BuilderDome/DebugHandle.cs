using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugHandle : MonoBehaviour
{
    private GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        obj = transform.Find("Image").gameObject;
        Log.Info_green("--DebugHandle Start--");
    }
    public void ButttonOnClick() {
        Log.Info_green("---ButttonOnClick---");
    }
}
