package celia.sdk;

// Unity3D
import com.unity3d.player.*;
// Android
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.Rect;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.View;
import android.view.WindowManager;
import android.widget.Toast;
import android.graphics.BitmapFactory;
import android.provider.Settings;
//TPNs
import com.tencent.android.tpush.XGIOperateCallback;
import com.tencent.android.tpush.XGPushConfig;
import com.tencent.android.tpush.XGPushManager;
// SDK
import com.starjoys.msdk.SJoyMSDK;
import com.starjoys.msdk.SJoyMsdkCallback;
import com.starjoys.msdk.model.constant.MsdkConstant;
import com.starjoys.msdk.platform.ysdk.ScreenCaptureDrawable;
import com.starjoys.msdk.model.bean.AppBean;
import com.starjoys.module.googletranslate.GoogleTranslateListener;
import com.starjoys.module.googletranslate.GoogleTranslateResult;
import com.starjoys.module.share.RastarShareCore;
import com.starjoys.module.share.bean.RastarShareParams;
import com.starjoys.module.share.bean.ShareContentType;
import com.starjoys.module.share.bean.SharePlatformType;
import com.starjoys.module.share.callback.RSShareResultCallback;
import com.starjoys.framework.callback.RSResultCallback;
import com.starjoys.open.demo.MainActivity;
import com.xlycs.rastar.R;
// Java
import java.io.IOException;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Properties;


import org.json.JSONException;
import org.json.JSONObject;
public class CeliaActivity extends UnityPlayerActivity
{
    public enum MsgID {
        Invalid(0),
        Init(100),
        Login(101),
        Switch(102),
        Pay(103),
        UploadInfo(104),
        ExitGame(105),
        Logout(106),

        GetDeviceId(200),
        ConfigInfo(201),
        GoogleTranslate(202),
        Bind(203),
        Share(204),
        Naver(205),
        GameOnLine(207),
        ConsumeGoogleOrder(401),

        CustomerService(501),

        FaceBookEvent(601),
        AdjustEvent(602),
        Purchase3rdEvent(603),
        ClearNotification(701),
        RegisterNotification(702);

        MsgID(int code) {
            this.code = code;
        }

        private int code;

        public int getCode() {
            return code;
        }

        public static MsgID GetMsgID(int code) {
            for (MsgID item : MsgID.values()) {
                if (item.getCode() == code) {
                    return item;
                }
            }
            return Invalid;
        }
    }
    public void CallFromUnity(int methedID, String data) throws JSONException {
        ShowLog("CallFromUnity methedID:" + methedID+"   CallFromUnity data:"+data);
        MsgID msgID = MsgID.GetMsgID(methedID);
        switch (msgID) {
            case Init:
                Init();
                break;
            case Login:
                Login();
                break;
            case Logout:
                break;
            case Pay:
                Pay(data);
                break;
            case Switch:
                Switch();
                break;
            case GetDeviceId:
                break;
            case ExitGame:
                ExitGame();
                break;
            case ConfigInfo:
                GetConfigInfo();
                break;
            case Share:
                Share(data);
                break;
            case GameOnLine:
                GameOnLine(data);
                break;
            case UploadInfo:
                UploadInfo(data);
                break;
            case CustomerService:
                CustomerService();
                break;
            case ClearNotification:
                vitalitySender.ClearNotification();
                break;
            case RegisterNotification:
                vitalitySender.RegisterNotification(data);
                break;
            default:
                return;
        }
    }
    public void ShowLog(String msg) {
        if (isDebug == 0) {
            return;
        }
        Log.d(TAG, msg);
        if (isDebug == 1) {
            return;
        }
        CeliaActivity.this.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (mToast == null) {
                    mToast = Toast.makeText(CeliaActivity.this, null, Toast.LENGTH_SHORT);
                    mToast.setText(msg);
                } else {
                    mToast.setText(msg);
                }
                mToast.show();
            }
        });
    }
    private final String TAG = "RastarSDK";
    private String iniFileName = "sjoys_app.ini";
    private String appkey = "";
    //debug等级，0关闭，1打印，2打印+toast
    private int isDebug = 1;
    private Toast mToast;

    VitalitySender vitalitySender;

    public void SendMessageToUnity(int msgID, HashMap<String, String> dataMap)
    {
        dataMap.put("msgID", String.valueOf(msgID));
        JSONObject jsonObject = new JSONObject(dataMap);
        UnityPlayer.UnitySendMessage ("SDKManager", "OnResult", jsonObject.toString());
    }

    private SJoyMsdkCallback mSJoyMsdkCallback = new SJoyMsdkCallback() {
        @Override
        public void onInitSuccess() {
            //SDK初始化成功，收到此回调后，游戏可往下继续进行相关操作。
            //【注意】：对于一些加载耗时比较久的游戏，调用SDK初始化接口后立即进行游戏资源加载相关操作，
            //无需等SDK初始化完成后再加载资源。如出现SDK未初始化完成游戏调用SDK登录情况，SDK内部已做好流程控制，游戏无需担心。
            ShowLog("SDK初始化成功！");
            SendMessageToUnity(MsgID.Init.getCode(), new HashMap<String, String>(){ {put("state","1");} });
        }

        @Override
        public void onInitFail(String message) {
            //SDK初始化失败，游戏可重新调用SDK初始化接口。
            ShowLog("SDK初始化失败，重新调用SDK初始化接口！");
            SendMessageToUnity(MsgID.Init.getCode(),new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onLoginSuccess(Bundle bundle) {
            //SDK登录成功，登录成功bundle里包含星辉帐号的token与uid两个值。
            //【注意】：
            //1. 获取到token后，游戏用该token通过服务端验证接口获取真实的uid，具体参考服务端接入文档；
            //1. 客户端返回的uid只能作为一个备用值，真实的uid需通过服务端获取，这里提供为了防止网络问题，导致验证超时，从而无法获取uid的问题；
            ShowLog("SDK登录成功：" + "\nuid：" + bundle.getString("uid") + "\ntoken：" + bundle.getString("token"));
            HashMap<String, String> dataMap = new HashMap<String, String>(){
                {
                    put("state", "1");
                    put("uid", bundle.getString("uid"));
                    put("token", bundle.getString("token"));
                }
            };
            SendMessageToUnity(MsgID.Login.getCode(), dataMap);
            
            //当用户在客服系统提交游戏异常的提单时回调
            SJoyMSDK.getInstance().setServiceIssueListener(new RSServiceIssueCallback() {
                @Override
                public void onSubmitSuccess(IssueContent issue) {//注意对返回值的判空处理
                    if (issue!=null){
                        // ShowLog("RaStar-onSubmitSuccess"+"issue：\n"
                        //         + "openid:" + issue.getOpenid()/*用户uid*/
                        //         + "\nroleId:" + issue.getRoleId()/*角色id*/
                        //         + "\nquestion_title:" + issue.getIssueTitle()/*问题标题*/
                        //         + "\nquestion_desc:" + issue.getIssueDesc()/*问题描述*/
                        //         + "\nimage_url:" + issue.getImageUrls().toString());/*问题相关图片地址，若无提交则返回空集合*/
                    }
                    }

            });
        }

        @Override
        public void onLoginFail(String message) {
            //登录失败有两种情况：
            //1.玩家取消登录：message等于MsdkConstant.CALLBACK_LOGIN_CANCEL
            //2.玩家登录失败：message会提示对应的原因
            //注意：CP不需要提示失败的具体原因，自行处理失败逻辑，如重新登录等
            ShowLog("SDK登录失败：" + message);
            SendMessageToUnity(MsgID.Login.getCode(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onUserSwitchSuccess(Bundle bundle) {
            //SDK切换帐号成功，切换帐号成功bundle里包含星辉帐号的token与uid两个值。
            //【注意】：
            //1. 获取到token后，游戏用该token通过服务端验证接口获取真实的uid，具体参考服务端接入文档；
            //1. 客户端返回的uid只能作为一个备用值，真实的uid需通过服务端获取，这里提供为了防止网络问题，导致验证超时，从而无法获取uid的问题；
            ShowLog("SDK切换帐号成功：" + "\nuid：" + bundle.getString("uid") + "\ntoken：" + bundle.getString("token"));
            HashMap<String, String> dataMap = new HashMap<String, String>(){
                {
                    put("state", "1");
                    put("uid", bundle.getString("uid"));
                    put("token", bundle.getString("token"));
                }
            };
            SendMessageToUnity(MsgID.Login.getCode(), dataMap);
        }

        @Override
        public void onUserSwitchFail(String message) {
            //切换帐号失败有两种情况：
            //1.玩家取消切换：message等于MsdkConstant.CALLBACK_SWITCH_CANCEL
            //2.玩家切换失败：message会提示对应的原因
            //注意：CP不需要提示失败的具体原因，自行处理失败逻辑，如重新登录等
            ShowLog("SDK切换帐号失败：" + message);
            SendMessageToUnity(MsgID.Login.getCode(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onPaySuccess(Bundle bundle) {
            //SDK支付成功，游戏发货以服务端回调为准
            ShowLog("SDK支付成功，请在游戏内发货！");

            SendMessageToUnity(MsgID.Pay.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onPayFail(String message) {
            //SDK支付失败，游戏按需进行处理
            ShowLog("SDK支付失败！");

            SendMessageToUnity(MsgID.Pay.getCode(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onExitGameSuccess() {
            //退出游戏成功，游戏在此进行退出游戏，销毁游戏资源相关操作。
            ShowLog("SDK退出成功！");
            SendMessageToUnity(MsgID.ExitGame.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
            CeliaActivity.this.finish();
            System.exit(1);
        }

        @Override
        public void onExitGameFail() {
            //游戏无需处理，继续游戏。
            ShowLog("SDK退出取消！");
            SendMessageToUnity(MsgID.ExitGame.getCode(), new HashMap<String, String>(){ {put("state", "0");} });
        }

        @Override
        public void onLogoutSuccess() {
            //SDK注销成功，触发：SDK->悬浮球->更多->设置->切换帐号
            //【注意】游戏收到吃回调后，先回调游戏登录界面，再调用SDK切换帐号方法
            ShowLog("SDK注销成功！");
            // SJoyMSDK.getInstance().userSwitch(CeliaActivity.this);
            SendMessageToUnity(MsgID.Logout.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onLogoutFail(String message) {
            //SDK注销失败，游戏无需处理
            ShowLog("SDK注销失败！");
            SendMessageToUnity(MsgID.Logout.getCode(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }
    };

    public void Init()
    {
        Properties ini = new Properties();
        try {
            InputStream iniFile = CeliaActivity.this.getResources().getAssets().open(iniFileName);
            ini.load(iniFile);
        } catch (IOException e) {
            ini = null;
        }
        appkey = ini.getProperty("app_key");
        // isDebug = Integer.parseInt(ini.getProperty("debug", "0"));

        vitalitySender = new VitalitySender(this);

        SJoyMSDK.getInstance().doInit(CeliaActivity.this, appkey, mSJoyMsdkCallback);
        TPNSInit();
    }
    public void TPNSInit()
    {
        ShowLog("TPNSInit！");
        XGPushConfig.enableDebug(CeliaActivity.this,false);
        XGPushManager.registerPush(CeliaActivity.this, new XGIOperateCallback() {
            @Override
            public void onSuccess(Object data, int flag) {
                //token在设备卸载重装的时候有可能会变
                ShowLog("TPush注册成功，设备token为：" + data);
            }

            @Override
            public void onFail(Object data, int errCode, String msg) {
                ShowLog("TPush注册失败，错误码：" + errCode + ",错误信息：" + msg);
            }
        });
    }
//region 基础SDK接口

    public void Login()
    {
        ShowLog("Login...");
        SJoyMSDK.getInstance().userLogin(CeliaActivity.this);
    }
    public void Switch()
    {
        ShowLog("Switch...");
        SJoyMSDK.getInstance().userSwitch(CeliaActivity.this);
    }
    public void Pay(String jsonString)
    {
        ShowLog("Pay...");
        try{
            JSONObject jsonObject = new JSONObject(jsonString);
            HashMap<String, String> payInfo = new HashMap<String, String>();
            //充值金额，单位：元
            payInfo.put(MsdkConstant.PAY_MONEY, jsonObject.getString("PayMoney"));
            //CP订单号（不得超过32个字符），全局唯一，不可重复
            payInfo.put(MsdkConstant.PAY_ORDER_NO, jsonObject.getString("OrderID"));
            //商品名称
            payInfo.put(MsdkConstant.PAY_ORDER_NAME, jsonObject.getString("GoodsName"));
            //商品拓展数据，服务端支付结果回调。(不接受特殊字符串，请务必传入普通字符串或者使用base64加密)
            payInfo.put(MsdkConstant.PAY_ORDER_EXTRA, jsonObject.getString("Extra"));
            //角色ID，数字，必须大于0不得超过32个字符
            payInfo.put(MsdkConstant.PAY_ROLE_ID, jsonObject.getString("RoleID"));
            //角色名称
            payInfo.put(MsdkConstant.PAY_ROLE_NAME, jsonObject.getString("RoleName"));
            //角色等级，数字，不得超过32个字符
            payInfo.put(MsdkConstant.PAY_ROLE_LEVEL, jsonObject.getString("RoleLevel"));
            //服务器ID，数字，不得超过32个字符
            payInfo.put(MsdkConstant.PAY_SERVER_ID, jsonObject.getString("ServerID"));
            //服务器名称
            payInfo.put(MsdkConstant.PAY_SERVER_NAME, jsonObject.getString("ServerName"));
            SJoyMSDK.getInstance().userPay(CeliaActivity.this, payInfo);
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    public void ExitGame()
    {
        ShowLog("ExitGame...");
        CeliaActivity.this.runOnUiThread(new Runnable() {
            @Override public void run() {
                SJoyMSDK.getInstance().doExitGame(CeliaActivity.this);
            }
        });
    }

    public void UploadInfo(String jsonData)
    {
        ShowLog("UploadInfo...");
        try
        {
            JSONObject jsonObject = new JSONObject(jsonData);
            HashMap<String, String> infos = new HashMap<String, String>();
            //角色ID，数字，不得超过32个字符
            infos.put(MsdkConstant.SUBMIT_ROLE_ID, jsonObject.getString("RoleID"));
            //角色名称
            infos.put(MsdkConstant.SUBMIT_ROLE_NAME, jsonObject.getString("RoleName"));
            //角色等级，数字，不得超过32个字符
            infos.put(MsdkConstant.SUBMIT_ROLE_LEVEL, jsonObject.getString("RoleLevel"));
            //服务器ID，数字，不得超过32个字符
            infos.put(MsdkConstant.SUBMIT_SERVER_ID, jsonObject.getString("ServerID"));
            //服务器名称
            infos.put(MsdkConstant.SUBMIT_SERVER_NAME, jsonObject.getString("ServerName"));
            //角色创建所在的服务器id（非合服后的服务器id)，数字，不得超过32个字符
            infos.put(MsdkConstant.SUBMIT_REAL_SERVER_ID, jsonObject.getString("RealServerID"));
            //服务器名称,角色创建所在的服务器名称（非合服后的服务器名称）
            infos.put(MsdkConstant.SUBMIT_REAL_SERVER_NAME, jsonObject.getString("RealServerName"));
            //玩家余额，数字，默认0
            infos.put(MsdkConstant.SUBMIT_BALANCE, jsonObject.getString("Balance"));
            //玩家VIP等级，数字，默认0
            infos.put(MsdkConstant.SUBMIT_VIP, jsonObject.getString("VIPLevel"));
            //玩家帮派，没有传“无”
            infos.put(MsdkConstant.SUBMIT_PARTYNAME, jsonObject.getString("PartyName"));
            //拓展字段，传旧角色名，默认传""
            infos.put(MsdkConstant.SUBMIT_EXTRA, jsonObject.getString("Extra"));

            //上传类型
            int uploadType = jsonObject.getInt("UploadType");
            CeliaActivity.this.runOnUiThread(new Runnable() {
                @Override public void run() {
                    switch (uploadType)
                    {
                        case 0:// 完成新手
                            break;
                        case 1:// 角色创建
                            //角色创建时间，单位：秒，获取服务器存储的时间，不可用手机本地时间
                            try {
                                infos.put(MsdkConstant.SUBMIT_TIME_CREATE, jsonObject.getString("CreateTime"));
                            } catch (JSONException e) {
                                e.printStackTrace();
                            }
                            SJoyMSDK.getInstance().roleCreate(infos);
                            break;
                        case 2:// 进入游戏
                            try {
                                infos.put(MsdkConstant.SUBMIT_TIME_CREATE, jsonObject.getString("CreateTime"));
                            } catch (JSONException e) {
                                e.printStackTrace();
                            }
                            SJoyMSDK.getInstance().roleEnterGame(infos);
                            break;
                        case 3:// 角色升级
                            try {
                                infos.put(MsdkConstant.SUBMIT_TIME_CREATE, jsonObject.getString("CreateTime"));
                            } catch (JSONException e) {
                                e.printStackTrace();
                            }
                            //角色升级时间，单位：秒，获取服务器存储的时间，不可用手机本地时间
                            try {
                                infos.put(MsdkConstant.SUBMIT_TIME_LEVELUP, jsonObject.getString("UpgradeTime"));
                            } catch (JSONException e) {
                                e.printStackTrace();
                            }
                            SJoyMSDK.getInstance().roleUpgrade(infos);
                            break;
                        case 4:// 更名
                            //旧角色名称
                            try {
                                infos.put(MsdkConstant.SUBMIT_LAST_ROLE_NAME, jsonObject.getString("OldName"));
                            } catch (JSONException e) {
                                e.printStackTrace();
                            }
                            SJoyMSDK.getInstance().roleUpdate(infos);
                            break;
                    }
                }
            });
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    private String isOnline = "1";
    public  void GameOnLine(String jsonStr){
    	if (jsonStr.equals(isOnline)) {
    		return;
    	}
        if (jsonStr.equals("1")){
            //角色在线连接接口，开启SDK长链接（当角色由离线状态，重新上线时调用）
            SJoyMSDK.getInstance().onlineGameRole(CeliaActivity.this);
        }
        if (jsonStr.equals("0")){
            //角色离线连接接口，断开SDK长链接（当角色从在线状态离开游戏或者下线时调用）
            SJoyMSDK.getInstance().offlineGameRole();
        }
        isOnline = jsonStr;
    }
    // endregion

    public void GetConfigInfo()
    {
        ShowLog("appID -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getApp_id() +
                "\ncchID -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getCch_id() +
                "\n mdID -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getMd_id() +
                "\nsdkVersion -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getSdk_version());

        AppBean appBean= SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this);
        if (appBean!=null) {
            SendMessageToUnity(MsgID.ConfigInfo.getCode(), new HashMap<String, String>(){
                {
                    put("state", "1");
                    put("appID", appBean.getApp_id());
                    put("cchID", appBean.getCch_id());
                    put("mdID", appBean.getMd_id());
                    put("sdkVersion",appBean.getSdk_version());
                    put("deviceID", SJoyMSDK.getInstance().getSdkDev(CeliaActivity.this));
                    put("sdkMac", SJoyMSDK.getInstance().getSdkMac(CeliaActivity.this));
                    put("sdkIMEI", SJoyMSDK.getInstance().getSdkIMEI(CeliaActivity.this));
                    put("uid", SJoyMSDK.getInstance().getSdkUserUid());
                    put("adId", Settings.Secure.getString(CeliaActivity.this.getContentResolver(), Settings.Secure.ANDROID_ID));
                }
            });
        }else {
            SendMessageToUnity(MsgID.ConfigInfo.getCode(), new HashMap<String, String>(){ {put("state", "0");} });
        }

    }
    public void CustomerService()
    {
        ShowLog("CustomerService...");
        //打开独立的客服中心，可以嵌入在游戏设置界面中
        SJoyMSDK.getInstance().openSdkCustomerService(CeliaActivity.this);
    }
    public void Share(String jsonStr){
        try {
            JSONObject json = new JSONObject(jsonStr);
            String text = json.getString("text");
            String imgPath = json.getString("img");
            int type = json.getInt("type");
            int[] typeArr = new int[]{3,4,0,1,2}; //Unity转为Android
            //SINA QQ QQZONE WECHAT  WECHAT_FRIEND --->RastarSDK Android
            //WeChat TimeLine Weibo QQ  QZone      --->RastarSDK IOS --->Unity
            SharePlatformType sharePlatformType = SharePlatformType.values()[typeArr[type]];
            Bitmap bitmap = BitmapFactory.decodeFile(imgPath);

            RastarShareParams rsp = new RastarShareParams()
                    .setContentType(ShareContentType.IMAGE) //类型（必须）
                    //平台设置
                    .setmPlatformType(sharePlatformType)
                    .setTitle(text)//分享的标题（必要）
                    .setDescription(text)//分享的描述（必要）
                    .setImageBit(bitmap);//分享的图片（bitmap） ps: 图片最好不要过大
//                    .setThumb(BitmapFactory.decodeResource(this.getResources(), R.drawable.rsdk_plugin_share_wechat));//缩略图（最好不大于20k）
            /**
             * 分享本地图片
             */
            RastarShareCore.getInstance().shareAction(this, rsp, new RSShareResultCallback() {
                @Override
                public void onSuccess() {
                    ShowLog("分享成功");
                    SendMessageToUnity(MsgID.Share.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
                }

                @Override
                public void onFail(String message) {
                    ShowLog("分享失败");
                    SendMessageToUnity(MsgID.Share.getCode(), new HashMap<String, String>(){ {put("state", "0");} });
                }
            });
        } catch (JSONException e)
        {

        }
    }

    private void showWaringDialog() {
        AlertDialog dialog = new AlertDialog.Builder(this)
                .setTitle("警告！")
                .setMessage("游戏内部分功能需要授权才能正常运行，若想体验全部功能请前往设置→应用→少女的王座→权限中手动打开相关权限！")
                .setPositiveButton("前往设置", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        // 调转至设置界面
                        getAppDetailSettingIntent(CeliaActivity.this);
                    }
                }).show();
    }
    /*
     * 跳转到权限设置界面
     */
    private void getAppDetailSettingIntent(Context context){
        Intent intent = new Intent();
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if(Build.VERSION.SDK_INT >= 9){
            intent.setAction("android.settings.APPLICATION_DETAILS_SETTINGS");
            intent.setData(Uri.fromParts("package", getPackageName(), null));
        } else if(Build.VERSION.SDK_INT <= 8){
            intent.setAction(Intent.ACTION_VIEW);
            intent.setClassName("com.android.settings","com.android.settings.InstalledAppDetails");
            intent.putExtra("com.android.settings.ApplicationPkgName", getPackageName());
        }
        startActivity(intent);
    }
    //region Activity生命周期
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        // 目前SDK必须在onCreate调用，不然会有初始化失败、登录失败的问题
        Init();
    }

    @Override protected void onStart()
    {
        super.onStart();
        SJoyMSDK.getInstance().onStart();
    }

    @Override protected void onRestart()
    {
        super.onRestart();
        SJoyMSDK.getInstance().onRestart();
    }
    @Override protected void onStop()
    {
        super.onStop();
        SJoyMSDK.getInstance().onStop();
    }
    // Quit Unity
    @Override protected void onDestroy ()
    {
        super.onDestroy();
        SJoyMSDK.getInstance().onDestroy();
    }
    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();
        SJoyMSDK.getInstance().onPause();
    }
    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();
        SJoyMSDK.getInstance().onResume();
    }
    @Override protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        SJoyMSDK.getInstance().onActivityResult(requestCode, resultCode, data);
    }
    @Override protected void onNewIntent(Intent intent)
    {
        super.setIntent(intent);
        SJoyMSDK.getInstance().onNewIntent(intent);
    }
    @Override public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        SJoyMSDK.getInstance().onConfigurationChanged(newConfig);
    }
    @Override public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        SJoyMSDK.getInstance().onRequestPermissionsResult(requestCode, permissions, grantResults);
    }
    @Override
    public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        SJoyMSDK.getInstance().onWindowFocusChanged(hasFocus);
    }
//endregion
}
