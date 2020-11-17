package celia.sdk;

// Unity3D
import com.starjoys.module.share.RastarShareCore;
import com.starjoys.module.share.bean.RastarShareParams;
import com.starjoys.module.share.bean.ShareContentType;
import com.starjoys.module.share.callback.RSShareResultCallback;
import com.starjoys.open.demo.MainActivity;
import com.unity3d.player.*;
// Android
import android.content.Intent;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.Rect;
import android.os.Bundle;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.View;
import android.view.WindowManager;
import android.widget.Toast;
import android.graphics.BitmapFactory;

// SDK
import com.starjoys.msdk.SJoyMSDK;
import com.starjoys.msdk.SJoyMsdkCallback;
import com.starjoys.msdk.model.constant.MsdkConstant;
import com.starjoys.msdk.platform.ysdk.ScreenCaptureDrawable;
import com.starjoys.module.googletranslate.GoogleTranslateListener;
import com.starjoys.module.googletranslate.GoogleTranslateResult;
import com.starjoys.framework.callback.RSResultCallback;
import com.starjoys.msdk.model.bean.AppBean;
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

        WeiboShare(301),
        FaceBookShare(302),
        LineShare(303),

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
        System.out.println(msg);
    }
    private final String TAG = "Celia";
    private String iniFileName = "sjoys_app.ini";
    private String appkey = "";
    private int isDebug = 1;
    private Toast mToast;

    VitalitySender vitalitySender;

    private void showToast(String msg) {
        
        if(isDebug == 0){
            return;
        }
        
        CeliaActivity.this.runOnUiThread(new Runnable() {
            @Override public void run() {
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
            showToast("SDK初始化成功！");
            SendMessageToUnity(MsgID.Init.getCode(), new HashMap<String, String>(){ {put("state","1");} });
        }

        @Override
        public void onInitFail(String message) {
            //SDK初始化失败，游戏可重新调用SDK初始化接口。
            showToast("SDK初始化失败，重新调用SDK初始化接口！");
            SendMessageToUnity(MsgID.Init.getCode(),new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onLoginSuccess(Bundle bundle) {
            //SDK登录成功，登录成功bundle里包含星辉帐号的token与uid两个值。
            //【注意】：
            //1. 获取到token后，游戏用该token通过服务端验证接口获取真实的uid，具体参考服务端接入文档；
            //1. 客户端返回的uid只能作为一个备用值，真实的uid需通过服务端获取，这里提供为了防止网络问题，导致验证超时，从而无法获取uid的问题；
            showToast("SDK登录成功：" + "\nuid：" + bundle.getString("uid") + "\ntoken：" + bundle.getString("token"));
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
        public void onLoginFail(String message) {
            //登录失败有两种情况：
            //1.玩家取消登录：message等于MsdkConstant.CALLBACK_LOGIN_CANCEL
            //2.玩家登录失败：message会提示对应的原因
            //注意：CP不需要提示失败的具体原因，自行处理失败逻辑，如重新登录等
            showToast("SDK登录失败：" + message);
            SendMessageToUnity(MsgID.Login.getCode(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onUserSwitchSuccess(Bundle bundle) {
            //SDK切换帐号成功，切换帐号成功bundle里包含星辉帐号的token与uid两个值。
            //【注意】：
            //1. 获取到token后，游戏用该token通过服务端验证接口获取真实的uid，具体参考服务端接入文档；
            //1. 客户端返回的uid只能作为一个备用值，真实的uid需通过服务端获取，这里提供为了防止网络问题，导致验证超时，从而无法获取uid的问题；
            showToast("SDK切换帐号成功：" + "\nuid：" + bundle.getString("uid") + "\ntoken：" + bundle.getString("token"));
            HashMap<String, String> dataMap = new HashMap<String, String>(){
                {
                    put("state", "1");
                    put("uid", bundle.getString("uid"));
                    put("token", bundle.getString("token"));
                }
            };
            SendMessageToUnity(MsgID.Switch.getCode(), dataMap);
        }

        @Override
        public void onUserSwitchFail(String message) {
            //切换帐号失败有两种情况：
            //1.玩家取消切换：message等于MsdkConstant.CALLBACK_SWITCH_CANCEL
            //2.玩家切换失败：message会提示对应的原因
            //注意：CP不需要提示失败的具体原因，自行处理失败逻辑，如重新登录等
            showToast("SDK切换帐号失败：" + message);
            SendMessageToUnity(MsgID.Switch.getCode(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onPaySuccess(Bundle bundle) {
            //SDK支付成功，游戏发货以服务端回调为准
            showToast("SDK支付成功，请在游戏内发货！");

            SendMessageToUnity(MsgID.Pay.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onPayFail(String message) {
            //SDK支付失败，游戏按需进行处理
            showToast("SDK支付失败！");

            SendMessageToUnity(MsgID.Pay.getCode(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onExitGameSuccess() {
            //退出游戏成功，游戏在此进行退出游戏，销毁游戏资源相关操作。
            showToast("SDK退出成功！");
            SendMessageToUnity(MsgID.ExitGame.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
            CeliaActivity.this.finish();
            System.exit(1);
        }

        @Override
        public void onExitGameFail() {
            //游戏无需处理，继续游戏。
            showToast("SDK退出取消！");
            SendMessageToUnity(MsgID.ExitGame.getCode(), new HashMap<String, String>(){ {put("state", "0");} });
        }

        @Override
        public void onLogoutSuccess() {
            //SDK注销成功，触发：SDK->悬浮球->更多->设置->切换帐号
            //【注意】游戏收到吃回调后，先回调游戏登录界面，再调用SDK切换帐号方法
            showToast("SDK注销成功！");
            SJoyMSDK.getInstance().userSwitch(CeliaActivity.this);
            SendMessageToUnity(MsgID.Logout.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onLogoutFail(String message) {
            //SDK注销失败，游戏无需处理
            showToast("SDK注销失败！");
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
        isDebug = Integer.parseInt(ini.getProperty("debug", "0"));

        vitalitySender = new VitalitySender(this);

        SJoyMSDK.getInstance().doInit(CeliaActivity.this, appkey, mSJoyMsdkCallback);
    }
//region 基础SDK接口

    public void Login()
    {
        showToast("Login...");
        SJoyMSDK.getInstance().userLogin(CeliaActivity.this);
    }
    public void Switch()
    {
        showToast("Switch...");
        SJoyMSDK.getInstance().userSwitch(CeliaActivity.this);
    }
    public void Pay(String jsonString)
    {
        showToast("Pay...");
        try{
            JSONObject jsonObject = new JSONObject(jsonString);
            HashMap<String, String> payInfo = new HashMap<String, String>();
            //充值金额，单位：元
            //payInfos2.put(MsdkConstant.PAY_MONEY, "0");// 不定额支付
            payInfo.put(MsdkConstant.PAY_MONEY, jsonObject.getString("PayMoney"));
            //CP订单号（不得超过32个字符），全局唯一，不可重复
            payInfo.put(MsdkConstant.PAY_ORDER_NO, jsonObject.getString("OrderID"));
            //商品名称
            payInfo.put(MsdkConstant.PAY_ORDER_NAME, jsonObject.getString("GoodsName"));
            //商品拓展数据，服务端支付结果回调，原样返回给游戏
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
        showToast("ExitGame...");
        CeliaActivity.this.runOnUiThread(new Runnable() {
            @Override public void run() {
                SJoyMSDK.getInstance().doExitGame(CeliaActivity.this);
            }
        });
    }

    public void UploadInfo(String jsonData)
    {
        showToast("UploadInfo...");
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
            //玩家余额，数字，默认0
            infos.put(MsdkConstant.SUBMIT_BALANCE, jsonObject.getString("Balance"));
            //玩家VIP等级，数字，默认0
            infos.put(MsdkConstant.SUBMIT_VIP, jsonObject.getString("VIPLevel"));
            //玩家帮派，没有传“无”
            infos.put(MsdkConstant.SUBMIT_PARTYNAME, jsonObject.getString("PartyName"));
            //角色创建时间，单位：秒，获取服务器存储的时间，不可用手机本地时间
            infos.put(MsdkConstant.SUBMIT_TIME_CREATE, jsonObject.getString("CreateTime"));
            //角色升级时间，单位：秒，获取服务器存储的时间，不可用手机本地时间
            infos.put(MsdkConstant.SUBMIT_TIME_LEVELUP, jsonObject.getString("UpgradeTime"));
            //旧角色名称
            infos.put(MsdkConstant.SUBMIT_LAST_ROLE_NAME, jsonObject.getString("OldName"));
            //拓展字段，传旧角色名，默认传""
            infos.put(MsdkConstant.SUBMIT_EXTRA, jsonObject.getString("Extra"));

            //上传类型
            int uploadType = jsonObject.getInt("UploadType");
            CeliaActivity.this.runOnUiThread(new Runnable() {
                @Override public void run() {
                    switch (uploadType)
                    {
                        case 0:// 角色创建
                            SJoyMSDK.getInstance().roleCreate(infos);
                            break;
                        case 1:// 进入游戏
                            SJoyMSDK.getInstance().roleEnterGame(infos);
                            break;
                        case 2:// 角色升级
                            SJoyMSDK.getInstance().roleUpgrade(infos);
                            break;
                        case 3:// 完成新手
                            break;
                        case 4:// 更名
                            SJoyMSDK.getInstance().roleUpdate(infos);
                            break;
                    }
                }
            });
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
   // endregion

    public void GetConfigInfo()
    {
        showToast("appID -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getApp_id() +
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
                }
            });
        }else {
            SendMessageToUnity(MsgID.ConfigInfo.getCode(), new HashMap<String, String>(){ {put("state", "0");} });
        }

    }
    public void CustomerService()
    {
        showToast("CustomerService...");
		//打开独立的客服中心，可以嵌入在游戏设置界面中
		SJoyMSDK.getInstance().openSdkCustomerService(CeliaActivity.this);
    }
    public void Share(String jsonStr){
        try {
            JSONObject json = new JSONObject(jsonStr);
            String text = json.getString("text");
            String imgPath = json.getString("img");
            Bitmap bitmap = BitmapFactory.decodeFile(imgPath);

            RastarShareParams rsp = new RastarShareParams()
                    .setContentType(ShareContentType.IMAGE) //类型（必须）
                    .setTitle(text)//分享的标题（必要）
                    .setDescription(text)//分享的描述（必要）
                    .setImageBit(bitmap);//分享的图片（bitmap） ps: 图片最好不要过大
//                    .setThumb(BitmapFactory.decodeResource(this.getResources(), R.drawable.rsdk_plugin_share_wechat));//缩略图（最好不大于20k）
            /**
             * 分享本地图片
             */
            RastarShareCore.getInstance().oneKeyShare(this, rsp, new RSShareResultCallback() {
                @Override
                public void onSuccess() {
                    System.out.println("分享成功");
                    SendMessageToUnity(MsgID.Share.getCode(), new HashMap<String, String>(){ {put("state", "1");} });
                }

                @Override
                public void onFail(String message) {
                    System.out.println("分享失败");
                    SendMessageToUnity(MsgID.Share.getCode(), new HashMap<String, String>(){ {put("state", "0");} });
                }
            });
        } catch (JSONException e)
        {

        }
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
//endregion
}
