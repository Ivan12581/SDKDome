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
// SDK
import com.third.thirdsdk.framework.api.game.ThirdSDKGameSubmitType;
import com.third.thirdsdk.framework.bean.ThirdSDKGameRoleInfo;
import com.third.thirdsdk.framework.bean.ThirdSDKPayRoleInfo;
import com.third.thirdsdk.framework.bean.ThirdSDKSdkUserInfo;
import com.third.thirdsdk.framework.callback.ThirdSDKUserListener;
import com.third.thirdsdk.framework.uitls.ToastUtils;
import com.third.thirdsdk.sdk.ThirdSDK;
// Java
import java.io.IOException;
import java.io.InputStream;
import java.util.HashMap;
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
    private String iniFileName = "sdk_config.ini";
    private String appkey = "";
    private int isDebug = 1;
    private Toast mToast;

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

    private ThirdSDKUserListener mThirdSDKUserListener = new ThirdSDKUserListener() {
        @Override
        public void onInitSuccess() {
            //SDK初始化成功，收到此回调后，游戏可往下继续进行相关操作。
            //【注意】：对于一些加载耗时比较久的游戏，调用SDK初始化接口后立即进行游戏资源加载相关操作，
            //无需等SDK初始化完成后再加载资源。如出现SDK未初始化完成游戏调用SDK登录情况，SDK内部已做好流程控制，游戏无需担心。
            showToast("SDK初始化成功！");

            SendMessageToUnity(MsgID.Init.ordinal(), new HashMap<String, String>(){ {put("state","1");} });
        }

        @Override
        public void onInitFail(String message) {
            //SDK初始化失败，游戏可重新调用SDK初始化接口。
            showToast("SDK初始化失败，重新调用SDK初始化接口！");
            SendMessageToUnity(MsgID.Init.ordinal(),new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onLoginSuccess(ThirdSDKSdkUserInfo sdkUserInfo) {
            //SDK登录成功，登录成功bundle里包含星辉帐号的token与uid两个值。
            //【注意】：
            //1. 获取到token后，游戏用该token通过服务端验证接口获取真实的uid，具体参考服务端接入文档；
            //1. 客户端返回的uid只能作为一个备用值，真实的uid需通过服务端获取，这里提供为了防止网络问题，导致验证超时，从而无法获取uid的问题；
            showToast("SDK登录成功：" + "\nuid：" + sdkUserInfo.getUid() + "\ntoken：" + sdkUserInfo.getToken());

            HashMap<String, String> dataMap = new HashMap<String, String>(){
                {
                    put("state", "1");
                    put("uid", sdkUserInfo.getUid());
                    put("token", sdkUserInfo.getToken());
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
        public void onLoginCancel() {
            //SDK登录取消,游戏无需处理。
            showToast("SDK登录取消");
        }

        @Override
        public void onSwitchSuccess(ThirdSDKSdkUserInfo sdkUserInfo) {
            //SDK切换帐号成功，切换帐号成功bundle里包含星辉帐号的token与uid两个值。
            //【注意】：
            //1. 获取到token后，游戏用该token通过服务端验证接口获取真实的uid，具体参考服务端接入文档；
            //1. 客户端返回的uid只能作为一个备用值，真实的uid需通过服务端获取，这里提供为了防止网络问题，导致验证超时，从而无法获取uid的问题；
            showToast("SDK切换帐号成功：" + "\nuid：" + sdkUserInfo.getUid() + "\ntoken：" + sdkUserInfo.getToken());

            HashMap<String, String> dataMap = new HashMap<String, String>(){
                {
                    put("state", "1");
                    put("uid", sdkUserInfo.getUid());
                    put("token", sdkUserInfo.getToken());
                }
            };
            SendMessageToUnity(MsgID.Switch.ordinal(), dataMap);
        }

        @Override
        public void onSwitchFail(String message) {
            //SDK切换帐号失败，message中为失败原因具体信息
            //建议游戏收到此回调后，无需提示原因信息给玩家，重新调用SDK切换帐号接口。
            showToast("SDK切换帐号失败：" + message);

            SendMessageToUnity(MsgID.Switch.ordinal(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onSwitchCancel() {
            //SDK切换帐号取消,游戏无需处理。
            showToast( "SDK切换帐号取消");
        }

        @Override
        public void onPayFail(String message) {
            //SDK支付失败，游戏按需进行处理
            showToast("SDK支付失败！");

            SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onPaySuccess(ThirdSDKPayRoleInfo payRoleInfo) {
            //SDK支付成功，游戏发货以服务端回调为准
            showToast("SDK支付成功，请在游戏内发货！");
            SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onPayCancel() {
            //SDK支付取消,游戏无需处理。
            showToast("SDK支付取消");
            SendMessageToUnity(MsgID.Pay.ordinal(), new HashMap<String, String>(){ {put("state", "2");} });
        }

        @Override
        public void onExitSuccess() {
            //退出游戏成功，游戏在此进行退出游戏，销毁游戏资源相关操作。
            showToast("SDK退出成功！");

            SendMessageToUnity(MsgID.ExitGame.ordinal(), new HashMap<String, String>(){ {put("state", "1");} });

            CeliaActivity.this.finish();
            System.exit(1);
        }

        @Override
        public void onExitFail() {
            //游戏无需处理，继续游戏。
            showToast("SDK退出取消！");

            SendMessageToUnity(MsgID.ExitGame.ordinal(), new HashMap<String, String>(){ {put("state", "0");} });
        }

        @Override
        public void onExitCancel() {
            //退出游戏取消,游戏无需处理，继续游戏。
            showToast("SDK退出取消");
        }

        @Override
        public void onLogoutSuccess() {
            //SDK登出成功，触发1：SDK->悬浮球->账号->切换帐号
            //SDK登出成功，触发2：ThirdSDK.getInstance().userSwitch();
            //【注意】游戏收到此回调后，回到游戏登录界面，无需调起登录框
            showToast("SDK注销成功！");
            SendMessageToUnity(MsgID.Logout.ordinal(), new HashMap<String, String>(){ {put("state", "1");} });
        }

        @Override
        public void onLogoutFail(String message) {
            //SDK注销失败，游戏无需处理
            showToast("SDK注销失败！");

            SendMessageToUnity(MsgID.Logout.ordinal(), new HashMap<String, String>(){ {put("state", "0"); put("message", message);} });
        }

        @Override
        public void onSubmitRoleInfo(boolean isSuccessful) {
            //上报游戏角色信息,游戏无需处理。
            showToast("上报游戏角色信息" + (isSuccessful ? "成功！" : "失败！"));
        }
    };

//region 基础SDK接口
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
        showToast("Init app_key : " + appkey);

        ThirdSDK.getInstance().init(CeliaActivity.this, appkey, mThirdSDKUserListener);
    }
    public void Login()
    {
		showToast("Login...");
        ThirdSDK.getInstance().userLogin();
    }
    public void Switch()
    {
		showToast("Switch...");
        ThirdSDK.getInstance().userSwitch();
    }
    public void Pay(String jsonString)
    {
        showToast("Pay...");
        try{
            JSONObject jsonObject = new JSONObject(jsonString);
            ThirdSDKPayRoleInfo payInfo = new ThirdSDKPayRoleInfo()
                .setMoney(jsonObject.getString("PayMoney"))//充值金额，单位：元
                .setCpOrderNo(jsonObject.getString("OrderID"))//CP订单号（不得超过32个字符），全局唯一，不可重复
                .setOrderName(jsonObject.getString("OrderName"))//商品名称
                .setOrderExtra(jsonObject.getString("Extra"))//商品拓展数据（透传参数），服务端支付结果回调，原样返回给游戏
                .setRoleId(jsonObject.getString("RoleID"))//角色ID，数字，不得超过32个字符，必传
                .setRoleName(jsonObject.getString("RoleName"))//角色名称，必传
                .setRoleLevel(jsonObject.getString("RoleLevel"))//角色等级，数字，不得超过32个字符，必传
                .setServerId(jsonObject.getString("ServerID"))//服务器ID，数字，不得超过32个字符，必传
                .setServerName(jsonObject.getString("ServerName"));//服务器名称，必传
            ThirdSDK.getInstance().userPay(payInfo);
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    public void ExitGame()
    {
        showToast("ExitGame...");
        CeliaActivity.this.runOnUiThread(new Runnable() {
            @Override public void run() {
                ThirdSDK.getInstance().userExit();
            }
        });
    }
    public void UploadInfo(String jsonData)
    {
        showToast("UploadInfo...");
        try
        {
            JSONObject jsonObject = new JSONObject(jsonData);

            ThirdSDKGameRoleInfo gameRoleInfo = new ThirdSDKGameRoleInfo();
            gameRoleInfo.setRoleId(jsonObject.getString("RoleID"));//角色ID，数字，不得超过32个字符，必传
            gameRoleInfo.setRoleName(jsonObject.getString("RoleName"));//角色名称，必传
            gameRoleInfo.setRoleLevel(jsonObject.getString("RoleLevel"));//角色等级，数字，不得超过32个字符，必传
            gameRoleInfo.setServerId(jsonObject.getString("ServerID"));//服务器ID，数字，不得超过32个字符，必传
            gameRoleInfo.setServerName(jsonObject.getString("ServerName"));//服务器名称，必传
            gameRoleInfo.setVip(jsonObject.getString("VIPLevel"));//玩家VIP等级，数字，默认0，必传
            gameRoleInfo.setBalance(jsonObject.getString("Balance"));//玩家余额，数字，默认0，必传
            gameRoleInfo.setPartyName(jsonObject.getString("PartyName"));//玩家帮派，没有传“无”，必传
            gameRoleInfo.setTimeCreate(jsonObject.getString("CreateTime"));//角色创建时间，单位：秒，获取服务器存储的时间，不可用手机本地时间，必传
            gameRoleInfo.setTimeLevelUp(jsonObject.getString("UpgradeTime"));//角色升级时间，单位：秒，获取服务器存储的时间，不可用手机本地时间，角色升级时必传
            gameRoleInfo.setLastRoleName(jsonObject.getString("OldName"));//旧角色名称，更新角色信息时必传
            gameRoleInfo.setExtra(jsonObject.getString("Extra"));//扩展信息，是一个自定义的json map，没有传空字符串，如：""

            //上传类型
            int uploadType = jsonObject.getInt("UploadType");
            CeliaActivity.this.runOnUiThread(new Runnable() {
                @Override public void run() {
                    switch (uploadType)
                    {
                        case 0:// 角色创建
                            ThirdSDK.getInstance().submitRoleInfo(gameRoleInfo, ThirdSDKGameSubmitType.ROLE_CREATE);
                            break;
                        case 1:// 进入游戏
                            ThirdSDK.getInstance().submitRoleInfo(gameRoleInfo, ThirdSDKGameSubmitType.ENTER_GAME);
                            break;
                        case 2:// 角色升级
                            ThirdSDK.getInstance().submitRoleInfo(gameRoleInfo, ThirdSDKGameSubmitType.ROLE_LEVELUP);
                            break;
                        case 3:// 完成新手
                            break;
                        case 4:// 更名
                            ThirdSDK.getInstance().submitRoleInfo(gameRoleInfo, ThirdSDKGameSubmitType.ROLE_UPDATE);
                            break;
                    }
                }
            });
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
//endregion

    public void GetConfigInfo()
    {
        showToast("appID -> " + ThirdSDK.getInstance().getSdkInfo().getApp_id() +
                "\ncchID -> " + ThirdSDK.getInstance().getSdkInfo().getCch_id() +
                "\n mdID -> " + ThirdSDK.getInstance().getSdkInfo().getMd_id() +
                "\nsdkVersion -> " + ThirdSDK.getInstance().getSdkInfo().getSdkVersion());

        SendMessageToUnity(MsgID.ConfigInfo.ordinal(), new HashMap<String, String>(){
            {
                put("state", "1");
                put("appID", ThirdSDK.getInstance().getSdkInfo().getApp_id());
                put("cchID", ThirdSDK.getInstance().getSdkInfo().getCch_id());
                put("mdID", ThirdSDK.getInstance().getSdkInfo().getMd_id());
                put("sdkVersion", ThirdSDK.getInstance().getSdkInfo().getSdkVersion());
                put("deviceID", ThirdSDK.getInstance().getSdkDeviceId(CeliaActivity.this));
            }
        });
    }

    public void YYBUploadInfo(String jsonData)
    {
        // 应用宝上传信息
    }

// region Google翻译相关
    // 翻译语言到目标语言
    private void TranslateTextByGoogle(String jsonData) {
    }
    // 检测输入的文本语言类型，返回缩写
    public void DetectTextByGoogle(String jsonData){
    }
    // 查看支持语音
    public void SupportTextLanguage(){
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
        ThirdSDK.getInstance().onStart(CeliaActivity.this);
    }

    @Override protected void onRestart()
    {
        super.onRestart();
        ThirdSDK.getInstance().onReStart(CeliaActivity.this);
    }
    @Override protected void onStop()
    {
        super.onStop();
        ThirdSDK.getInstance().onStop(CeliaActivity.this);
    }
    // Quit Unity
    @Override protected void onDestroy ()
    {
        super.onDestroy();
        ThirdSDK.getInstance().onDestroy(CeliaActivity.this);
    }
    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();
        ThirdSDK.getInstance().onPause(CeliaActivity.this);
    }
    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();
        ThirdSDK.getInstance().onResume(CeliaActivity.this);
    }
    @Override protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        ThirdSDK.getInstance().onActivityResult(CeliaActivity.this, requestCode, resultCode, data);
    }
    @Override protected void onNewIntent(Intent intent)
    {
        super.setIntent(intent);
        ThirdSDK.getInstance().onNewIntent(CeliaActivity.this, intent);
    }
    @Override public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        ThirdSDK.getInstance().onConfigurationChanged(CeliaActivity.this, newConfig);
    }
    @Override public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        ThirdSDK.getInstance().onRequestPermissionsResult(requestCode,permissions,grantResults);
    }
//endregion
}
