package celia.sdk;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;

import com.facebook.AccessToken;
import com.facebook.CallbackManager;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.FacebookSdk;
import com.facebook.LoginStatusCallback;
import com.facebook.appevents.AppEventsConstants;
import com.facebook.appevents.AppEventsLogger;
import com.facebook.login.LoginManager;
import com.facebook.login.LoginResult;
import com.facebook.share.Sharer;
import com.facebook.share.model.ShareLinkContent;
import com.facebook.share.model.SharePhoto;
import com.facebook.share.model.SharePhotoContent;
import com.facebook.share.widget.ShareDialog;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.math.BigDecimal;
import java.util.Arrays;
import java.util.Currency;
import java.util.HashMap;
import java.util.Locale;

public class FaceBookHelper {

    CeliaActivity mainActivity;
    public CallbackManager callbackManager;
    ShareDialog _fbShareDialog;
    AppEventsLogger logger;
    public FaceBookHelper(CeliaActivity activity)
    {
        mainActivity = activity;
        Init();
    }

    public  void Init()
    {
        FacebookSdk.setAutoLogAppEventsEnabled(true);
        FacebookSdk.setAutoInitEnabled(true);
        FacebookSdk.fullyInitialize();
        FacebookSdk.setAdvertiserIDCollectionEnabled(true);

        logger = AppEventsLogger.newLogger(mainActivity);

        //玩家点击应用图标，打开游戏，触发该事件
        logger.logEvent(AppEventsConstants.EVENT_NAME_ACTIVATED_APP);

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
                        mainActivity.ShowLog( "--onSuccess--");
                        mainActivity.ShowLog("---FaceBookHelper userId--->" + userId);
                        mainActivity.ShowLog("---FaceBookHelper token--->" + token);
                        mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                            {
                                put("state", "1");
                                put("uid", userId);
                                put("token",token);
                            }
                        });
                    }

                    @Override
                    public void onCancel() {
                        // App code
                        mainActivity.ShowLog("--onCancel--");
                        mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                            {
                                put("state", "0");
                            }
                        });
                    }

                    @Override
                    public void onError(FacebookException exception) {
                        // App code
                        mainActivity.ShowLog( "--onError--");
                        mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                            {
                                put("state", "0");
                            }
                        });
                    }
                });
        initFBShare();
    }

    public void Login(){
        AccessToken accessToken = AccessToken.getCurrentAccessToken();
        boolean isLoggedIn = accessToken != null && !accessToken.isExpired();
        mainActivity.ShowLog("--isLoggedIn--"+isLoggedIn);
        if (isLoggedIn){
            String userId = accessToken.getUserId();
            String token = accessToken.getToken();
            // TODO：拿到userId和token，传给游戏服务器校验
            mainActivity.ShowLog("---FaceBookHelper userId--->" + userId);
            mainActivity.ShowLog( "---FaceBookHelper token--->" + token);
            mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                {
                    put("state", "1");
                    put("uid", userId);
                    put("token",token);
                }
            });
        }else {
            LoginManager.getInstance().logInWithReadPermissions(mainActivity, Arrays.asList("public_profile"));
        }
    }
    public void Logout(){
        LoginManager.getInstance().logOut();
    }
    //facebook事件统计
    public void Event(String jsonStr){
        try{
            JSONObject jsonObject = new JSONObject(jsonStr);
            String type = jsonObject.getString("type");
            if (type.equals("1")){
                String level = jsonObject.getString("level");
                logAchieveLevelEvent(level);            //玩家通过关卡“1-2”后，触发该事件
            }else if (type.equals("2")){
                String contentData = jsonObject.getString("contentData");
                String contentId = jsonObject.getString("contentId");
                String success = jsonObject.getString("success");
                logCompleteTutorialEvent(contentData,contentId,success);            //玩家通过关卡“1-13”后，触发该事件
            }
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    /**
     * This function assumes logger is an instance of AppEventsLogger and has been
     * created using AppEventsLogger.newLogger() call.
     */
    public void logAchieveLevelEvent (String level) {
        Bundle params = new Bundle();
        params.putString(AppEventsConstants.EVENT_PARAM_LEVEL, level);
        logger.logEvent(AppEventsConstants.EVENT_NAME_ACHIEVED_LEVEL, params);
    }
    /**
     * This function assumes logger is an instance of AppEventsLogger and has been
     * created using AppEventsLogger.newLogger() call.
     */
    public void logCompleteTutorialEvent (String contentData, String contentId, String success) {
        Bundle params = new Bundle();
        params.putString(AppEventsConstants.EVENT_PARAM_CONTENT, contentData);
        params.putString(AppEventsConstants.EVENT_PARAM_CONTENT_ID, contentId);
        params.putInt(AppEventsConstants.EVENT_PARAM_SUCCESS, Integer.parseInt(success));
        logger.logEvent(AppEventsConstants.EVENT_NAME_COMPLETED_TUTORIAL, params);
    }
    public void purchaseEvent(String purchaseAmout,String currencyType,String orderID){
        Bundle params = new Bundle();
        params.putString(AppEventsConstants.EVENT_PARAM_CURRENCY, currencyType);
        params.putString(AppEventsConstants.EVENT_PARAM_CONTENT_TYPE, "product");
        params.putString(AppEventsConstants.EVENT_PARAM_CONTENT_ID, orderID);
        //params.putString(AppEventsConstants.EVENT_PARAM_CONTENT, "[{\"id\": \"1234\", \"quantity\": 2}, {\"id\": \"5678\", \"quantity\": 1}]");
//        Locale.TRADITIONAL_CHINESE

        logger.logPurchase(new BigDecimal(purchaseAmout), Currency.getInstance(Locale.getDefault()),params);//玩家每次成功完成付费购买，触发该事件
    }
//share
    public void Share(String jsonStr){
        doShare(jsonStr);
    }
    private void initFBShare() {
        this._fbShareDialog = new ShareDialog(mainActivity);
        this._fbShareDialog.registerCallback(this.callbackManager, new FacebookCallback<Sharer.Result>() {
            @Override
            public void onSuccess(Sharer.Result result) {

                String code = String.format("cc.game.emit('ON_SHARE_RESULT',{'result':'%d'});",1);
                mainActivity.ShowLog("---facebook 分享成功-->" + code);
            }

            @Override
            public void onCancel() {

                String code = String.format("cc.game.emit('ON_SHARE_RESULT',{'result':'%d'});",2);
                mainActivity.ShowLog("---facebook 分享取消-->"+code);
            }

            @Override
            public void onError(FacebookException error) {
                mainActivity.ShowLog("---facebook 分享失败-->"+error);
                String code = String.format("cc.game.emit('ON_SHARE_RESULT',{'result':'%d'});",3);
            }
        });
    }

    public void doShare(String jsonStr) {
        try {
            JSONObject json = new JSONObject(jsonStr);
            int shareType = json.getInt("shareType");
            String url = json.getString("shareUrl");
            String imgPath = json.getString("imgPath");

            if (shareType == 0) {
                if (ShareDialog.canShow(ShareLinkContent.class)) {
                    ShareLinkContent content = new ShareLinkContent.Builder()
                            .setContentUrl(Uri.parse(url))
                            .build();
                    this._fbShareDialog.show(this.mainActivity, content);
                }
            } else if (shareType == 2) {
                if (ShareDialog.canShow(SharePhotoContent.class)) {
                    File file = new File(imgPath);
                    if (!file.exists()) {
                        mainActivity.ShowLog("---facebook Share IMG file no exists-->");

                        return;
                    }
                    Bitmap img = BitmapFactory.decodeFile(imgPath);
                    SharePhoto photo = new SharePhoto.Builder()
                            .setBitmap(img)
//                            .setCaption("123")
                            .build();
                    SharePhotoContent content = new SharePhotoContent.Builder()
//                            .setContentUrl(Uri.parse(url))
                            .addPhoto(photo)
                            .build();
                    this._fbShareDialog.show(this.mainActivity, content);
                }
            }

        }
        catch (JSONException e)
        {

        }
    }
}
