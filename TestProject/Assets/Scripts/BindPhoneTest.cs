using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace celia.game
{
	public class BindPhoneTest : MonoBehaviour
	{
        public InputField phone, code;


        public void SendAsk()
        {
            c2l_bind_phone_send_msg pkg = new c2l_bind_phone_send_msg();
            pkg.PhoneNum = phone.text;
            pkg.Uid = AuthProcessor.gi.Account;
            pkg.PappId = "193";
            pkg.AppId = SDKManager.gi.PackageParams.AppID;
            pkg.CchId = SDKManager.gi.PackageParams.CCHID;
            pkg.Source = "test";

            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LBindPhoneSendMsg, pkg,LogicMsgID.LogicMsgL2CBindPhoneSendMsg,(args)=> {
                l2c_bind_phone_send_msg msg = l2c_bind_phone_send_msg.Parser.ParseFrom(args.msg);
                Debug.Log("Bind phone callback" + Newtonsoft.Json.JsonConvert.SerializeObject(msg));
            });
        }

        public void SendCheck()
        {
            c2l_bind_phone_verify pkg = new c2l_bind_phone_verify();
            pkg.PhoneNum = phone.text;
            pkg.Uid = AuthProcessor.gi.Account;
            pkg.PappId = "193";
            pkg.AppId = SDKManager.gi.PackageParams.AppID;
            pkg.CchId = SDKManager.gi.PackageParams.CCHID;
            pkg.Source = "test";
            pkg.Code = code.text;

            NetworkManager.gi.SendPktWithCallback(LogicMsgID.LogicMsgC2LBindPhoneVerify, pkg, LogicMsgID.LogicMsgL2CBindPhoneVerify, (args) =>
            {
                l2c_bind_phone_verify msg = l2c_bind_phone_verify.Parser.ParseFrom(args.msg);
                Debug.Log("Bind phone callback" + Newtonsoft.Json.JsonConvert.SerializeObject(msg));
            });
        }
	}
}