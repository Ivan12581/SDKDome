package com.elex.girlsthrone.tw.gp;

import com.facebook.AccessToken;
import com.facebook.CallbackManager;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.LoginStatusCallback;
import com.facebook.login.LoginManager;
import com.facebook.login.LoginResult;
import com.unity3d.player.*;
import android.app.Activity;
import android.content.Intent;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.Toast;

import org.json.JSONObject;

import java.util.Arrays;
import java.util.HashMap;

public class MainActivity extends UnityPlayerActivity
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
    private int isDebug = 1;
    private Toast mToast;
    private void showToast(String msg) {
        if(isDebug == 0){
            return;
        }
        MainActivity.this.runOnUiThread(new Runnable() {
            @Override public void run() {
                if (mToast == null) {
                    mToast = Toast.makeText(MainActivity.this, null, Toast.LENGTH_SHORT);
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
    //region 基础SDK接口
    public void Login(String jsonString)
    {
        showToast("Login...");
        int Type = Integer.parseInt(jsonString);
        if (Type == SDKLoginType.FaceBook.ordinal()){
            showToast("....FaceBookLogin.....");
        }
    }
    protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code
    //Facebook 需要接入登陆 分享(应用链接是否接入 具体看策划需求) 广告:Facebook Event中包含统计事件和广告(具体看需求)
    CallbackManager  callbackManager;
    boolean isLoggedIn;
    // Setup activity layout
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        super.onCreate(savedInstanceState);

        mUnityPlayer = new UnityPlayer(this);
        setContentView(mUnityPlayer);
        mUnityPlayer.requestFocus();

        ReginsterFBLoginBack();
    }

    @Override protected void onNewIntent(Intent intent)
    {
        // To support deep linking, we need to make sure that the client can get access to
        // the last sent intent. The clients access this through a JNI api that allows them
        // to get the intent set on launch. To update that after launch we have to manually
        // replace the intent with the one caught here.
        setIntent(intent);
    }
    protected void FBLogin() {
        LoginManager.getInstance().logInWithReadPermissions(MainActivity.this, Arrays.asList("public_profile"));

    }
    //启用快捷登录
    //启用快捷登录功能后，用户可以使用 Facebook 帐户跨设备和平台登录您的应用。
    // 如果用户在 Android 设备上登录了您的应用，然后更换设备，则快捷登录可使用他们的 Facebook 帐户登录，
    // 而不是要求他们选择一个登录方法。这样可避免创建重复的帐户或无法登录
    protected void doQuickLogin() {
        LoginManager.getInstance().retrieveLoginStatus(this, new LoginStatusCallback() {
            @Override
            public void onCompleted(AccessToken accessToken) {
                // User was previously logged in, can log them in directly here.
                // If this callback is called, a popup notification appears that says
                // "Logged in as <User Name>"
            }
            @Override
            public void onFailure() {
                // No access token could be retrieved for the user
            }
            @Override
            public void onError(Exception exception) {
                // An error occurred
            } });
    }
    //注册facebook登录回调
    protected void ReginsterFBLoginBack() {
        callbackManager = CallbackManager.Factory.create();
        LoginManager.getInstance().registerCallback(callbackManager,
                new FacebookCallback<LoginResult>() {
                    @Override
                    public void onSuccess(LoginResult loginResult) {
                        // App code
                        //获取FB返回的uid：loginResult.getAccessToken().getUserId();
                        String Uid = loginResult.getAccessToken().getUserId();
                        String Token = loginResult.getAccessToken().getToken();
                    }

                    @Override
                    public void onCancel() {
                        // App code
                    }

                    @Override
                    public void onError(FacebookException exception) {
                        // App code
                    }
                });
        //检测登陆状态，可选功能。 在官方文档中也有介绍，判断本地是否有登陆缓存，直接登陆
        AccessToken accessToken = AccessToken.getCurrentAccessToken();
        isLoggedIn = accessToken != null && !accessToken.isExpired();

    }
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        callbackManager.onActivityResult(requestCode, resultCode, data);
        super.onActivityResult(requestCode, resultCode, data);
    }

    protected void FBLoginOut(){
        LoginManager.getInstance().logOut();
    }
    // Quit Unity
    @Override protected void onDestroy ()
    {
        mUnityPlayer.destroy();
        super.onDestroy();
    }

    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();
        mUnityPlayer.pause();
    }

    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();
        mUnityPlayer.resume();
    }

    @Override protected void onStart()
    {
        super.onStart();
        mUnityPlayer.start();
    }

    @Override protected void onStop()
    {
        super.onStop();
        mUnityPlayer.stop();
    }

    // Low Memory Unity
    @Override public void onLowMemory()
    {
        super.onLowMemory();
        mUnityPlayer.lowMemory();
    }

    // Trim Memory Unity
    @Override public void onTrimMemory(int level)
    {
        super.onTrimMemory(level);
        if (level == TRIM_MEMORY_RUNNING_CRITICAL)
        {
            mUnityPlayer.lowMemory();
        }
    }

    // This ensures the layout will be correct.
    @Override public void onConfigurationChanged(Configuration newConfig)
    {
        super.onConfigurationChanged(newConfig);
        mUnityPlayer.configurationChanged(newConfig);
    }

    // Notify Unity of the focus change.
    @Override public void onWindowFocusChanged(boolean hasFocus)
    {
        super.onWindowFocusChanged(hasFocus);
        mUnityPlayer.windowFocusChanged(hasFocus);
    }

    // For some reason the multiple keyevent type is not supported by the ndk.
    // Force event injection by overriding dispatchKeyEvent().
    @Override public boolean dispatchKeyEvent(KeyEvent event)
    {
        if (event.getAction() == KeyEvent.ACTION_MULTIPLE)
            return mUnityPlayer.injectEvent(event);
        return super.dispatchKeyEvent(event);
    }

    // Pass any events not handled by (unfocused) views straight to UnityPlayer
    @Override public boolean onKeyUp(int keyCode, KeyEvent event)     { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onKeyDown(int keyCode, KeyEvent event)   { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onTouchEvent(MotionEvent event)          { return mUnityPlayer.injectEvent(event); }
    /*API12*/ public boolean onGenericMotionEvent(MotionEvent event)  { return mUnityPlayer.injectEvent(event); }
}
