using celia.game;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{

    public InputField serverIP;
    public GameObject GoTest;
    //554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun.apps.googleusercontent.com
    //com.googleusercontent.apps.554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun
    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddEventListener<int>(Notif.INHOUSE_LOGIN_INIT_DATA_COMPLETED, (m)=> 
        {
            GoTest.SetActive(true);
            gameObject.SetActive(false);
            
            SDKPay.gi.GetServerPayInfo();
        });

        Messenger.AddEventListener(Notif.NO_NAME_LOG_IN, () =>
        {
            c2l_create_character_req pkt = new c2l_create_character_req();
            pkt.CharacterName = "Cest";
            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LCreateCharacterReq, pkt, LogicMsgID.LogicMsgL2CCreateCharacterRep, (e) =>
            {
                l2c_create_character_rep msg = l2c_create_character_rep.Parser.ParseFrom(e.msg);
                Debug.LogWarning(Newtonsoft.Json.JsonConvert.SerializeObject(msg));
            });
        });
        Debug.Log("---LoginView Start---");

    }

    public void SetServerIP()
    {
        if (string.IsNullOrEmpty(serverIP.text))
        {
            Debug.Log("---serverIP.text---");
            return;
        }
        Debug.Log("---SetServerIP---" + serverIP.text);
        GameSetting.gi.ip = serverIP.text;
    }


    public void Login()
    {
        //c2l_create_character_req pkt = new c2l_create_character_req();
        //pkt.CharacterName = "Cest";
        //NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LCreateCharacterReq, pkt, LogicMsgID.LogicMsgL2CCreateCharacterRep, (e) =>
        //{
        //    l2c_create_character_rep msg = l2c_create_character_rep.Parser.ParseFrom(e.msg);
        //    Debug.LogWarning(Newtonsoft.Json.JsonConvert.SerializeObject(msg));
        //});

    }

    public void InitSDK()
    {
        Debug.Log("---Unity---InitSDK---");
        SDKManager.gi.InitSDK((s, dataDict) => {

        });
    }

    public void ApplePay() {
        Debug.Log("---Unity---SDKPay---");
        SDKPay.gi.Pay("test1");
    }

    public void AppleLogin()
    {
        Debug.Log("---Unity---AppleLogin---");
        SDKManager.gi.Login(SDKLoginType.Apple,(s, dataDict) => {
            Debug.Log("---Unity---AppleLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                //第一次授权登陆 有identityTokenStr等信息
                NetworkManager.gi.ConnectAuth_LoginApple(dataDict["user"], dataDict["token"]);
            }
            else if (state == 2) {
                //后续就没有identityTokenStr这些校验信息了
                NetworkManager.gi.ConnectAuth_LoginApple(dataDict["user"]);
            }
            else
            {

                Debug.Log("SDK login fail ----------------");
            }
        });
    }
    public void GCLogin()
    {
        Debug.Log("---Unity---GCLogin---");
        SDKManager.gi.Login(SDKLoginType.GameCenter,(s, dataDict) => {
            Debug.Log("---Unity---GCLogin--callback-");
            //int state = int.Parse(dataDict["state"]);

            c2a_logon_apple_gamecenter args = new c2a_logon_apple_gamecenter();
            dataDict.TryGetValue("state", out string state);
            dataDict.TryGetValue("playerID", out string playerID);
            dataDict.TryGetValue("publicKeyUrl", out string publicKeyUrl);
            dataDict.TryGetValue("signature", out string signature);
            dataDict.TryGetValue("salt", out string salt);
            dataDict.TryGetValue("timestamp", out string timestamp);
            args.UserIdentifier = playerID;
            //应服务器要求 下面字段先注释掉 正式应该加上的
            //args.PublicKeyUrl = publicKeyUrl;
            //args.Signature = signature;
            //args.Salt = salt;
            //args.Timestamp = timestamp;
            if (int.Parse(state) == 1)
            {
                NetworkManager.gi.ConnectAuth_LoginGameCenter(args);
            }
        });
    }
    public void FBLogin()
    {
        Debug.Log("---Unity---FBLogin---");
        SDKManager.gi.Login(SDKLoginType.FaceBook, (s, dataDict) =>
        {
            Debug.Log("---Unity---FBLogin--callback-");
            //int state = int.Parse(dataDict["state"]);

            c2a_logon_apple_gamecenter args = new c2a_logon_apple_gamecenter();
            dataDict.TryGetValue("state", out string state);
            dataDict.TryGetValue("playerID", out string playerID);
            dataDict.TryGetValue("publicKeyUrl", out string publicKeyUrl);
            dataDict.TryGetValue("signature", out string signature);
            dataDict.TryGetValue("salt", out string salt);
            dataDict.TryGetValue("timestamp", out string timestamp);
            args.UserIdentifier = playerID;
            //应服务器要求 下面字段先注释掉 正式应该加上的
            //args.PublicKeyUrl = publicKeyUrl;
            //args.Signature = signature;
            //args.Salt = salt;
            //args.Timestamp = timestamp;
            if (int.Parse(state) == 1)
            {
                NetworkManager.gi.ConnectAuth_LoginGameCenter(args);
            }
        });
    }

    public void FBShare() {
        Debug.Log("---Unity---FBShare---");
    }
    public void ApplePayInit()
    {
        //Debug.Log("---Unity---ApplePayInit---");
        //SDKPay.gi.ApplePayInit();
    }
    public void Switch()
    {
        Debug.Log("---Unity---Switch---");
        SDKManager.gi.Switch((s, dataDict) => {

        });
    }
    public void Exit()
    {
        Debug.Log("---Unity---Exit---");
        SDKManager.gi.ExitGame((s, dataDict) => {

        });
    }

}




