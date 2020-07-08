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
        Debug.Log("---Unity Start---");

    }

    public void SetServerIP()
    {
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
        SDKPay.gi.Pay();
    }

    public void ApplePay1()
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
                        Debug.Log("---receive data from server--->" + JsonConvert.SerializeObject(msg));
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
        Debug.Log("---Unity---ApplePayInit---");
        SDKPay.gi.Init();
        SDKPay.gi.GetSDKPayInfo();
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




