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
    //554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn.apps.googleusercontent.com
    //com.googleusercontent.apps.554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn
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
            SDKPay.gi.ApplePayInit();
        });
    }
    public void ApplePay() {
        Debug.Log("---Unity---SDKPay---");
        SDKPay.gi.Pay("test1");
    }
    public void AccountLogin()
    {
        Debug.Log("---Unity---AccountLogin---");
        NetworkManager.gi.ConnectAuth_Login("user", "token");
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
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                //第一次授权登陆 有identityTokenStr等信息
                NetworkManager.gi.ConnectAuth_LoginApple(dataDict["user"], dataDict["token"]);
            }
            else if (state == 2)
            {
                //后续就没有identityTokenStr这些校验信息了
                NetworkManager.gi.ConnectAuth_LoginApple(dataDict["user"]);
            }
            else
            {

                Debug.Log("SDK login fail ----------------");
            }

        });
    }
    public void GoogleLogin()
    {
        Debug.Log("---Unity---GoogleLogin---");
        SDKManager.gi.Login(SDKLoginType.Google, (s, dataDict) => {
            Debug.Log("---Unity---GoogleLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1){
                if (dataDict.TryGetValue("user", out string userID) && dataDict.TryGetValue("token", out string token))
                {
                    NetworkManager.gi.ConnectAuth_LoginGoogle(userID, token);
                }else {
                    Debug.Log("--userID is nil or token is nil--");
                }
            }
            else{
                Debug.Log("SDK login fail ----------------");
            }

        });
    }
    public void FBLogin()
    {
        Debug.Log("---Unity---FBLogin---");
        SDKManager.gi.Login(SDKLoginType.FaceBook, (s, dataDict) =>
        {
            Debug.Log("---Unity---FBLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                if (dataDict.TryGetValue("user", out string userID) && dataDict.TryGetValue("token", out string token)){
                    NetworkManager.gi.ConnectAuth_LoginFaceBook(userID, token);
                }
                else{
                    Debug.Log("--userID is nil or token is nil--");
                }
            }
            else
            {
                Debug.Log("SDK login fail ----------------");
            }
        });
    }

    public void FBShare() {
        Debug.Log("---Unity---FBShare---");
        SDKManager.gi.Share((s, dataDict) =>
        {
            Debug.Log("---Unity---FBShare--callback-");

        });
    }
    /// <summary>
    /// AI-Helper 客服
    /// </summary>
    public void CustomerService()
    {
        Debug.Log("---Unity---CustomerService---");
        SDKManager.gi.CustomerService((s, dataDict) =>
        {
            Debug.Log("---Unity---CustomerService--callback-");

        });
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




