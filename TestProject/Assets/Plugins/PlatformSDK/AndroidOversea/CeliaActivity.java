package celia.sdk;

// Unity3D
import com.unity3d.player.*;
// Android
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.text.TextUtils;
import android.util.Log;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;
// SDK
import com.rastargame.sdk.library.utils.LogUtils;
import com.rastargame.sdk.library.utils.ResourceUtils;
import com.rastargame.sdk.oversea.na.api.RSFunctionViewType;
import com.rastargame.sdk.oversea.na.api.RastarCallback;
import com.rastargame.sdk.oversea.na.api.RastarResult;
import com.rastargame.sdk.oversea.na.api.RastarSDKProxy;
import com.rastargame.sdk.oversea.na.api.StatusCode;
import com.rastargame.sdk.oversea.na.framework.common.SDKConstants;
import com.rastargame.sdk.oversea.na.framework.permission.RSPermissionWrapper;
import com.rastargame.sdk.oversea.na.framework.view.ripple.MaterialRippleLayout;
import com.rastargame.sdk.oversea.na.module.collect.entity.RoleInfo;
import com.rastargame.sdk.oversea.na.module.pay.entity.PayInfo;
import com.rastargame.sdk.oversea.na.share.model.RSShareContent;
import com.rastargame.sdk.oversea.na.share.model.RSShareLinkContent;
import com.rastargame.sdk.oversea.na.share.model.RSSharePhotoContent;
import com.rastargame.sdk.oversea.na.share.model.RSShareVideoContent;
// Java
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.HashMap;
import java.util.Map;
import java.util.Properties;

import org.json.JSONException;
import org.json.JSONObject;

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
    private String iniFileFolder = "rsdk";
    private String iniFileName = "rastar_na_config.ini";
    private String appkey = "";
    private int isDebug = 1;
    private Toast mToast;

    private final int IMAGE_CODE = 0,VIDEO_CODE = 300;
    private boolean isLogin = false,ballShowed = false,naverState = false;

//region 安卓方法
    private void showToast(String msg) {
        if(isDebug == 0){
            return;
        }

        Log.e(TAG, msg);
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

    private String screenshot(Activity activity) {
        View view = activity.getWindow().getDecorView().findViewById(android.R.id.content).getRootView();
        view.setDrawingCacheEnabled(true);
        view.buildDrawingCache(true);

        Bitmap screenshot = view.getDrawingCache(true);

        String filename = "screenshot" + System.currentTimeMillis() + ".png";
        String fileUri = null;
        try {
            File f = new File(activity.getFilesDir(), filename);

            f.createNewFile();
            fileUri = f.toURI().toString();

            OutputStream outStream = new FileOutputStream(f);
            screenshot.compress(Bitmap.CompressFormat.PNG, 100, outStream);
            outStream.close();
        } catch (IOException e) {
            e.printStackTrace();
        }

        view.setDrawingCacheEnabled(false);
        return fileUri;
    }

    private String getAppName() {
        try {
            PackageManager pm = getPackageManager();
            PackageInfo pi = pm.getPackageInfo(getPackageName(), 0);
            return pi == null ? null : pi.applicationInfo.loadLabel(pm).toString();
        } catch (PackageManager.NameNotFoundException e) {
            e.printStackTrace();
            return null;
        }
    }
//endregion

    public void SendMessageToUnity(int msgID, HashMap<String, String> dataMap)
    {
        dataMap.put("msgID", String.valueOf(msgID));
        JSONObject jsonObject = new JSONObject(dataMap);
        UnityPlayer.UnitySendMessage ("SDKManager", "OnResult", jsonObject.toString());
    }

    private RastarCallback rastarCallback = new RastarCallback() {
        @Override public void onResult(RastarResult result) {
            //在此处理SDK所有回调
            switch (result.code) {
                case StatusCode.SDK_INIT_SUCCESS:
                    //SDK初始化成功
                    showToast("Do sdk init success. ==> " + result.toString());
//                    RastarSDKProxy.getInstance().setLiveOpsNotificationIconStyle(CeliaActivity.this, "rastar_sdk_notification_icon", "", -1);

                    SendMessageToUnity(MsgID.Init.ordinal(), new HashMap<String, String>(){ {put("state","1");} });
                    break;
                case StatusCode.SDK_INIT_FAIL:
                    //SDK初始化失败
                    showToast("Do sdk init fail. ==> " + result.toString());

                    SendMessageToUnity(MsgID.Init.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "0");
                            put("message", result.data);
                        }
                    });
                    break;

                case StatusCode.SDK_LOGIN_SUCCESS:
                    //SDK登录成功
                    showToast("Do sdk login success. ==> " + result.toString());
                    isLogin = true;
                    Properties initProperties = new Properties();
                    try {
                        initProperties.load(CeliaActivity.this.getResources().getAssets().open("rsdk/rastar_na_config.ini"));
                        ballShowed = Boolean.parseBoolean(initProperties.getProperty("autoShowFlowBal", "false"));
                    } catch (IOException e) {
                        e.printStackTrace();
                    }

                    try{
                        JSONObject resultData = new JSONObject(result.data);
                        String token = resultData.getString("accessToken");
                        SendMessageToUnity(MsgID.Login.ordinal(), new HashMap<String, String>(){
                            {
                                put("state", "1");
                                put("uid", "");
                                put("token",token);
                            }
                        });
                    }catch (JSONException e){
                        e.printStackTrace();
                    }
                    break;
                case StatusCode.SDK_LOGIN_CANCEL:
                    //SDK登录被取消
                    showToast("Do sdk login cancel. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Login.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "2");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_LOGIN_FAIL:
                    //SDK登录失败
                    showToast("Do sdk login fail. ==> " + result.toString());
                    ballShowed = false;
                    isLogin =false;
                    RastarSDKProxy.getInstance().hideFloatBall();
                    SendMessageToUnity(MsgID.Login.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "0");
                            put("message", result.data);
                        }
                    });
                    break;

                case StatusCode.SDK_SWITCH_ACCOUNT_FAIL:
                    //SDK切换帐号失败
                    showToast("Do sdk switch account fail. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Switch.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "0");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_SWITCH_ACCOUNT_SUCCESS:
                    //SDK切换帐号成功
                    showToast("Do sdk switch account success. ==> " + result.toString());
                    isLogin = true;
                    try{
                        JSONObject resultData = new JSONObject(result.data);
                        SendMessageToUnity(MsgID.Switch.ordinal(), new HashMap<String, String>(){
                            {
                                put("state", "1");
                                put("uid", "");
                                put("token", resultData.getString("accessToken"));
                            }
                        });
                    }catch (JSONException e){
                        e.printStackTrace();
                    }
                    break;
                case StatusCode.SDK_SWITCH_ACCOUNT_CANCEL:
                    //SDK切换帐号被取消
                    showToast("Do sdk switch account cancel. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Switch.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "2");
                            put("message", result.data);
                        }
                    });
                    break;

                case StatusCode.SDK_PAY_FAIL:
                    //SDK支付失败
                    showToast("Do sdk pay fail. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "0");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_PAY_SUCCESS:
                    //SDK支付成功（已付款成功，通知发货成功，消耗商品成功）
                    showToast("Do sdk pay success. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "1");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_PAY_CANCEL:
                    //SDK支付取消
                    showToast("Do sdk pay cancel. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "2");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_PAY_OPERATE_BUSY:
                    //SDK支付繁忙
                    showToast("Do sdk pay busy. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "3");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_PAY_VERIFY_FAILED:
                    // 支付结果码：订单校验失败（已付款成功，玩家会出现收货延迟或者无法收货的情况，游戏可以根据情况给玩家提示）
                    showToast("Do sdk pay failed. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "4");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_PAY_CONSUME_FAILED:
                    // 支付结果码：消耗商品失败（已付款成功，玩家会出现收货延迟的情况，游戏可以根据情况给玩家提示）
                    showToast("Do sdk pay failed. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "5");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_PAY_NOTIFY_DELIVERY_FAILED:
                    // 支付结果码：通知发货失败（已付款成功，玩家会出现收货延迟的情况，游戏可以根据情况给玩家提示）
                    showToast("Do sdk pay failed. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "6");
                            put("message", result.data);
                        }
                    });
                    break;

                case StatusCode.SDK_LOGOUT:
                    //SDK登出
                    showToast("Do sdk logout. ==> " + result.toString());
                    isLogin = false;
                    SendMessageToUnity(MsgID.Logout.ordinal(), new HashMap<String, String>(){ { put("state", "1"); } });
                    break;

                case StatusCode.SDK_EXIT_SUCCESS:
                    //SDK退出游戏成功
                    showToast("Do sdk exit game success. ==> " + result.toString());
                    isLogin = false;
                    SendMessageToUnity(MsgID.ExitGame.ordinal(), new HashMap<String, String>(){ { put("state", "1"); } });
//                    CeliaActivity.this.finish();
//                    System.exit(1);
                    break;

                case StatusCode.SDK_ACCOUNT_BIND_SUCCESS:
                    //SDK账号绑定成功
                    showToast("Do sdk bind account success. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Bind.ordinal(), new HashMap<String, String>(){{ put("state", "1"); }});
                    break;
                case StatusCode.SDK_SHARE_SUCCESS:
                    showToast("share success. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){{ put("state", "1"); }});
                    break;
                case StatusCode.SDK_SHARE_FAILED:
                    showToast("share failed. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "0");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_SHARE_CANCEL:
                    showToast("share canceled. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "2");
                            put("message", result.data);
                        }
                    });
                    break;
                case StatusCode.SDK_SHARE_NEED_INSTALL_FACEBOOK_APP:
                    showToast("Need to install facebook app to share this content.");
//                    SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){
//                        {
//                            put("state", "3");
//                            put("needApp", "0");
//                            put("message", result.toString());
//                        }
//                    });
                    break;
                case StatusCode.SDK_SHARE_NEED_INSTALL_LINE_APP:
                    showToast("Need to install line app to share this content.");
//                    SendMessageToUnity(MsgID.Share.ordinal(), new HashMap<String, String>(){
//                        {
//                            put("state", "3");
//                            put("needApp", "1");
//                            put("message", result.toString());
//                        }
//                    });
                    break;

                case StatusCode.SDK_BBS_NAVER_JOINED:
                    showToast("Naver注册成功. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Naver.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "1");
                            put("message", result.toString());
                        }
                    });
                    break;
                case StatusCode.SDK_BBS_NAVER_POSTED_ARTICLE:
                    showToast("Naver发帖上传成功. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Naver.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "2");
                            put("message", result.toString());
                        }
                    });
                    break;
                case StatusCode.SDK_BBS_NAVER_POSTED_COMMENT:
                    showToast("Naver回帖上传成功. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Naver.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "3");
                            put("message", result.toString());
                        }
                    });
                    break;
                case StatusCode.SDK_BBS_NAVER_SDK_START:
                    showToast("Naver回调开始. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Naver.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "4");
                            put("message", result.toString());
                        }
                    });
                    break;
                case StatusCode.SDK_BBS_NAVER_SDK_STOP:
                    showToast("Naver回调结束.. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Naver.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "5");
                            put("message", result.toString());
                        }
                    });
                    break;
                case StatusCode.SDK_BBS_NAVER_SCREEN_SHOT:
                    showToast("Naver截屏完成. ==> " + result.toString());
                    String path = screenshot(CeliaActivity.this);
                    RastarSDKProxy.getInstance().startNaverImageWrite(CeliaActivity.this, path);
                    SendMessageToUnity(MsgID.Naver.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "6");
                            put("message", result.toString());
                        }
                    });
                    break;
                case StatusCode.SDK_BBS_NAVER_VIDEO_FINISH:
                    showToast("录制视频完成. ==> " + result.toString());
                    SendMessageToUnity(MsgID.Naver.ordinal(), new HashMap<String, String>(){
                        {
                            put("state", "7");
                            put("message", result.toString());
                        }
                    });
                    break;
            }
        }
    };

    public void Init()
    {
        // WRITE_EXTERNAL_STORAGE
        RSPermissionWrapper writeSDCardPermission = new RSPermissionWrapper(android.Manifest.permission.WRITE_EXTERNAL_STORAGE,
                String.format(ResourceUtils.getStringByName("rastar_sdk_permission_storage_desc", this), getAppName()),
                ResourceUtils.getStringByName("rastar_sdk_permission_storage_usage", this), false);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN) {
            // READ_EXTERNAL_STORAGE
            // 从API 16开始才需要对读SDK授权，跟WRITE_EXTERNAL_STORAGE权限是同一个组（申请权限时也是同一个对话框），只说明一个即可
            RSPermissionWrapper readSDCardPermission = new RSPermissionWrapper(android.Manifest.permission.READ_EXTERNAL_STORAGE, "", "", false);
            RastarSDKProxy.getInstance().addGameInitPermissions(readSDCardPermission);
        }

        // READ_PHONE_STATE
        RSPermissionWrapper phoneStatePermission = new RSPermissionWrapper(android.Manifest.permission.READ_PHONE_STATE,
                String.format(ResourceUtils.getStringByName("rastar_sdk_permission_phone_desc", this), getAppName()),
                ResourceUtils.getStringByName("rastar_sdk_permission_phone_usage", this), false);

//        // GET_ACCOUNTS
        RSPermissionWrapper getAccountPermission = new RSPermissionWrapper(android.Manifest.permission.GET_ACCOUNTS,
                String.format(ResourceUtils.getStringByName("rastar_sdk_permission_contact_desc", this), getAppName()),
                ResourceUtils.getStringByName("rastar_sdk_permission_contact_usage", this), false);

        // 权限请求
        // Add custom init permission
        RastarSDKProxy.getInstance().addGameInitPermissions( getAccountPermission, phoneStatePermission, writeSDCardPermission);
        // 获取appkey进行初始化
        Properties ini = new Properties();
        String iniPath = iniFileFolder + File.separator + iniFileName;
        try {
            InputStream iniFile = CeliaActivity.this.getResources().getAssets().open(iniPath);
            ini.load(iniFile);
        } catch (IOException e) {
            ini = null;
        }
        appkey = ini.getProperty("app_key");
        // 设置debug
        isDebug = ini.getProperty("Debug_Switch", "false").compareTo("true") == 0 ? 1 : 0;
        showToast("Init app_key : " + appkey);
        RastarSDKProxy.getInstance().init(this, appkey, rastarCallback);
        // 设置Naver回调
        RastarSDKProxy.getInstance().setNaverCallback(rastarCallback);
    }
    public void Login()
    {
        showToast("Login...");
        RastarSDKProxy.getInstance().userLogin();
    }
    public void Switch()
    {
        showToast("Switch...");
        RastarSDKProxy.getInstance().userSwitchAccount();
    }
    public void Pay(String jsonData)
    {
        showToast("Pay...");
        try{
            JSONObject jsonObject = new JSONObject(jsonData);

            PayInfo payInfo = new PayInfo();
            payInfo.setRoleId(jsonObject.getString("RoleID"));
            payInfo.setRoleName(jsonObject.getString("RoleName"));
            payInfo.setRoleLevel(jsonObject.getString("RoleLevel"));
            payInfo.setServerId(jsonObject.getString("ServerID"));
            payInfo.setServerName(jsonObject.getString("ServerName"));
            payInfo.setMoney(jsonObject.getString("PayMoney"));
            payInfo.setOrderId(jsonObject.getString("OrderID"));
            payInfo.setOrderName(jsonObject.getString("OrderName"));
            payInfo.setGoodsName(jsonObject.getString("GoodsName"));
            payInfo.setGoodsDesc(jsonObject.getString("GoodsDesc"));
            payInfo.setCurrency(jsonObject.getString("MoneySymbol"));    //货币（货币标准符号，例：美元 USD）
            payInfo.setOrderExt(jsonObject.getString("Extra"));// 透传参数
            // if("104".equals(RastarSDKProxy.getInstance().getCCHID())) {
            //     // 小米渠道（需要用真钱测试，所以商品为1分钱人民币）
            //     payInfo.setGoodsName("1分钱"); // app_id 100013小米渠道商品
            //     payInfo.setGoodsDesc("1分钱"); // app_id 100013小米渠道商品
            //     payInfo.setMoney("0.01");
            //     payInfo.setOrderName("1分钱");
            //     payInfo.setCurrency("CNY");    //货币（货币标准符号，CNY 人民币）
            // }
//          // 支付模式（目前三星在用，测试模式下不需要真钱，默认是线上模式，如果配置了此项，上线前必须去掉）
//          payInfo.putIntExtra(SDKConstants.PARAM_PAY_OPERATION_MODE, SDKConstants.PAY_OPERATION_MODE_TEST);
            RastarSDKProxy.getInstance().userPay(payInfo);
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    public void ExitGame()
    {
        showToast("ExitGame...");
        CeliaActivity.this.runOnUiThread(new Runnable() {
            @Override public void run() {
                RastarSDKProxy.getInstance().showExitDialog();
            }
        });
    }
    public void UploadInfo(String jsonData)
    {
        showToast("UploadInfo...");
        try
        {
            JSONObject jsonObject = new JSONObject(jsonData);
            RoleInfo roleInfo = new RoleInfo();
            roleInfo.setRoleId(jsonObject.getString("RoleID"));  //角色ID --必传
            roleInfo.setRoleName(jsonObject.getString("RoleName")); //角色名称 --必传
            roleInfo.setRoleLevel(jsonObject.getString("RoleLevel"));  //角色等级 --必传
            roleInfo.setServerId(jsonObject.getString("ServerID"));  //服务器ID --必传
            roleInfo.setServerName(jsonObject.getString("ServerName"));   //服务器名称 --必传
            roleInfo.setBalance(jsonObject.getString("Balance"));   //账号余额 --非必传 传默认值0
            roleInfo.setVip(jsonObject.getString("VIPLevel")); //vip等级 --非必传 传默认值0
            roleInfo.setPartyName(jsonObject.getString("PartyName")); //公会名称 --非必传 传默认值“无”
            roleInfo.setTimeCreate(-1);   //创建角色时间，单位秒 --非必传 传默认值-1
            roleInfo.setTimeLevelUp(-1);  //等级升级时间，单位秒 --非必传 传默认值-1
            roleInfo.setExtra("extra");  //扩展字段 --非必传 传默认值extra
            //上报类型
            int uploadType = jsonObject.getInt("UploadType");
            switch (uploadType)
            {
                case 0:// 角色创建
                    RastarSDKProxy.getInstance().roleCreate(roleInfo);
                    break;
                case 1:// 进入游戏
                    RastarSDKProxy.getInstance().roleEnterGame(roleInfo);
                    break;
                case 2:// 角色升级
                    RastarSDKProxy.getInstance().roleUpgrade(roleInfo);
                    break;
                case 3:// 完成新手
                    //海外统计SDK的埋点上报接口【默认不需要】
                    //RastarSDKPoxy.getInstance().eventTracking(RastarSDKPoxy.REPORT_TUTORIAL_COMPLETION, null);
                    break;
                case 4:// 更名
                    break;
            }
        }catch (JSONException e){
            e.printStackTrace();
        }
    }


    public void GetConfigInfo()
    {
        showToast("appID -> " + RastarSDKProxy.getInstance().getAppID() +
                "\ncchID -> " + RastarSDKProxy.getInstance().getCCHID() +
                "\n mdID -> " + RastarSDKProxy.getInstance().getMDID() +
                "\nsdkVersion -> " + RastarSDKProxy.getInstance().getSDKVersion());

        SendMessageToUnity(MsgID.ConfigInfo.ordinal(), new HashMap<String, String>(){
            {
                put("state", "1");
                put("appID", RastarSDKProxy.getInstance().getAppID());
                put("cchID", RastarSDKProxy.getInstance().getCCHID());
                put("mdID", RastarSDKProxy.getInstance().getMDID());
                put("sdkVersion", RastarSDKProxy.getInstance().getSDKVersion());
                put("deviceID", RastarSDKProxy.getInstance().getSDKDeviceId());
            }
        });
    }
    public void OpenServiceCenter()// 打开客服中心
    {
        RastarSDKProxy.getInstance().openServiceCenter(CeliaActivity.this);
    }
    public void SwitchBall(String jsonString)// 悬浮球开关
    {
        if (ballShowed){
            RastarSDKProxy.getInstance().hideFloatBall();
            ballShowed = false;
        }else {
            RastarSDKProxy.getInstance().showFloatBall();
            ballShowed = true;
        }
    }
    public void PushMsg(String jsonData)// 消息推送
    {
        try{
            JSONObject jsonObject = new JSONObject(jsonData);
            int delaySecond = Integer.parseInt(jsonObject.getString("delaySecond"));
            String message = jsonObject.getString("message");
            int eventID = Integer.parseInt(jsonObject.getString("eventID"));
            boolean pushOnPlaying = Integer.parseInt(jsonObject.getString("pushOnPlaying")) == 1;
//            RastarSDKProxy.getInstance().setLiveOpsNormalClientPushEvent(this,
//                    delaySecond,  // Delay seconds. 设置为多少秒后发推送的事项
//                    message, // 想要传送的推送内容。
//                    eventID,  // Event ID, 为了取消时使用的值。
//                    pushOnPlaying    // 应用在运行中是否显示推送信息。
//            );
        }catch (JSONException e){
            e.printStackTrace();
        }
    }

    // Naver相关功能
    public void NaverFunc(String jsonData)
    {
        switch ("") {
            case "R.id.rs_demo_naver_home_btn":
                RastarSDKProxy.getInstance().startNaverHome(this);
                break;
            case "R.id.rs_demo_naver_stop_btn":
                RastarSDKProxy.getInstance().stopNaver(this);
                break;
            case "R.id.rs_demo_naver_wight_btn":
                if (!naverState){
                    RastarSDKProxy.getInstance().startNaverWidget(this);
                    showToast("关闭悬浮窗");
                    naverState = true;
                }else {
                    RastarSDKProxy.getInstance().stopNaverWidget(this);
                    showToast("打开悬浮窗");
                    naverState = false;
                }
                break;
            case "R.id.rs_demo_naver_write_btn":// 打开编辑界面
                RastarSDKProxy.getInstance().startNaverWrite(this);
                break;
            case "R.id.rs_demo_naver_image_write_btn":// 打开图片编辑界面
                String imageUri = "file://your_image_path";
                RastarSDKProxy.getInstance().startNaverImageWrite(this,imageUri);
                break;
            case "R.id.rs_demo_naver_video_write_btn":// 打开视频编辑界面
                String videoUri = "file://your_video_path";
                RastarSDKProxy.getInstance().startNaverVideoWrite(this,videoUri);
                break;
        }
    }

// region Google翻译相关
    // 翻译语言到目标语言
    private void TranslateTextByGoogle(String jsonData)
    {
        try{
            JSONObject jsonObject = new JSONObject(jsonData);
            String inputText = jsonObject.getString("inputText");
            String targetLanguage = jsonObject.getString("targetLanguage");
            RastarSDKProxy.getInstance().translateText(inputText, "", targetLanguage, new RastarCallback() {
                @Override public void onResult(@NonNull RastarResult result) {
                    showToast("translateText --> result:" + result);
//                    SendMessageToUnity(MsgID.GoogleTranslate.ordinal(), new HashMap<String, String>(){
//                        {
//                            put("state", "1");
//                            put("supportLanguage", result.data);
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
            RastarSDKProxy.getInstance().detectText(inputText, new RastarCallback() {
                @Override public void onResult(@NonNull RastarResult result) {
                    showToast("textLanguageType --> result:" + result);
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
        RastarSDKProxy.getInstance().supportLanguage(new RastarCallback() {
            @Override public void onResult(@NonNull RastarResult result) {
                showToast("supportTextLanguage --> result:" + result.data);
//                SendMessageToUnity(MsgID.GoogleTranslate.ordinal(), new HashMap<String, String>(){
//                    {
//                        put("state", "1");
//                        put("supportLanguage", result.data);
//                    }
//                });
            }
        });
    }
// endregion

// region 分享相关
    // 分享URL 到Facebook/Line
    // public void ShareUrl(String jsonData)
    // {
    //     try{
    //         JSONObject jsonObject = new JSONObject(jsonData);
    //         String url = jsonObject.getString("url");
    //         RSShareLinkContent shareLinkContent = new RSShareLinkContent.Builder().setLinkUri(Uri.parse(url)).build();
    //         showShareSelectDialog(shareLinkContent, rastarCallback);
    //     }catch (JSONException e){
    //         e.printStackTrace();
    //     }
    // }

    // // 分享图片 到Facebook/Line
    // public void ShareImage(String jsonData)
    // {
    //     // 判断是否有读取SDCard的权限，如果没有权限，进行授权后再做下一步操作
    //     if(Build.VERSION.SDK_INT < Build.VERSION_CODES.JELLY_BEAN) {
    //         Intent imageIntent = new Intent(Intent.ACTION_GET_CONTENT).setType("image/*");
    //         startActivityForResult(imageIntent, IMAGE_CODE);
    //     } else {
    //         RSPermissionWrapper storagePermission = new RSPermissionWrapper(android.Manifest.permission.READ_EXTERNAL_STORAGE,
    //                 "允许海外Demo访问您设备上的照片、媒体内容和文件",
    //                 "用于分享功能读取本地图片", true);
    //         RastarSDKProxy.getInstance().checkSelfPermissions(this, new RastarCallback() {
    //             @Override public void onResult(@NonNull RastarResult result) {
    //                 switch (result.code) {
    //                     case StatusCode.SDK_REQUEST_PERMISSION_SUCCESS:
    //                         Intent imageIntent = new Intent(Intent.ACTION_GET_CONTENT).setType("image/*");
    //                         startActivityForResult(imageIntent, IMAGE_CODE);// 调用onActivityResult
    //                         break;
    //                     case StatusCode.SDK_REQUEST_PERMISSION_DENY:
    //                     default:
    //                         showToast(String.format("Share to facebook need permission '%s'", android.Manifest.permission.READ_EXTERNAL_STORAGE));
    //                         break;
    //                 }
    //             }
    //         }, storagePermission);
    //     }
    // }

    // // 分享视频 到Facebook/Line
    // public void ShareVideo(String jsonData)
    // {
    //     // 判断是否有读取SDCard的权限，如果没有权限，进行授权后再做下一步操作
    //     if(Build.VERSION.SDK_INT < Build.VERSION_CODES.JELLY_BEAN) {
    //         Intent videoIntent = new Intent(Intent.ACTION_GET_CONTENT).setType("video/*");
    //         startActivityForResult(videoIntent, VIDEO_CODE);
    //     } else {
    //         RSPermissionWrapper storagePermission = new RSPermissionWrapper(android.Manifest.permission.READ_EXTERNAL_STORAGE,
    //                 "允许海外Demo访问您设备上的照片、媒体内容和文件",
    //                 "用于分享功能读取本地图片", true);

    //         RastarSDKProxy.getInstance().checkSelfPermissions(this, new RastarCallback() {
    //             @Override public void onResult(@NonNull RastarResult result) {
    //                 switch (result.code) {
    //                     case StatusCode.SDK_REQUEST_PERMISSION_SUCCESS:
    //                         Intent videoIntent = new Intent(Intent.ACTION_GET_CONTENT).setType("video/*");
    //                         startActivityForResult(videoIntent, VIDEO_CODE);
    //                         break;
    //                     case StatusCode.SDK_REQUEST_PERMISSION_DENY:
    //                     default:
    //                         showToast(String.format("Share to facebook need permission '%s'", android.Manifest.permission.READ_EXTERNAL_STORAGE));
    //                         break;
    //                 }
    //             }
    //         }, storagePermission);
    //     }
    // }
// endregion

    // 权限处理
    @Override public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        RastarSDKProxy.getInstance().handleOnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

// region Activiy生命周期
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        Init();
    }

    @Override protected void onStart()
    {
        super.onStart();
        RastarSDKProxy.getInstance().onStart();
    }

    @Override protected void onRestart()
    {
        super.onRestart();
        RastarSDKProxy.getInstance().onRestart();
    }
    @Override protected void onStop()
    {
        super.onStop();
        RastarSDKProxy.getInstance().onStop();
    }
    // Quit Unity
    @Override protected void onDestroy ()
    {
        super.onDestroy();
        RastarSDKProxy.getInstance().onDestroy();
    }
    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();
        RastarSDKProxy.getInstance().onPause();
    }
    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();
        RastarSDKProxy.getInstance().onResume();
    }
    @Override protected void onNewIntent(Intent intent)
    {
        super.setIntent(intent);
        RastarSDKProxy.getInstance().onNewIntent(intent);
    }
    @Override protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if(RastarSDKProxy.getInstance().onActivityResult(requestCode, resultCode, data))
        {
            return;
        }
        if(null == data) {
            return;
        }
        // 获得uri
        Uri originalUri = data.getData();
        if(null == originalUri) {
            return;
        }
        showToast("onActivityResult ==> " + "requestCode -> " + requestCode + "\nresultCode -> " + resultCode + "\ndata -> " + data);
        // switch (requestCode) {
        //     case IMAGE_CODE:
        //         RSSharePhotoContent sharePhotoContent = new RSSharePhotoContent.Builder().setPhotoUri(originalUri).build();
        //         showShareSelectDialog(sharePhotoContent, rastarCallback);
        //         break;
        //     case VIDEO_CODE:
        //         RSShareVideoContent shareVideoContent = new RSShareVideoContent.Builder().setPhotoUri(originalUri).build();
        //         showShareSelectDialog(shareVideoContent, rastarCallback);
        //         break;
        //     default:
        //         break;
        // }
    }
// endregion
    /**
     * 显示分享方式选择对话框
     * @param shareContent
     * @param shareCallback
     */
    // private String [] mmSharePlatforms = new String[] {"分享到Facebook", "分享到Line"};
    // private void showShareSelectDialog(final RSShareContent shareContent, final RastarCallback shareCallback) {
    //     new AlertDialog.Builder(CeliaActivity.this)
    //             .setIcon(android.R.drawable.ic_menu_share)
    //             .setTitle("分享")
    //             .setItems(mmSharePlatforms, new DialogInterface.OnClickListener() {
    //                 @Override
    //                 public void onClick(DialogInterface dialog, int which) {
    //                     if (null == shareContent) {
    //                         return;
    //                     }
    //                     String platform;
    //                     if (0 == which) {
    //                         platform = SDKConstants.CHANNEL_FACEBOOK;
    //                     } else if (1 == which) {
    //                         platform = SDKConstants.CHANNEL_LINE;
    //                     } else {
    //                         return;
    //                     }
    //                     RastarSDKProxy.getInstance().share(CeliaActivity.this, shareContent, platform, shareCallback);
    //                 }
    //             })
    //             .setNegativeButton("取消", new DialogInterface.OnClickListener() {
    //                 @Override
    //                 public void onClick(DialogInterface dialog, int which) {
    //                     dialog.cancel();
    //                 }
    //             })
    //             .create()
    //             .show();
    // }
}
