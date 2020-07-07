using celia.game;

using Newtonsoft.Json;

using System;
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


        string str = "12";

        int count = str.Length;
        int num = (int)Math.Ceiling((double)count / 2);
        c2l_ios_recharge pkg = new c2l_ios_recharge();
        for (int i = 1; i <= num; i++)
        {
            string str1;
            if (num == i)
            {
                str1 = str.Substring((i - 1) * 2, count- (i - 1) * 2);
            }
            else {
                str1 = str.Substring((i - 1) * 2, 2);
            }

            Debug.Log(str1);
        }
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
            //SDKManager.gi.Login((s, dataDict) => {
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
            //});
        }
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
            OutputTop.text = "Init back:" + Newtonsoft.Json.JsonConvert.SerializeObject(dataDict);
        });
    }
    public void ApplePay()
    {
        Debug.Log("---Unity---SDKPay---");

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("PayType", "1");//这里定义支付类型 1去Apple为支付 2为服务器返回支付验证结果
        data.Add("MoneySymbol", "TWD");
        data.Add("Extra", "test");
        data.Add("GoodID", "test1");
        data.Add("GoodNum", "1");
        SDKManager.gi.Pay(data, (s, v) =>
        {
            //开始把交易凭证发给服务器验证
            v.TryGetValue("encodeStr",out string receiptData);
            v.TryGetValue("product_id", out string product_id);
            v.TryGetValue("transaction_id", out string transaction_id);

            int TotalCount = receiptData.Length;
            int TotalPackage = (int)Math.Ceiling((double)TotalCount / 2000);
            Debug.Log("---凭证总长度--->" + TotalCount);
            Debug.Log("===TotalPackage=" + TotalPackage);
            c2l_ios_recharge pkg = new c2l_ios_recharge();
            for (int PackageIndex = 1; PackageIndex <= TotalPackage; PackageIndex++)
            {
                pkg.TotalPackage = TotalPackage;
                pkg.PackageIndex = PackageIndex;
                if (TotalPackage == PackageIndex)
                {
                    pkg.RechargeOrderNo = receiptData.Substring((PackageIndex - 1) * 2000, TotalCount - (PackageIndex - 1) * 2000);
                    pkg.TransactionId = transaction_id;
                    pkg.CommodityId = product_id;
                    pkg.Num = 1;
                    NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LIosRecharge, pkg, LogicMsgID.LogicMsgL2CIosRechargeRep, (args) =>
                    {
                        //收到服务器验证结果 开始删除交易凭证
                        l2c_ios_recharge_rep msg = l2c_ios_recharge_rep.Parser.ParseFrom(args.msg);
                        Debug.Log("===收到服务器验证结果===" + JsonConvert.SerializeObject(msg));
                        IOSRechargeResult result = msg.RechargeResult;
                        Google.Protobuf.Collections.RepeatedField<string> TransactionIds = msg.TransactionIds;
                        Google.Protobuf.Collections.RepeatedField<PTGameElement> eles = msg.Eles;
                        if (result == IOSRechargeResult.RechargeReceive)
                        {
                            // finishTransaction:tran];
                            data.Add("tran", "");
                            data["PayType"] = "2";
                            
                            foreach (var item in TransactionIds)
                            {
                                data["tran"] = item;
                                SDKManager.gi.Pay(data);
                            }
                        }
                        else if (result == IOSRechargeResult.RechargeSendGoods)
                        {
                            data.Add("tran", TransactionIds[0]);
                            data["PayType"] = "2";
                            SDKManager.gi.Pay(data);
                            foreach (var item in msg.Eles)
                            {
                                int count = item.NCount;
                                int id = item.NID;
                                GameElementType type = item.EType;
                                Debug.Log("---Eles--item.NCount:" + item.NCount + " item.NID:" + item.NID + " item.EType:" + item.EType);
                            }
                        }
                    });
                }
                else {
                    pkg.RechargeOrderNo = receiptData.Substring((PackageIndex - 1) * 2000,2000);
                    NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LIosRecharge, pkg);
                }


            }


        });
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
                OutputTop.text += "\n" + "SDK fail!!!";
                Debug.Log("SDK login fail ----------------");
                waiting = false;
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
        Debug.Log("---Unity---ApplePayInit---");
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("PayType", "0");//这里定义支付类型 0:初始化支付 1去Apple为支付 2为服务器返回支付验证结果
        SDKManager.gi.Pay(data);
    }
    public void Switch()
    {
        Debug.Log("---Unity---Switch---");
        SDKManager.gi.Switch((s, dataDict) => {
            OutputTop.text = "Switch back:" + s;
        });
    }
    public void Exit()
    {
        Debug.Log("---Unity---Exit---");
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




