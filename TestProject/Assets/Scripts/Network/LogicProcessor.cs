using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using System.Numerics;
using System.Globalization;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

namespace celia.game
{
    // 登录消息处理
    class LogicProcessor : SingleClass<LogicProcessor>
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
                byte[] _K =  AuthProcessor.gi.SessionKey;

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

            //System.Console.WriteLine("{0} logon ask.", AuthProcessor.gi.Account);

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

            Debug.LogWarningFormat("get initialize data.");
            Debug.Log("登陆数据获取完成");

            // 根据初始化数据 初始化基础服务
            // NativeDataManager.gi.InitializeData();

            // // 初始化AccountDataService
            AccountDataService.Create();
            AccountDataService.gi.InitializeData(msg);
            /*
            // // 初始化约会数据
            EngagementService.gi.InitializeData(msg);

            // // 初始化国家声望数据
            NationService.gi.InitializeData(msg);

            // 初始化物品栏, 物品栏需要先初始化，因为CharacterService初始化依赖于Inventory数据
            InventoryService.gi.InitializeData(msg, afterInventoryInit);

            void afterInventoryInit()
            {
                // // 初始化角色数据
                CharacterService.gi.InitializeData(afterCharacterInit);

            }

            //需要在角色初始化完后再进行
            void afterCharacterInit()
            {
                //设置
                SettingService.gi.InitializeData();

                //邮箱
                MailService.gi.InitializeData();

                // // 初始化关卡数据
                ChapterService.gi.InitializeData();

                // // 初始化CG数据
                CGListService.gi.InitializeData();

                // // 初始化剧情数据
                GameStoryService.gi.InitializeData();

                // 通讯器数据的初始化请求不在登陆过程中进行, 采用惰性加载的方式，进入通讯器界面时才加载, 改为登录时初始化标记相关变量
                //190715，tianyou。由于需要在界面实现弹出式视频通话。如果进入界面才加载，则无法监听到消息解锁。所以改回在此初始化数据
                WechatService.gi.Init();
                WechatService.gi.InitializeData();

                //初始化钓鱼数据
                FishingManager.gi.InitializeData();

                // 初始化单人副本
                IndividualTowerService.gi.InitializeData();

                // 初始化百科
                EncyclopediaService.gi.InitializeData();

                // 初始化商店
                ShoppingMallService.gi.InitializeData();

                // 每日登录
                BulletinSignService.gi.InitializeData();

                //新手引导
                PlayerGuideManager.gi.InitializeData();

                //成就系统初始化
                AchievementService.gi.Init();
                AchievementService.gi.InitializeData();

                //任务系统
                MissionService.gi.Init();
                MissionService.gi.InitData();

                // 探索
                ExploreService.gi.InitializeData();

                CollectionService.gi.InitializeData();

                //资源购买
                ResourcePurchaseService.gi.InitializeData();

                //定时体力领取
                DailyVitalityService.gi.InitializeData();

                ResourcePurchaseService.gi.InitializeData();

            //抽卡
                GachaService.gi.InitializeData();
                     */
            // 派发登陆完成事件，进入主界面
            Messenger.DispatchEvent<int>(Notif.INHOUSE_LOGIN_INIT_DATA_COMPLETED, msg.Maxnomal);
            }

       

        }
        

            /*
        public static void OnDisconnectReturnToLoginResetData()
        {
            try
            {
                PlayerGuideManager.gi.ResetPlayerGuide();
                TmpStaticDataSave.ResetAllDataOnReturnToLogin();
                NativeDataManager.gi.OnReturnToLogin();
            }
            catch (Exception)
            {
                throw;
            }         
        }
        */
    
}
