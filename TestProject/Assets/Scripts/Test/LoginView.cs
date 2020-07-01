using celia.game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public InputField NameFile;
    public InputField WordFile;
    public InputField serverIP;

    public InputField OutputTop;
    public InputField OutputBtm;

    public GameObject GoTest;

    public Text openidText, successCountText,errorOpenid;
    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddEventListener<int>(Notif.INHOUSE_LOGIN_INIT_DATA_COMPLETED, (m)=> 
        {
            GoTest.SetActive(true);
            gameObject.SetActive(false);
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
        Debug.Log("LOG TEST");
        OutputTop.text = GameSetting.gi.ip.ToString() + "===========" + Utils.ip;
        counter = 0;
        testrunning = false;
        waiting = false;

        /*
        string text = "{\"code\":200,\"msg\":\"\u6210\u529f\",\"data\":{\"adult\":1,\"openid\":\"78040639\"}}";
        string[] strs = text.Split(',');
        string code;
        string id;
        code = strs[0].Substring(strs[0].IndexOf(':') + 1);
        id = strs[3].Substring(strs[3].IndexOf('\"',9) + 1, strs[3].LastIndexOf('\"') - strs[3].IndexOf('\"',9) - 1);

        Debug.Log(code +"---"+ id);        */
    }

    public void SetServerIP()
    {
        GameSetting.gi.ip = serverIP.text;
        OutputTop.text = GameSetting.gi.ip.ToString() + "===========" + Utils.ip;
    }

    // Update is called once per frame
    void Update()
    {
        if (testrunning && !waiting)
        {
            waiting = true;
            SDKManager.gi.Login((s, dataDict) => {
                //Debug.Log("login callback called ------------------");
                //int state = int.Parse(dataDict["state"]);
                //if (state == 1)
                //{
                //    OutputTop.text += "\n" + dataDict["token"];
                //    Debug.Log("Build url calling ---------------------");
                //  //  StartCoroutine(LoginTest(BuildUrl(dataDict["token"])));

                //      //NetworkManager.gi.ConnectAuth_LoginApple(dataDict["user"],dataDict["identityTokenStr"]);
                //}
                //else
                //{
                //    OutputTop.text += "\n" + "SDK fail!!!";
                //    Debug.Log("SDK login fail ----------------");
                //    waiting = false;
                //}
            });
        }
    }

    public void Login()
    {
        //Debug.Log(NameFile.text + "===" + WordFile.text);
        //NetworkManager.gi.ConnectAuth_Login(NameFile.text, WordFile.text);


        c2l_create_character_req pkt = new c2l_create_character_req();
        pkt.CharacterName = "Cest";
        NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LCreateCharacterReq, pkt, LogicMsgID.LogicMsgL2CCreateCharacterRep, (e) =>
        {
            l2c_create_character_rep msg = l2c_create_character_rep.Parser.ParseFrom(e.msg);
            Debug.LogWarning(Newtonsoft.Json.JsonConvert.SerializeObject(msg));
        });

    }

    public void InitSDK()
    {
        Debug.Log("---Unity---InitSDK---");
        SDKManager.gi.InitSDK((s, dataDict) => {
            OutputTop.text = "Init back:" + Newtonsoft.Json.JsonConvert.SerializeObject(dataDict);
        });
    }
    public void SDKPay()
    {
        Debug.Log("---Unity---SDKPay---");

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("PayType", "1");//这里定义支付类型 1去Apple为支付 2为服务器返回支付验证结果
        data.Add("MoneySymbol", "TWD");
        data.Add("Extra", "test");
        data.Add("GoodID", "test1");
        data.Add("GoodNum", "1");
        SDKManager.gi.Pay(data);
        SDKManager.gi.Pay(data, (s, v) =>
        {
            LogHelper.Log("SDK Pay Callback state : " + s);
            foreach (var item in v)
            {
                LogHelper.Log("SDK Pay Callback info : " + item.Key + "/" + item.Value);
            }
            //开始把交易凭证发给服务器验证
            string tran = v["encodeStr"];
            //NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LGenerateRechargeOrder, pkg, LogicMsgID.LogicMsgL2CGenerateRechargeOrder, (args) =>
            //{
            //    //收到服务器验证结果 开始删除交易凭证
            //    l2c_generate_recharge_order msg = l2c_generate_recharge_order.Parser.ParseFrom(args.msg);
            //    LogHelper.Log(Newtonsoft.Json.JsonConvert.SerializeObject(msg));
            //    OutputTop.text = OutputTop.text + Newtonsoft.Json.JsonConvert.SerializeObject(msg);


            //    NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LGenerateRechargeOrder, pkg, LogicMsgID.LogicMsgL2CGenerateRechargeOrder, (args) =>
            //    {
            //        //删除完交易凭证后需告知服务器



            //    });
            //    });
            //});
        });
    }

    public void SDKLogin()
    {
        Debug.Log("---Unity---SDKLogin---");
        SDKManager.gi.Login((s, dataDict) => {
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
                OutputTop.text += "\n" + "SDK fail!!!";
                Debug.Log("SDK login fail ----------------");
                waiting = false;
            }
        });
    }

    public void Switch()
    {
        SDKManager.gi.Switch((s, dataDict) => {
            OutputTop.text = "Switch back:" + s;
        });
    }
    public void Exit()
    {
        SDKManager.gi.ExitGame((s, dataDict) => {
            OutputTop.text = "ExitGame back:" + s;
        });
    }

    public static bool waiting;
    public bool testrunning;
    int counter;
    string openid;
    public IEnumerator LoginTest(string _url)
    {
        Debug.Log("URL-----------------" + _url);
        WWW getData = new WWW(_url);  
        yield return getData;
        if (getData.error != null)
        {
            Debug.Log("httppppp error-----------------" + getData.error);
        }
        else
        {
            string resultTest = string.Copy(getData.text);
            Debug.Log(resultTest.Split(',').Length + "httppppp result-----------------" + resultTest);
            string[] strs = resultTest.Split(',');

            string code;
            string id;
            code = strs[0].Substring(strs[0].IndexOf(':') + 1);
            id = strs[3].Substring(strs[3].IndexOf('\"', 9) + 1, strs[3].LastIndexOf('\"') - strs[3].IndexOf('\"', 9) - 1);

            if (code == "200")
            {
                counter++;
                successCountText.text = counter.ToString();
                if (string.IsNullOrEmpty(openid))
                {
                    openid = id;
                    openidText.text = openid;
                }
                if (openid != id)
                {
                    errorOpenid.text = openid;
                    testrunning = false;
                }
            }
            else
            {
                Debug.Log("LOGING Error" + getData.text);
            }
        }
        waiting = false;
        if (counter == 5000)
        {
            testrunning = false;
        }
    }

    

    public string BuildUrl(string token)
    {
        string result = "http://access_token.rastargame.com/v2/Token/verify/?";
        string md5_src = $"access_token={token}&app_id=101189&cch_id=185&tm={Utils.ConvertDateTimeInt(System.DateTime.Now)}&";

        result += md5_src + "sign=" + Utils.GetMD5Key(md5_src + "4byauQ3MJXUPNi0");

        return result;
    }
}




