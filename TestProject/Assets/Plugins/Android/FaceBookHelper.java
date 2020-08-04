package celia.sdk;

import android.content.Intent;
import android.os.Message;

import com.facebook.AccessToken;
import com.facebook.CallbackManager;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.LoginStatusCallback;
import com.facebook.login.LoginManager;
import com.facebook.login.LoginResult;
import com.unity3d.player.UnityPlayer;

import java.util.Arrays;

public class FaceBookHelper {

    CeliaActivity mainActivity;
    UnityPlayer mUnityPlayer;
    public CallbackManager callbackManager;
    public FaceBookHelper(CeliaActivity activity)
    {
        mainActivity = activity;
        Init();
    }

    public  void Init()
    {

        callbackManager = CallbackManager.Factory.create();

        LoginManager.getInstance().registerCallback(callbackManager,
                new FacebookCallback<LoginResult>() {
                    @Override
                    public void onSuccess(LoginResult loginResult) {
                        // App code
                        AccessToken accessToken = loginResult.getAccessToken();
                        String userId = accessToken.getUserId();
                        String token = accessToken.getToken();
                        // TODO：拿到userId和token，传给游戏服务器校验
                        mainActivity.ShowLog("--onSuccess--");
                        mainActivity.ShowLog("---FaceBookHelper userId--->" + userId);
                        mainActivity.ShowLog("---FaceBookHelper token--->" + token);
                    }

                    @Override
                    public void onCancel() {
                        // App code
                        mainActivity.ShowLog("--onCancel--");
                    }

                    @Override
                    public void onError(FacebookException exception) {
                        // App code
                        mainActivity.ShowLog("--onError--");
                    }
                });
    }
//启用快捷登录
    public void authLogin(){

        LoginManager.getInstance().retrieveLoginStatus(mainActivity, new LoginStatusCallback() {
            @Override
            public void onCompleted(AccessToken accessToken) {
                String userId = accessToken.getUserId();
                String token = accessToken.getToken();
                // TODO：拿到userId和token，传给游戏服务器校验
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
            }


        });
    }


    public void Login(){

        AccessToken accessToken = AccessToken.getCurrentAccessToken();
        boolean isLoggedIn = accessToken != null && !accessToken.isExpired();
        mainActivity.ShowLog("--isLoggedIn--"+isLoggedIn);
        if (isLoggedIn){
            String userId = accessToken.getUserId();
            String token = accessToken.getToken();
            // TODO：拿到userId和token，传给游戏服务器校验
            mainActivity.ShowLog("--onSuccess--");
            mainActivity.ShowLog("---FaceBookHelper userId--->" + userId);
            mainActivity.ShowLog("---FaceBookHelper token--->" + token);
            LoginManager.getInstance().logInWithReadPermissions(mainActivity, Arrays.asList("public_profile"));
        }else {
            LoginManager.getInstance().logInWithReadPermissions(mainActivity, Arrays.asList("public_profile"));
        }
    }
    public void Logout(){
        LoginManager.getInstance().logOut();
    }
    public void events(){
//        Bundle parameters = new Bundle();
//        parameters.putString(AppEventsConstants.EVENT_PARAM_CONTENT_ID, roleId);
//        parameters.putInt(AppEventsConstants.EVENT_NAME_ACHIEVED_LEVEL, roleLevel);
//        AppEventsLogger logger = AppEventsLogger.newLogger(this, FACEBOOK_AD_ID);
//        logger.logEvent("Login", parameters);

//        AppEventsLogger logger = AppEventsLogger.newLogger(this, FACEBOOK_AD_ID);
//        logger.logPurchase(BigDecimal.valueOf(orderInfo.getInt("price")), Currency.getInstance(SdkImp.CURRENCY));
    }
}
