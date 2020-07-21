using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace celia.game
{
    public class SDKManager : SingleMonoBehaviour<SDKManager>
    {
        public SDKParams SDKParams;
        public bool IsOversea { get { return SDKParams.SDKType == SDKType.Oversea; } }
        /// <summary>
        /// 是否需要激活码
        /// </summary>
        public bool NeedCode { get { return Application.platform == RuntimePlatform.Android && (SDKParams.SDKType == SDKType.Native || SDKParams.SDKType == SDKType.NativeChukai); } }
        public (string AppID, string CCHID, string MDID, string DeviceID) PackageParams { get; private set; }

        private SDKProxy proxy;
        private Dictionary<SDKResultType, Action<int, Dictionary<string, string>>> callBackDict = new Dictionary<SDKResultType, Action<int, Dictionary<string, string>>>();
        
        // 切换帐号流程控制相关
        public bool DoingSwitch;
        public string SwitchToken;
        private bool changingScene;

        public void Init()
        {
            SDKParams = Resources.Load<SDKParams>("SDKParams");
            //if (SDKParams.SDKType == SDKType.None)
            //{
            //    PackageParams = ("101089", "185", "200000", SystemInfo.deviceUniqueIdentifier);
            //    return;
            //}

            InitListener();

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            proxy = new SDKWindowsProxy();
#elif UNITY_ANDROID
            proxy = new SDKAndroidProxy();
#elif UNITY_IOS
            //proxy = new SDKIosProxy();
            //SDKPay.gi.ApplePayInit();
#endif
            GetConfigInfo();
            //UploadDeviceInfo(DeviceUpload.IconLaunch);
        }

        private void InitListener()
        {
            //监听登录，数据上报
            Messenger.AddEventListener<int>(Notif.INHOUSE_LOGIN_INIT_DATA_COMPLETED, (m) =>
            {
             //   UploadDeviceInfo(DeviceUpload.GameStart);
                UploadInfo(SDKUploadType.RoleEnterGame);
            });
            //监听升级，数据上报
            //Messenger.AddEventListener<int, int>(RESPONSE_UPDATE_CODE.LEVEL_UPDATE, (charactorId, level) =>
            //{
            //    if (charactorId == 1)// 女主角（玩家）
            //    {
            //        UploadInfo(SDKUploadType.RoleUpgrade);
            //    }
            //});
        }

        public void Update()
        {
            // 按下回退键
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitGame();
            }
        }

        /// <summary>
        /// Java层调用的回调
        /// </summary>
        /// <param name="resultType">回调类型</param>
        /// <param name="jsonString">回调数据</param>
        public void OnResult(string jsonString)
        {
            Debug.Log("Unity: ---SDK OnResult--->");

            Dictionary<string, string> dataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            SDKResultType type = (SDKResultType)int.Parse(dataDict["msgID"]);
            int state = dataDict.ContainsKey("state") ? int.Parse(dataDict["state"]) : -1;
            // 通用处理
            OnResultBack(type, state, dataDict);
            // 自定义一次性回调
            Action<int, Dictionary<string, string>> callBack;
            if (callBackDict.TryGetValue(type, out callBack))
            {
                callBack?.Invoke(state, dataDict);
                callBackDict[type] = null;
            }
        }

        /// <summary>
        /// SDK初始化
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void InitSDK(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Init] = callBack;
            CallSDK(SDKResultType.Init);
            //proxy?.Init();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void Login(SDKLoginType type,Action<int, Dictionary<string, string>> callBack = null)
        {
            DoingSwitch = false;
            callBackDict[SDKResultType.Login] = callBack;
            CallSDK(SDKResultType.Login, ((int)type).ToString());
            //proxy?.Login(type);
        }

        /// <summary>
        /// 切换帐号
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void Switch(Action<int, Dictionary<string, string>> callBack = null)
        {
            DoingSwitch = true;
            callBackDict[SDKResultType.Switch] = callBack;
            CallSDK(SDKResultType.Switch);
            //proxy?.Switch();
        }

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void Pay(Dictionary<string, string> payData, Action<int, Dictionary<string, string>> callBack = null)
        {
            //TODO 数据获取处理为Json
            string jsonData = JsonConvert.SerializeObject(payData);
            Debug.Log(jsonData);
            callBackDict[SDKResultType.Pay] = callBack;
            CallSDK(SDKResultType.Pay, jsonData);
            //proxy?.Pay(jsonData);
        }

        /// <summary>
        /// 上报数据
        /// </summary>
        public void UploadInfo(SDKUploadType uploadType, string name = null)
        {
            AccountData accountdata = AccountDataService.gi.getAccountData();
            string rolename = !string.IsNullOrEmpty(name) ? name : accountdata.name;
            int rolelevel = uploadType == SDKUploadType.RoleCreate ? 1 : 60;

            JObject jobj = new JObject();
            jobj.Add("uploadtype", (int)uploadType);
            //角色id，数字，不得超过32个字符
            jobj.Add("RoleID", AuthProcessor.gi.ID);
            //角色名称（必传）
            jobj.Add("RoleName", rolename);
            //角色等级，数字，不得超过32个字符（必传）
            jobj.Add("RoleLevel", rolelevel);
            //服务器id，数字，不得超过32个字符（必传）
            jobj.Add("ServerID", "001");
            //服务器名称（必传）
            jobj.Add("ServerName", "服务器001");
            //玩家余额，数字，默认0
            jobj.Add("Balance", "0");
            //玩家vip等级，数字，默认0
            jobj.Add("VIPLevel", "0");
            //玩家帮派，没有传“无”
            jobj.Add("PartyName", "无");
            //角色创建时间，单位：秒，获取服务器存储的时间，不可用手机本地时间
            jobj.Add("createtime", "-1");
            //角色升级时间，单位：秒，获取服务器存储的时间，不可用手机本地时间
            jobj.Add("upgradetime", "-1");
            //旧角色名称
            jobj.Add("oldname", "");
            //拓展字段，传旧角色名
            jobj.Add("extra", "");
            CallSDK(SDKResultType.UploadInfo,jobj.ToString());
            //proxy?.UploadInfo(jobj.ToString());
        }

        /// <summary>
        /// 获取SDK、设备信息
        /// </summary>
        public void GetConfigInfo(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.ConfigInfo] = callBack;
            CallSDK(SDKResultType.ConfigInfo);
            //proxy?.GetConfigInfo();
        }
        /// <summary>
        /// 客服入口
        /// </summary>
        /// <param name="callBack"></param>
        public void CustomerService(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.CustomerService] = callBack;
            JObject jobj = new JObject();
            jobj.Add("playerName", "123"); //游戏中用户名称。如果拿不到userName，传入空字符串@""，会使用默认昵称"anonymous"
            jobj.Add("playerUid", "123");   //用户在游戏里的唯一标示id。如果拿不到uid，传入空字符串@""，系统会生成一个唯一设备id
            jobj.Add("ServerID", "001");    //用户所在的服务器编号
            jobj.Add("PlayerParseId", "001");   //传空字符串
            jobj.Add("PlayershowConversationFlag", "001"); //参数的值是 “0” 或 “1”，标识是否开启人工入口。为 “1” 时，将在机器人客服聊天界面右上角，提供人工客服聊天的入口
            jobj.Add("Config", "001");//可选参数，自定义Dictionary信息。可以在此处设置特定的Tag信息。说明: elva - tags对应的值为array类型，此处传入自定义的标签，需要在AIHelp 客服后台配置同名称的标签才能生效。
            CallSDK(SDKResultType.CustomerService, jobj.ToString());
            //proxy?.CustomerService(jobj.ToString());
        }
        /// <summary>
        /// 分享
        /// </summary>
        /// <param name="callBack">分享回调</param>
        public void Share(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Share] = callBack;
            JObject jobj = new JObject();
            CallSDK(SDKResultType.Share, jobj.ToString());
            //proxy?.Share(jobj.ToString());
        }
        /// <summary>
        /// 退出游戏数据
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void ExitGame(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.ExitGame] = callBack;
            CallSDK(SDKResultType.ExitGame);
            //proxy?.ExitGame();
        }

        void CallSDK(SDKResultType type, string jsonString = "") {
            SDKBridgeHelper.gi.CallSDK(type, jsonString);
        }
        // 通用回调处理
        public void OnResultBack(SDKResultType resultType, int state, Dictionary<string, string> data)
        {
            switch (resultType)
            {
                case SDKResultType.Init:
                    if (state == 0)//初始化失败则重新初始化
                    {
                        //InitSDK();
                    }else if(state == 1)
                    {
                        //UploadDeviceInfo(DeviceUpload.SDKInitSucc);
                    }
                    break;
                case SDKResultType.Login:
                    // IOS SDK切换账号时，TOKEN都是从Login成功消息返回，无切换账号成功消息
                    if (state == 1 && DoingSwitch)
                    {
                        //SwitchToken = data["token"];
                        //切换到登录场景后才收到token时，发起自动登录
                        //Messenger.DispatchEvent(Notif.SDK_SWITCH_SUCCESS);
                        //UploadDeviceInfo(DeviceUpload.SDKLoginSucc);
                    }
                    break;
                case SDKResultType.Switch:
                    if (state == 1)
                    {
                        SwitchToken = data["token"];
                        //国内SDK直接调用切换帐号接口时，不会收到Logout回调，只有切换帐号回调，因此这里要切换回登录 ps:通过SDK界面切换帐号，则有Logout消息
                        BackToLogin();
                        //UploadDeviceInfo(DeviceUpload.SDKLoginSucc);
                        //切换到登录场景后才收到token时，发起自动登录
                        //Messenger.DispatchEvent(Notif.SDK_SWITCH_SUCCESS);
                    }
                    break;
                case SDKResultType.Pay:
                    SDKPay.gi.SDKPayCallBack(data);
                    break;
                case SDKResultType.ExitGame:
                    if (state == 1)
                    {
                        Application.Quit();
                    }
                    break;
                case SDKResultType.Logout:
                    if (state == 1)
                    {
                        SwitchToken = "";
                        //GameSparks.Core.GS.Reset();
                        BackToLogin();
                    }
                    break;
                case SDKResultType.ConfigInfo:
                    if (state == 1)
                    {
                        PackageParams = (data["appID"], data["cchID"], data["mdID"], data["deviceID"]);
                    }
                    break;
                case SDKResultType.CustomerService:
                    if (state == 1)
                    {
                        
                    }
                    break;
            }
        }
        private void PayCallBack(int type) {
            if (type == 0)
            {
               //支付初始化 返回所有订单信息
               //然后向服务器获得订单信息作对比 
            } else if (type == 1) {
                //正常购买
            }
            else if (type == 2)
            {

            }
        }
        private void BackToLogin()
        {
            if (changingScene)
            {
                return;
            }
            changingScene = true;
            //SceneTransition.gi.ToScene(SceneMgr.LOGIN, completedCallBack: (scene, obj) => {
            //    GameTcpClient.gi.Close();
            //    changingScene = false;
            //});
        }
        
#region 打点
        /// <summary>
        /// 设备阶段打点(上报数据)
        /// </summary>
        /// <param name="uploadType">步骤</param>
        //public void UploadDeviceInfo(DeviceUpload uploadType)
        //{
        //    GetConfigInfo((state, dict) =>
        //    {
        //        if (state == 1)
        //        {
        //            //OpenApiManager.gi.UploadDeviceInfo(uploadType, dict["deviceID"], dict["cchID"]);
        //        }
        //    });
        //}
#endregion
    }

    // 该枚举与SDK接入层定义一致
    public enum SDKUploadType
    {
        RoleCreate,
        RoleEnterGame,
        RoleUpgrade,
        RoleGuideOver,
        RoleRename,
    }
    // 该枚举与SDK接入层定义一致 Android为同名方法 IOS为枚举值 列如SDKResultType.Init 在Android中为“Init” 在IOS为0
    public enum SDKResultType
    {
        Init,
        Login,
        Switch,
        Pay,
        UploadInfo,
        ExitGame,
        Logout,

        ConfigInfo,
        GoogleTranslate,
        Bind,
        Share,
        Naver,
        /// <summary>
        /// AIHelper 客服
        /// </summary>
        CustomerService,
    }

    // SDK类型
    public enum SDKType
    {
        None,
        Native,
        NativeChukai,
        Oversea,
        OverseaELEX,
    }
    public enum SDKLoginType
    {
        None,
        /// <summary>
        /// 星辉SDK
        /// </summary>
        Apple,
        GameCenter,
        FaceBook,
        Google,//新增加请在Google、Rastar之间加入
        Rastar,
    }
}