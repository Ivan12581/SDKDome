using System.Net.NetworkInformation;

using celia.game;

using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public InputField accountName;
    public GameObject GoTest;
    public InputField passWord;
    public InputField serverIP;
    public InputField serverPort;

    /// <summary>
    /// 账号登陆
    /// </summary>
    public void AccountLogin()
    {
        Debug.Log("---Unity---AccountLogin---");
        Debug.Log("---accountName.text---" + accountName.text);
        Debug.Log("---passWord.text---" + passWord.text);
        Debug.Log("---SystemInfo.deviceUniqueIdentifier---" + SystemInfo.deviceUniqueIdentifier);
        string name = SystemInfo.deviceUniqueIdentifier;
        NetworkManager.gi.ConnectAuth_Login(name, "");
    }

    /// <summary>
    /// Apple账号登陆
    /// </summary>
    public void AppleLogin()
    {
        Debug.Log("---Unity---AppleLogin---");
        SDKManager.gi.Login(SDKLoginType.Apple, (s, dataDict) =>
        {
            Debug.Log("---Unity---AppleLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                //第一次授权登陆 有identityTokenStr等信息
                dataDict.TryGetValue("uid", out string uid);
                dataDict.TryGetValue("token", out string token);
                NetworkManager.gi.ConnectAuth_LoginApple(uid, token);
            }
            else
            {
                Debug.Log("SDK login fail ----------------");
            }
        });
    }

    /// <summary>
    /// Apple 内购支付
    /// </summary>
    public void ApplePay()
    {
        Debug.Log("---Unity---SDKPay---");
        //SDKPay.gi.Pay("test1");
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

    /// <summary>
    /// 退出
    /// </summary>
    public void Exit()
    {
        Debug.Log("---Unity---Exit---");
        SDKManager.gi.ExitGame((s, dataDict) =>
        {
        });
    }

    /// <summary>
    /// FaceBook登陆
    /// </summary>
    public void FBLogin()
    {
        Debug.Log("---Unity---FBLogin---");
        SDKManager.gi.Login(SDKLoginType.FaceBook, (s, dataDict) =>
        {
            Debug.Log("---Unity---FBLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                if (dataDict.TryGetValue("uid", out string userID) && dataDict.TryGetValue("token", out string token))
                {
                    NetworkManager.gi.ConnectAuth_LoginFaceBook(userID, token);
                }
                else
                {
                    Debug.Log("--userID is nil or token is nil--");
                }
            }
            else
            {
                Debug.Log("SDK login fail ----------------");
            }
        });
    }

    /// <summary>
    /// FaceBook分享
    /// </summary>
    public void FBShare()
    {
        Debug.Log("---Unity---FBShare---");
        SDKManager.gi.Share((s, dataDict) =>
        {
            Debug.Log("---Unity---FBShare--callback-");
        });
    }

    /// <summary>
    /// Apple GameCenter登陆
    /// </summary>
    public void GCLogin()
    {
        Debug.Log("---Unity---GCLogin---");
        SDKManager.gi.Login(SDKLoginType.GameCenter, (s, dataDict) =>
        {
            Debug.Log("---Unity---GCLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                c2a_logon_apple_gamecenter arg = new c2a_logon_apple_gamecenter();
                arg.UserIdentifier = dataDict["uid"];
                NetworkManager.gi.ConnectAuth_LoginGameCenter(arg);
            }
            else if (state == 2)
            {
                c2a_logon_apple_gamecenter arg = new c2a_logon_apple_gamecenter();
                arg.UserIdentifier = dataDict["uid"];
                NetworkManager.gi.ConnectAuth_LoginGameCenter(arg);
            }
            else
            {
                Debug.Log("SDK login fail ----------------");
            }
        });
    }

    /// <summary>
    /// Google 登陆
    /// </summary>
    public void GoogleLogin()
    {
        Debug.Log("---Unity---GoogleLogin---");
        SDKManager.gi.Login(SDKLoginType.Google, (s, dataDict) =>
        {
            Debug.Log("---Unity---GoogleLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                if (dataDict.TryGetValue("uid", out string userID) && dataDict.TryGetValue("token", out string token))
                {
                    NetworkManager.gi.ConnectAuth_LoginGoogle(userID, token);
                }
                else
                {
                    Debug.Log("--userID is nil or token is nil--");
                }
            }
            else
            {
                Debug.Log("SDK login fail ----------------");
            }
        });
    }

    /// <summary>
    /// SDK初始化
    /// </summary>
    public void InitSDK()
    {
        Debug.Log("---Unity---InitSDK---");
        SDKManager.gi.InitSDK((s, dataDict) =>
        {
            Debug.Log("---Unity---InitSDKCallBack---");
            SDKPay.gi.ApplePayInit();
        });
    }

    /// <summary>
    /// 设置auth服务器的IP、Port
    /// </summary>
    public void SetServerIP()
    {
        Debug.Log("---serverIP.text---" + serverIP.text);
        Debug.Log("---serverPort.text---" + serverPort.text);
        if (!string.IsNullOrEmpty(serverIP.text))
        {
            GameSetting.gi.ip = serverIP.text;
        }
        if (!string.IsNullOrEmpty(serverPort.text))
        {
            GameSetting.gi.port = uint.Parse(serverPort.text);
        }
        Debug.Log("---开始测试--Application.OpenURL--接口-");
        Application.OpenURL("https://www.baidu.com/");
    }

    /// <summary>
    /// 账号切换
    /// </summary>
    public void Switch()
    {
        Debug.Log("---Unity---Switch---");
        SDKManager.gi.Switch((s, dataDict) =>
        {
        });
    }

    //554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn.apps.googleusercontent.com
    //com.googleusercontent.apps.554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn
    // Start is called before the first frame update
    private void Start()
    {
        Messenger.AddEventListener<int>(Notif.INHOUSE_LOGIN_INIT_DATA_COMPLETED, (m) =>
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
        Debug.Log("---LoginView Start--");

    }
}