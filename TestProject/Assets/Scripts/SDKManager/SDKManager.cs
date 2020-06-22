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
        private DelayMethod loginDelay;
        
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
            proxy = new SDKIosProxy();
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
            Debug.Log("SDK OnResult: " + jsonString);

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
            Debug.Log("---SDK初始化---");

            callBackDict[SDKResultType.Init] = callBack;
            if (proxy==null)
            {
                Debug.Log("---proxy==null---");
            }
            proxy?.Init();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void Login(Action<int, Dictionary<string, string>> callBack = null)
        {
            DoingSwitch = false;
            callBackDict[SDKResultType.Login] = (s, v) =>
            {
                if (loginDelay != null)
                {
                    DelayManager.gi.Break(loginDelay);
                }
                callBack?.Invoke(s, v);
            };
            // 根据接入文档，某些SDK没有登录失败回调，需自定义计时器
            loginDelay = DelayManager.gi.Invoke(() =>
            {
                loginDelay = null;
                JObject jObj = new JObject();
                jObj.Add("msgID", (int)SDKResultType.Login);
                jObj.Add("state", 0);
                jObj.Add("message", "login out time");
                OnResult(jObj.ToString());
            }, 8);

            proxy?.Login();
        }

        /// <summary>
        /// 切换帐号
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void Switch(Action<int, Dictionary<string, string>> callBack = null)
        {
            DoingSwitch = true;
            callBackDict[SDKResultType.Switch] = callBack;
            proxy?.Switch();
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
            proxy?.Pay(jsonData);
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
            proxy?.UploadInfo(jobj.ToString());
        }

        /// <summary>
        /// 获取SDK、设备信息
        /// </summary>
        public void GetConfigInfo(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.ConfigInfo] = callBack;
            proxy?.GetConfigInfo();
        }

        /// <summary>
        /// 退出游戏数据
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void ExitGame(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.ExitGame] = callBack;
            proxy?.ExitGame();
        }


        // 通用回调处理
        public void OnResultBack(SDKResultType resultType, int state, Dictionary<string, string> data)
        {
            switch (resultType)
            {
                case SDKResultType.Init:
                    if (state == 0)//初始化失败则重新初始化
                    {
                        InitSDK();
                    }else if(state == 1)
                    {
                        //UploadDeviceInfo(DeviceUpload.SDKInitSucc);
                    }
                    break;
                case SDKResultType.Login:
                    if (loginDelay != null)
                    {
                        DelayManager.gi.Break(loginDelay);
                    }
                    // IOS SDK切换账号时，TOKEN都是从Login成功消息返回，无切换账号成功消息
                    if (state == 1 && DoingSwitch)
                    {
                        SwitchToken = data["token"];
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
    // 该枚举与SDK接入层定义一致
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
    }

    // SDK类型
    public enum SDKType
    {
        None,
        Native,
        NativeChukai,
        Oversea,
    }
}