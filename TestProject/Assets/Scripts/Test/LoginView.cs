using System;
using System.IO;

using celia.game;

using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public InputField accountName;
    public InputField passWord;
    public GameObject CeliaSDK;
    public GameObject RastarSDK;
    public Text ShareText;
    // Start is called before the first frame update
    private void Start()
    {
        CeliaSDK.SetActive(SDKManager.gi.SDKParams.SDKType == SDKType.CeliaOversea);
        RastarSDK.SetActive(SDKManager.gi.SDKParams.SDKType == SDKType.Native);
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

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Debug.Log("-Unity--LoginView Start--Application.version:" + Application.version + " Application.productName:" + Application.productName);
    }
    /// <summary>
    /// 账号登陆
    /// </summary>
    public void AccountLogin()
    {
        Debug.Log("---Unity---AccountLogin---"+ "  accountName:" + accountName.text);
        NetworkManager.gi.ConnectAuth_Login(accountName.text, "");
    }

    /// <summary>
    /// Apple 内购支付
    /// </summary>
    public void ApplePay()
    {
        Debug.Log("---Unity---ApplePay---");
        ApplePurchaseProxy.gi.Pay(20);
    }
    public void CeliaLogin(int type)
    {
        LoginType loginType = (LoginType)type;
        Debug.Log("---Unity---CeliaLogin---" + type);
        SDKManager.gi.CeliaLogin(loginType, (s, dataDict) =>
        {
            Debug.Log("---Unity---CeliaLogin--callback-");
            int state = int.Parse(dataDict["state"]);
            if (state == 1)
            {
                if (dataDict.TryGetValue("uid", out string uid) && dataDict.TryGetValue("token", out string token))
                {
                    if (loginType == LoginType.FaceBook)
                    {
                        NetworkManager.gi.ConnectAuth_FaceBook(uid, token);
                    }
                    else if (loginType == LoginType.Google)
                    {
                        NetworkManager.gi.ConnectAuth_Google(uid, token);
                    }
                    else if (loginType == LoginType.Apple)
                    {
                        NetworkManager.gi.ConnectAuth_Apple(uid, token);
                    }
                    Messenger.DispatchEvent(Notif.LOG_IN);
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

    public void CeliaLogout()
    {
        Debug.Log("---Unity---CeliaLogout---");
        SDKManager.gi.CeliaLogout(SDKManager.gi.loginType, (s, dataDict) =>
         {
         });
    }

    /// <summary>
    /// AI-Helper 客服
    /// </summary>
    public void CustomerService()
    {
        Debug.Log("---Unity---CustomerService---");
        SDKManager.gi.CustomerService();
    }
    /// <summary>
    /// SDK初始化
    /// </summary>
    public void InitSDK()
    {
        Debug.Log("---Unity---InitSDK---");
        //SDKManager.gi.InitSDK((s, dataDict) =>
        //{
        //    Debug.Log("---Unity---InitSDKCallBack---");
        //    SDKPay.gi.ApplePayInit();
        //});
        Application.OpenURL("www.baidu.com");
    }

    public void RastarLogin()
    {
        Debug.Log("---Unity---RastarLogin---");
        SDKManager.gi.RSLogin((s, dataDict) =>
        {
            string token = dataDict["token"];
            string openid = dataDict["uid"];
            //LoginService.gi.LogInWithToken(token, openID);
        });
    }

    public void RastarLogout()
    {
        Debug.Log("---Unity---RastarLogout---");
        SDKManager.gi.RSLogout((s, dataDict) =>
        {
        });
    }
    public void RastarSwitch()
    {
        Debug.Log("---Unity---RastarSwitch---");
        SDKManager.gi.Switch((s, dataDict) =>
        {
        });
    }

    public void Share(int type)
    {
        Debug.Log("---Unity---Share--type--->" + type);
        SDKManager.gi.shareType = (ShareType)type;
        GetScreenShot((imgPath) =>
        {
            //Application.OpenURL(string.Format("https://line.me/R/msg/{0}/{1}", "image", imgPath));

            SDKManager.gi.Share(imgPath, "分享文本", (s, dataDict) =>
             {
                 Debug.Log("---Unity---Share--callback-");
                 ShareText.text = ShareText.text + "Share Code:" + s + "   ";
             });
        });
    }

    private async void GetScreenShot(Action<string> callback)
    {
        await new WaitForEndOfFrame();
        Texture2D t = new Texture2D(Screen.currentResolution.width, Screen.currentResolution.height, TextureFormat.RGB24, false);
#if UNITY_EDITOR
        t.ReadPixels(new Rect(Vector2.zero, new Vector2(Screen.currentResolution.height, Screen.currentResolution.width)), 0, 0);
#else
            t.ReadPixels(new Rect(Vector2.zero, new Vector2(Screen.currentResolution.width, Screen.currentResolution.height)), 0, 0);
#endif
        t.Apply();

        string shareName = "share.png";
        string sharePath = Path.Combine(Application.temporaryCachePath, shareName);
        if (File.Exists(sharePath))
        {
            File.Delete(sharePath);
            Debug.Log(sharePath + " deleted first!");
        }
        byte[] img = t.EncodeToPNG();

        await new WaitForEndOfFrame();
        File.WriteAllBytes(sharePath, img);
        callback?.Invoke(sharePath);
    }


}