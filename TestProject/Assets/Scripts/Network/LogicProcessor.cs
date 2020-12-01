using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;

namespace celia.game
{
    // 登录消息处理
    internal class LogicProcessor : SingleClass<LogicProcessor>
    {
        // 网络事件回调
        public event EventHandler<AuthEventArgs> state_callback;

        public LogicProcessor()
        {
        }

        public void Init()
        {
            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CLogonAsk,
            new EventHandler<TcpClientEventArgs>(OnLogicMsgL2CLogonAsk));

            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CLogonRep,
            new EventHandler<TcpClientEventArgs>(OnLogicMsgL2CLogonRep));

            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CNoCharacter,
            new EventHandler<TcpClientEventArgs>(OnLogicMsgL2CNoCharacter));

            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CReady,
            new EventHandler<TcpClientEventArgs>(OnLogicMsgL2CReady));

            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CInitialize,
            new EventHandler<TcpClientEventArgs>(OnLogicMsgL2CInitialize));
        }

        private void OnLogicMsgL2CLogonAsk(object sender, TcpClientEventArgs e)
        {
            Debug.Log("触发OnLogicMsgL2CLogonAsk");
            l2c_logon_ask msg = l2c_logon_ask.Parser.ParseFrom(e.msg);

            uint server_seed = msg.KeySeed;
            System.Random rand = new System.Random();
            uint client_seed = (uint)rand.Next();

            string real_digest;
            {// _digest=H(_account,_client_seed,_server_seed,_K)
                byte[] _account = Encoding.ASCII.GetBytes(AuthProcessor.gi.Account.ToUpper());
                byte[] _client_seed = BitConverter.GetBytes(client_seed);
                byte[] _server_seed = BitConverter.GetBytes(server_seed);
                byte[] _K = AuthProcessor.gi.SessionKey;

                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] _digest = sha.ComputeHash(
                    _account.Concat(_client_seed).ToArray().Concat(_server_seed).ToArray().Concat(_K).ToArray());

                // 保证 _digest 转hex表示不丢失二进制位
                _digest = _digest.Concat(new byte[] { 0 }).ToArray();

                // 加密 session key
                BigInteger digest = new BigInteger(_digest);
                real_digest = digest.ToString("x");
            }

            {
                c2l_logon_proof pkt = new c2l_logon_proof();
                pkt.Username = AuthProcessor.gi.Account.ToUpper();
                pkt.ClientSeed = client_seed;
                pkt.Proof = real_digest;
                pkt.AccountId = AuthProcessor.gi.ID;
                pkt.DeviceInfo = new c2l_logon_proof.Types.LogonInfo();
                //pkt.DeviceInfo.Appid = uint.Parse(SDKManager.gi.PackageParams.AppID);
                //pkt.DeviceInfo.Channel = SDKManager.gi.PackageParams.CCHID;
                //pkt.DeviceInfo.Mdid = uint.Parse(SDKManager.gi.PackageParams.MDID);
                //pkt.DeviceInfo.Device = SDKManager.gi.PackageParams.DeviceID;
                //pkt.DeviceInfo.Devicename = UnityWebRequest.EscapeURL(SystemInfo.deviceModel);

                pkt.DeviceInfo.Appid = 111;
                pkt.DeviceInfo.Channel = "111";
                pkt.DeviceInfo.Mdid = 111;
                pkt.DeviceInfo.Device = "111";
                pkt.DeviceInfo.Devicename = "111";
                Debug.Log("---LogicMsgID.LogicMsgC2LLogonProof--->" + Newtonsoft.Json.JsonConvert.SerializeObject(pkt));
                NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LLogonProof, pkt);
                Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(pkt));
            }

            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_LOGIC_PROOF_REQ));
        }

        private void OnLogicMsgL2CLogonRep(object sender, TcpClientEventArgs e)
        {
            Debug.Log("触发OnLogicMsgL2CLogonRep");
            l2c_logon_rep msg = l2c_logon_rep.Parser.ParseFrom(e.msg);

            if (msg.Result != CharacterOpResult.CorOk)
            {
                Debug.Log("OnLogicMsgL2CLogonRep 失败");
                Debug.LogWarningFormat("{0} logic logon failed. reason:{1}", AuthProcessor.gi.Account, msg.Result);

                GameTcpClient.gi.ReturnToLoginPopup();
            }
            else
            {
                Debug.Log("OnLogicMsgL2CLogonRep 成功");

                // 状态变化
                state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_LOGIC_IN_GAME));
            }
        }

        private void OnLogicMsgL2CNoCharacter(object sender, TcpClientEventArgs e)
        {
            Debug.Log("OnLogicMsgL2CNoCharacter 触发");

            GameTcpClient.gi.isNoCharacter = true;

            l2c_no_character msg = l2c_no_character.Parser.ParseFrom(e.msg);
            Messenger.DispatchEvent(Notif.NO_NAME_LOG_IN);

            //如果有待发送的创建角色消息，则重发
            GameTcpClient.gi.SendWaitingCreateCharacterReq();
        }

        private void OnLogicMsgL2CReady(object sender, TcpClientEventArgs e)
        {
            Debug.Log("OnLogicMsgL2CReady 触发");
            //初始化完成
            GameTcpClient.gi.isInitReady = true;

            Debug.Log("重连成功，检查是否有待发的消息.");
            GameTcpClient.gi.SendWaitingMsg();

            Debug.Log("初始化数据");
            NetworkManager.gi.SendPkt(LogicMsgID.LogicMsgC2LReady, new c2l_ready());
        }

        private void OnLogicMsgL2CInitialize(object sender, TcpClientEventArgs e)
        {
            Debug.Log("OnLogicMsgL2CInitialize 触发");

            l2c_initialize msg = l2c_initialize.Parser.ParseFrom(e.msg);

            Debug.Log("登陆数据获取完成");

            AccountDataService.Create();
            AccountDataService.gi.InitializeData(msg);

            // 派发登陆完成事件，进入主界面
            Debug.Log("派发登陆完成事件，进入主界面");
            Messenger.DispatchEvent<int>(Notif.INHOUSE_LOGIN_INIT_DATA_COMPLETED, msg.Maxnomal);
        }
    }
}