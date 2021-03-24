using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

namespace celia.game
{
    public class SDKManager : SingleMonoBehaviour<SDKManager>
    {
        public SDKParams SDKParams;
        public bool IsOversea { get { return SDKParams.SDKType == SDKType.Oversea || SDKParams.SDKType == SDKType.CeliaOversea; } }

        /// <summary>
        /// 是否需要激活码
        /// </summary>
        public bool NeedCode { get { return Application.platform == RuntimePlatform.Android && (SDKParams.SDKType == SDKType.Native || SDKParams.SDKType == SDKType.NativeChukai); } }

        /// <summary>
        /// 渠道Id
        /// </summary>
        public int Pf
        {
            get
            {
                if (SDKParams.MdId == 200000)
                    return (SDKParams.AppId % 10000) * 1000 + SDKParams.CchId;
                else
                    return (SDKParams.AppId % 10000) * 100000 + (SDKParams.MdId % 100000);
            }
        }

        private SDKProxy proxy;
        private Dictionary<SDKResultType, Action<int, Dictionary<string, string>>> callBackDict = new Dictionary<SDKResultType, Action<int, Dictionary<string, string>>>();
        public bool CanLoginWithApple; //Sign In With Apple In IOS need IOS higher than IOS13
        public LoginType loginType;
        public ShareType shareType;
        public string AppleSessionKey;
        public string AppleUserToken;
        public string AppleUserIdentifier;
        public DefaultAccountBindType AcountBindType = DefaultAccountBindType.Null; //服务器注释 LogonType 0.default 1.apple 2.google 3.facebook
        public bool AcountBindSwitch = false; //只有游客才有账号绑定功能
        public bool TouriseRedDot = true; //只有游客绑定红点

        // 切换帐号流程控制相关
        public bool DoingSwitch;
        private bool changingScene;

        private void Awake()
        {
            SDKParams = Resources.Load<SDKParams>("SDKParams");
        }

        public void Init()
        {
            AppleSessionKey = PlayerPrefs.GetString("AppleSessionKey", "");
            AppleUserIdentifier = PlayerPrefs.GetString("AppleUserIdentifier", "");
            AppleUserToken = PlayerPrefs.GetString("AppleUserToken", "");
            //SystemInfo.deviceUniqueIdentifier 在手机删除app后再次安装就会改变 故只做初始化

            if (SDKParams.SDKType == SDKType.None)
            {
                SDKParams.AppId = 101356;
                SDKParams.CchId = 213;
                SDKParams.MdId = 20000;
                SDKParams.DeviceID = SystemInfo.deviceUniqueIdentifier;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                proxy = new SDKWindowsProxy();
#endif
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            proxy = new SDKWindowsProxy();
#elif UNITY_ANDROID
            proxy = new SDKAndroidProxy();
#elif UNITY_IOS
            proxy = new SDKIosProxy();
            //if (SDKParams.SDKType == SDKType.CeliaOversea)
            //{
            //    Messenger.AddEventListener(Notif.LoginAuth_SUCCESS, () =>
            //    {
            //        //登录认证服之后才有用户id
            //        ApplePurchaseProxy.gi.ApplePurchaseInit();
            //    });
            //}
#endif

            InitListener();

            //UploadDeviceInfo(DeviceUpload.IconLaunch);// 此处第一次请求SDK的GetConfigInfo接口，获取PackageParams
        }

        private void InitListener()
        {
            //监听登录，数据上报
            Messenger.AddEventListener<int>(Notif.INHOUSE_LOGIN_INIT_DATA_COMPLETED, (m) =>
            {
                if (SDKManager.gi.SDKParams.SDKType == SDKType.CeliaOversea)
                {//  认证服登陆成功后删除本地存储的AppleUserToken
                    PlayerPrefs.DeleteKey("AppleUserToken");
                    if (!string.IsNullOrEmpty(AppleSessionKey))
                    {
                        PlayerPrefs.SetString("AppleSessionKey", AppleSessionKey);
                    }
#if UNITY_IOS
    ApplePurchaseProxy.gi.GetServerPayInfo();
#endif
                }
            });

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
                if (type != SDKResultType.Login)
                {
                    callBackDict[type] = null;
                }
            }
        }

        /// <summary>
        /// SDK初始化
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void InitSDK(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Init] = callBack;
            proxy?.CallSDKMethod(SDKResultType.Init);
        }

        /// <summary>
        /// 星辉sdk登录
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void RSLogin(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Login] = callBack;
            if (DoingSwitch)
            {
                proxy?.CallSDKMethod(SDKResultType.Switch);
                DoingSwitch = false;
            }
            else
            {
                proxy?.CallSDKMethod(SDKResultType.Login);
            }
        }
        /// <summary>
        /// 星辉sdk登出
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void RSLogout(Action<int, Dictionary<string, string>> callBack = null)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                DoingSwitch = true;
                BackToLogin();
            }
            else
            {
                callBackDict[SDKResultType.Logout] = callBack;
                proxy?.CallSDKMethod(SDKResultType.Logout);
            }
        }
        /// <summary>
        /// Celia sdk登录
        /// </summary>
        /// <param name="loginType"></param>
        /// <param name="callBack"></param>
        public void CeliaLogin(LoginType loginType, Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Login] = callBack;
            proxy?.CallSDKMethod(SDKResultType.Login, ((int)loginType).ToString());
        }
        /// <summary>
        /// Celia sdk登出
        /// </summary>
        /// <param name="loginType"></param>
        /// <param name="callBack"></param>
        public void CeliaLogout(LoginType loginType, Action<int, Dictionary<string, string>> callBack = null)
        {
            if (loginType == LoginType.Apple && Application.platform == RuntimePlatform.Android)
            {
                AppleUserIdentifier = "";
                return;
            }
            callBackDict[SDKResultType.Logout] = callBack;
            proxy?.CallSDKMethod(SDKResultType.Logout, ((int)loginType).ToString());
        }

        /// <summary>
        /// 切换帐号（该接口废弃）
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void Switch(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Switch] = callBack;
            proxy?.CallSDKMethod(SDKResultType.Switch);
        }

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="callBack"></param>
        public void Pay(string jsonString, Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Pay] = callBack;
            proxy?.CallSDKMethod(SDKResultType.Pay, jsonString);
        }

        /// <summary>
        /// 获取SDK、设备信息
        /// </summary>
        public void GetConfigInfo(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.ConfigInfo] = callBack;
            proxy?.CallSDKMethod(SDKResultType.ConfigInfo);
        }

        public void Share(string imgPath, string text, Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.Share] = callBack;
            JObject jObj = new JObject();
            jObj.Add("img", imgPath);
            jObj.Add("text", text);
            jObj.Add("type", (int)shareType);
            proxy?.CallSDKMethod(SDKResultType.Share, jObj.ToString());
        }

        /// <summary>
        /// 消耗Google支付商品
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="callBack"></param>
        public void ConsumeGoogleOrder(string orderNumber, Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.ConsumeGoogleOrder] = callBack;
            proxy?.CallSDKMethod(SDKResultType.ConsumeGoogleOrder, orderNumber);
        }

        /// <summary>
        /// FaceBook统计事件
        /// </summary>
        public void FBEvent(string jsonStr)
        {
            proxy?.CallSDKMethod(SDKResultType.FaceBookEvent, jsonStr);
        }

        /// <summary>
        /// Adjust统计事件
        /// </summary>
        /// <param name="callBack"></param>
        public void ADEvent(string evnetToken)
        {
            JObject jobj = new JObject();
            jobj.Add("evnetToken", evnetToken);//通过配置 还要区分平台
            proxy?.CallSDKMethod(SDKResultType.AdjustEvent, jobj.ToString());
        }

        /// <summary>
        ///第三方MyCard支付统计
        /// </summary>
        /// <param name="price">价格</param>
        /// <param name="currency">币种</param>
        /// <param name="productID">商品ID（后台）</param>
        /// <param name="orderID">订单ID（后台）</param>
        public void Purchase3rdEvent(string price, string currency, string productID, string orderID)
        {
            JObject jobj = new JObject();
            jobj.Add("price", price);
            jobj.Add("currency", currency);
            jobj.Add("productID", productID);
            jobj.Add("orderID", orderID);
            proxy?.CallSDKMethod(SDKResultType.Purchase3rdEvent, jobj.ToString());
        }

        /// <summary>
        /// Elva客服入口
        /// </summary>
        /// <param name="showType">客服界面开启种类</param>
        /// <param name="showConversationFlag">当前界面是否开始人工入口</param>
        public void CustomerService(string showType = "1", string showConversationFlag = "1", string formID = "")
        {
            JObject jobj = new JObject();
            string name = "";
            if (AccountDataService.gi.getAccountData() != null)
            {
                name = AccountDataService.gi.getAccountData().name;
            }
            jobj.Add("playerName", name); //游戏中用户名称。如果拿不到userName，传入空字符串@""，会使用默认昵称"anonymous"
            string ID = "";
            if (AuthProcessor.gi.ID != 0)
            {
                ID = AuthProcessor.gi.ID.ToString();
            }
            jobj.Add("playerUid", ID);   //用户在游戏里的唯一标示id。如果拿不到uid，传入空字符串@""，系统会生成一个唯一设备id
            jobj.Add("ServerID", GameSetting.gi.ip);    //用户所在的服务器编号
            jobj.Add("PlayershowConversationFlag", showConversationFlag); //参数的值是 “0” 或 “1”，标识是否开启人工入口。为 “1” 时，将在机器人客服聊天界面右上角，提供人工客服聊天的入口
            jobj.Add("Type", showType);
            jobj.Add("formID", formID);
            //1.智能客服主界面启动，调用 showElva 方法，启动机器人界面
            //2.展示FAQ列表
            //3.运营主界面启动
            //4.直接进行人工客服聊天
            //5.外部打开反馈表单
            proxy?.CallSDKMethod(SDKResultType.CustomerService, jobj.ToString());
        }

        public void OpenSuggestWindow()
        {
            if (SDKManager.gi.SDKParams.SDKType == SDKType.CeliaOversea)
            {
                CustomerService("5", "1", "59a639c15027487ba447551b1f8d16cb");
            }
            else// 国内打开客服界面
            {
                //星辉SDK客服界面
                proxy?.CallSDKMethod(SDKResultType.CustomerService);
            }
        }

        public void RegisterAndroidNotification(int type, string title, string text, long timeStamp)
        {
            JObject jobj = new JObject();
            jobj.Add("type", type);
            jobj.Add("title", title);
            jobj.Add("text", text);
            jobj.Add("timeStamp", timeStamp);
            proxy?.CallSDKMethod(SDKResultType.RegisterNotification, jobj.ToString());
        }

        public void ClearAndroidNotification()
        {
            proxy?.CallSDKMethod(SDKResultType.ClearNotification);
        }

        /// <summary>
        /// 退出游戏数据
        /// </summary>
        /// <param name="callBack">回调，state+数据字典</param>
        public void ExitGame(Action<int, Dictionary<string, string>> callBack = null)
        {
            callBackDict[SDKResultType.ExitGame] = callBack;
            proxy?.CallSDKMethod(SDKResultType.ExitGame);
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
                    }
                    break;

                case SDKResultType.Login:
                    break;

                case SDKResultType.Switch:
                    break;
                case SDKResultType.Logout:
                    //只有ios才有
                    //Android没有Logout接口但是调用悬浮窗里面的切换账号会有Logout回调  IOS没有switch接口 是用logout和login接口拼接
                    if (state == 1)
                    {
                        BackToLogin();
                    }
                    break;
                case SDKResultType.Pay:
#if UNITY_IOS
                    if (SDKManager.gi.SDKParams.SDKType == SDKType.CeliaOversea)
                    {
                        ApplePurchaseProxy.gi.SDKPayCallBack(state, data);
                    }
#endif
                    break;

                case SDKResultType.ExitGame:
                    if (state == 1)
                    {
                        Application.Quit();
                    }
                    break;



                case SDKResultType.ConfigInfo:
                    if (state == 1)
                    {
                        SDKParams.DeviceID = data["deviceID"];
                        if (SDKParams.SDKType == SDKType.CeliaOversea)
                        {
                            if (Application.platform == RuntimePlatform.IPhonePlayer)
                            {
                                SDKManager.gi.CanLoginWithApple = data["IsHighLevel"].Equals("1");
                            }
                            else
                            {
                                SDKManager.gi.CanLoginWithApple = true;
                            }
                        }
                        if (SDKParams.SDKType == SDKType.Native && Application.platform == RuntimePlatform.Android)
                        {
                            //星辉平台这边因涉及到出渠道包与SDK自更新等操作，所以游戏包内以下参数会动态改变
                            SDKParams.AppId = int.Parse(data["appID"]);
                            SDKParams.CchId = int.Parse(data["cchID"]);
                            SDKParams.MdId = int.Parse(data["mdID"]);
                        }
                    }
                    break;
            }
        }

        private void BackToLogin()
        {
            Debug.Log("返回登陆界面");
        }


    }

    // 该枚举与SDK接入层定义一致
    public enum SDKUploadType
    {
        RoleCreate,
        RoleEnterGame,
        RoleUpgrade,
        //RoleGuideOver,// 海外安卓SDK接口
        //RoleRename,// 国内更名上报，游戏内暂无此功能
    }

    // 该枚举与SDK接入层定义一致
    public enum SDKResultType
    {
        Init = 100,
        Login = 101,
        Switch = 102,
        Pay = 103,
        UploadInfo = 104,
        ExitGame = 105,
        Logout = 106,
        GetDeviceId = 200,
        ConfigInfo = 201,
        GoogleTranslate = 202,
        Bind = 203,
        Share = 204,
        Naver = 205,

        ConsumeGoogleOrder = 401,

        CustomerService = 501,
        FaceBookEvent = 601,
        AdjustEvent = 602,
        Purchase3rdEvent = 603,
        ClearNotification = 701,
        RegisterNotification = 702,
    }

    // SDK类型
    public enum SDKType
    {
        None,
        Native,
        NativeChukai,
        Oversea,
        CeliaOversea,
    }

    public enum ShareType
    {
        WeChat,
        TimeLine,
        Weibo,
        QQ,
        QZone,
        FaceBook,
        Line,
    }
}