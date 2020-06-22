using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using celia.game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;
using System.Security.Cryptography;
using System.Text;

public class Test : MonoBehaviour
{

    private SDKManager mgr;
    
    public Button Pay1;
    public Button Pay2;

    public InputField OutputTop;
    public InputField OutputBtm;

    private void Awake()
    {
        Pay1.onClick.AddListener(()=> 
        {
            Dictionary<string, string> payData = new Dictionary<string, string>()
            {
                { "PayMoney", "60" },
                { "OrderID", UnityEngine.Random.Range(0, 99999999).ToString() },
                { "OrderName", "购买60钻" },
                { "MoneySymbol", "CNY" },// oversea and
                { "GoodsName", "60钻" },// oversea and
                { "GoodsDesc", "获得60钻" },// oversea and
                { "Extra", "1"},// SDKManager.gi.SDKParams.SDKType == SDKType.Oversea ? "extra" : "0" },

                { "RoleID", "6" },
                { "RoleName", "TestCelia" },
                { "RoleLevel", "6" },
                { "ServerID", "6" },
                { "ServerName", "6" },
            };

            OutputTop.text = "===Pay===";
            SDKManager.gi.Pay(payData, (state, data) =>
            {
                OutputTop.text = $"state: {state} \n" + JsonConvert.SerializeObject(data);
            });
        });
        Pay2.onClick.AddListener(() =>
        {

        });
    }

    private void Start()
    {
        OutputTop.text = AuthProcessor.gi.Account + " " + AuthProcessor.gi.ID;
    }
}