package celia.sdk;

// Unity3D
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
// Java
import java.io.IOException;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Properties;
import java.io.*;
import java.io.FileInputStream;
import java.io.FileNotFoundException;

import org.json.JSONException;
import org.json.JSONObject;

// import com.sina.weibo.sdk.WbSdk;
// import com.sina.weibo.sdk.auth.AuthInfo;
// import com.sina.weibo.sdk.api.ImageObject;
// import com.sina.weibo.sdk.api.TextObject;
// import com.sina.weibo.sdk.api.WebpageObject;
// import com.sina.weibo.sdk.api.WeiboMultiMessage;
// import com.sina.weibo.sdk.share.WbShareCallback;
// import com.sina.weibo.sdk.share.WbShareHandler;
// import com.sina.weibo.sdk.utils.Utility;

/*
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;
import com.tencent.mm.opensdk.modelmsg.WXWebpageObject;
import com.tencent.mm.opensdk.modelmsg.WXMediaMessage;
import com.tencent.mm.opensdk.modelmsg.SendMessageToWX;
*/
public class CeliaActivity extends UnityPlayerActivity
{
    private enum MsgID {
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

    private final String TAG = "Celia";
    private String iniFileName = "sjoys_app.ini";
    private String appkey = "";
    private String wechatApp_ID = "wxf2d4d6ad6a49f086";
    private int isDebug = 1;
    private Toast mToast;

    public static final String KEY_SHARE_TYPE = "key_share_type";
    public static final int SHARE_CLIENT = 1;
    public static final int SHARE_ALL_IN_ONE = 2;
  // private IWXAPI api;
  // private WbShareHandler shareHandler;
  private int mShareType = SHARE_CLIENT;
  public  static final String APP_KEY = "3524867471";
    public  static final String REDIRECT_URL  = "http://www.sina.com";
    public  static final String SCOPE =
            "email,direct_messages_read,direct_messages_write,"
                    + "friendships_groups_read,friendships_groups_write,statuses_to_me_read,"
                    + "follow_app_official_microblog," + "invitation_write";

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

            // cp生成屏幕截屏（该接口必须在SDK 初始化成功之后调用）
            SJoyMSDK.getInstance().setScreenCaptureDrawable(new ScreenCaptureDrawable() {
                @Override
                public Bitmap captureImage() {
                    // 以下截屏逻辑仅作为demo例子参考，实际由cp实现
                    View view = getWindow().getDecorView();
                    view.setDrawingCacheEnabled(true);
                    view.buildDrawingCache();
                    Rect rect = new Rect();
                    view.getWindowVisibleDisplayFrame(rect);
                    int statusBarHeight = rect.top;
                    WindowManager windowManager = getWindowManager();
                    DisplayMetrics outMetrics = new DisplayMetrics();
                    windowManager.getDefaultDisplay().getMetrics(outMetrics);
                    int width = outMetrics.widthPixels;
                    int height = outMetrics.heightPixels;
                    Bitmap bitmap = Bitmap.createBitmap(view.getDrawingCache(), 0, statusBarHeight, width,
                            height - statusBarHeight);
                    view.destroyDrawingCache();
                    view.setDrawingCacheEnabled(false);

                    Log.d(TAG, "截屏成功");
                    return bitmap;
                }
            });

            SendMessageToUnity(MsgID.Init.ordinal(), new HashMap<String, String>(){ {put("state","1");} });
        }

        @Override
        public void onInitFail(String message) {
            //SDK初始化失败，游戏可重新调用SDK初始化接口。
            showToast("SDK初始化失败，重新调用SDK初始化接口！");
            SendMessageToUnity(MsgID.Init.ordinal(),new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
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
            SendMessageToUnity(MsgID.Login.ordinal(), dataMap);
        }

        @Override
        public void onLoginFail(String message) {
            //SDK登录失败，message中为失败原因具体信息
            //建议游戏收到此回调后，无需提示原因信息给玩家，重新调用SDK登录接口。
            showToast("SDK登录失败：" + message);

            SendMessageToUnity(MsgID.Login.ordinal(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
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
            SendMessageToUnity(MsgID.Switch.ordinal(), dataMap);
        }

        @Override
        public void onUserSwitchFail(String message) {
            //SDK切换帐号失败，message中为失败原因具体信息
            //建议游戏收到此回调后，无需提示原因信息给玩家，重新调用SDK切换帐号接口。
            showToast("SDK切换帐号失败：" + message);

            SendMessageToUnity(MsgID.Switch.ordinal(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onPaySuccess(Bundle bundle) {
            //SDK支付成功，游戏发货以服务端回调为准
            showToast("SDK支付成功，请在游戏内发货！");

            SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onPayFail(String message) {
            //SDK支付失败，游戏按需进行处理
            showToast("SDK支付失败！");

            SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onExitGameSuccess() {
            //退出游戏成功，游戏在此进行退出游戏，销毁游戏资源相关操作。
            showToast("SDK退出成功！");

            SendMessageToUnity(MsgID.ExitGame.ordinal(), new HashMap<String, String>(){ {put("state", "1");} });

            CeliaActivity.this.finish();
            System.exit(1);
        }

        @Override
        public void onExitGameFail() {
            //游戏无需处理，继续游戏。
            showToast("SDK退出取消！");

            SendMessageToUnity(MsgID.ExitGame.ordinal(), new HashMap<String, String>(){ {put("state", "0");} });
        }

        @Override
        public void onLogoutSuccess() {
            //SDK注销成功，触发：SDK->悬浮球->更多->设置->切换帐号
            //【注意】游戏收到吃回调后，先回调游戏登录界面，再调用SDK切换帐号方法
            showToast("SDK注销成功！");
            SJoyMSDK.getInstance().userSwitch(CeliaActivity.this);

            SendMessageToUnity(MsgID.Logout.ordinal(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onLogoutFail(String message) {
            //SDK注销失败，游戏无需处理
            showToast("SDK注销失败！");

            SendMessageToUnity(MsgID.Logout.ordinal(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
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

        SJoyMSDK.getInstance().doInit(CeliaActivity.this, appkey, mSJoyMsdkCallback);

        //  wechatApp_ID = ini.getProperty("wechatApp_ID");
        // api = WXAPIFactory.createWXAPI(this, wechatApp_ID, true);
        //api.registerApp(wechatApp_ID);
        // showToast("Init app_key : " + appkey + "wxapp:" + wechatApp_ID);
        // WbSdk.install(this,new AuthInfo(this, APP_KEY, REDIRECT_URL,SCOPE));
        // mShareType = getIntent().getIntExtra(KEY_SHARE_TYPE, SHARE_CLIENT);
        // shareHandler = new WbShareHandler(this);
        // shareHandler.registerApp();
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
            payInfo.put(MsdkConstant.PAY_ORDER_NAME, jsonObject.getString("OrderName"));
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

    @Override
    public void onWbShareSuccess() {
        showToast("分享成功...");
        SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){ {put("state","1");} });
    }

    @Override
    public void onWbShareFail() {
        showToast("分享失败...");
        SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){ {put("state","0");} });
    }

    @Override
    public void onWbShareCancel() {
        showToast("分享取消...");
        SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){ {put("state","0");} });
    }

    public void ShareWeibo(String input,byte[] img)
    {
        System.out.println("share called");
        WeiboMultiMessage weiboMessage = new WeiboMultiMessage();
        weiboMessage.textObject = getTextObj(input);
        weiboMessage.imageObject = getImageObj(img);
        shareHandler.shareMessage(weiboMessage,mShareType == SHARE_CLIENT);
    }

    private TextObject getTextObj(String input) {
        TextObject textObject = new TextObject();
        textObject.text = input;
        textObject.title = "TestTitle";
        //textObject.actionUrl = "http://www.baidu.com";
        return textObject;
    }

    private ImageObject getImageObj(byte[] img) {
        ImageObject imageObject = new ImageObject();
        Bitmap bitmap=BitmapFactory.decodeByteArray(img,0,img.length);
        imageObject.setImageObject(bitmap);
        return imageObject;
    }


    public void GetConfigInfo()
    {
        showToast("appID -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getApp_id() +
                "\ncchID -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getCch_id() +
                "\n mdID -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getMd_id() +
                "\nsdkVersion -> " + SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getSdk_version());

        SendMessageToUnity(MsgID.ConfigInfo.ordinal(), new HashMap<String, String>(){
            {
                put("state", "1");
                put("appID", SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getApp_id());
                put("cchID", SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getCch_id());
                put("mdID", SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getMd_id());
                put("sdkVersion", SJoyMSDK.getInstance().getAppConfig(CeliaActivity.this).getSdk_version());
                put("deviceID", SJoyMSDK.getInstance().getSdkDev(CeliaActivity.this));
            }
        });
    }

    public void YYBUploadInfo(String jsonData)
    {
        // 应用宝上传信息
    }

// region Google翻译相关
    // 翻译语言到目标语言
    private void TranslateTextByGoogle(String jsonData) //
    {
        try{
            JSONObject jsonObject = new JSONObject(jsonData);
            String inputText = jsonObject.getString("inputText");
            String targetLanguage = jsonObject.getString("targetLanguage");
            SJoyMSDK.getInstance().googleTranslate(inputText, "", targetLanguage, new GoogleTranslateListener() {
                @Override public void onResult(GoogleTranslateResult googleTranslateResult) {
                    showToast(googleTranslateResult.getCode() + "\n" + googleTranslateResult.getMsg() + "\n" + googleTranslateResult.getData());
//                    SendMessageToUnity(MsgID.GoogleTranslate.ordinal(), new HashMap<String, String>(){
//                        {
//                            put("state", "1");
//                            put("language", result.data);
//                        }
//                    });
                }
            });
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    // 检测输入的文本语言类型，返回缩写
    public void DetectTextByGoogle(String jsonData){
        try{
            JSONObject jsonObject = new JSONObject(jsonData);
            String inputText = jsonObject.getString("inputText");
            SJoyMSDK.getInstance().googleDetectLanguage(inputText, new GoogleTranslateListener() {
                @Override public void onResult(GoogleTranslateResult googleTranslateResult) {
                    showToast(googleTranslateResult.getCode() + "\n" + googleTranslateResult.getMsg() + "\n" + googleTranslateResult.getData());
//                    SendMessageToUnity(MsgID.GoogleTranslate.ordinal(), new HashMap<String, String>(){
//                        {
//                            put("state", "1");
//                            put("language", result.data);
//                        }
//                    });
                }
            });
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    // 查看支持语音
    public void SupportTextLanguage(){
        SJoyMSDK.getInstance().googleCheckSupportLanguage("", "", new GoogleTranslateListener() {
            @Override
            public void onResult(GoogleTranslateResult googleTranslateResult) {
                showToast(googleTranslateResult.getCode() + "\n" + googleTranslateResult.getMsg() + "\n" + googleTranslateResult.getData());
//                SendMessageToUnity(MsgID.GoogleTranslate.ordinal(), new HashMap<String, String>(){
//                    {
//                        put("state", "1");
//                        put("language", result.data);
//                    }
//                });
            }
        });
    }
// endregion


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
        shareHandler.doResultIntent(data,this);
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
