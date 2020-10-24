package celia.sdk;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.provider.MediaStore;

import com.facebook.share.model.SharePhoto;
import com.facebook.share.model.SharePhotoContent;
import com.linecorp.linesdk.LineAccessToken;
import com.linecorp.linesdk.LineApiResponse;
import com.linecorp.linesdk.LoginDelegate;
import com.linecorp.linesdk.Scope;
import com.linecorp.linesdk.api.LineApiClient;
import com.linecorp.linesdk.api.LineApiClientBuilder;
import com.linecorp.linesdk.auth.LineAuthenticationParams;
import com.linecorp.linesdk.auth.LineLoginApi;
import com.linecorp.linesdk.auth.LineLoginResult;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.URLEncoder;
import java.util.Arrays;
import java.util.HashMap;

public class LineHelper {
    private CeliaActivity mainActivity;
    private int REQUEST_CODE=1;
    private String lineChannelID = "123";
    private static LineApiClient lineApiClient;
    public LineHelper(CeliaActivity Activity){
        mainActivity = Activity;
        Init();
    }
    public void Init(){


    }
    public void Login(){
        LineApiClientBuilder apiBuilder = new LineApiClientBuilder(mainActivity,lineChannelID);
        lineApiClient = apiBuilder.build();

        Intent loginIntent = null;
        if(checkApkExist(mainActivity,"jp.naver.line.android")){//App-to-App
//            Log.d(tag,"Login-App-to-App");
            loginIntent= LineLoginApi.getLoginIntent(mainActivity,lineChannelID,new LineAuthenticationParams.Builder()
                    .scopes(Arrays.asList(Scope.PROFILE))
                    // .nonce("<a randomly-generated string>") // nonce can be used to improve security
                    .build());
        } else{
            //浏览器中的LINE登录界面
//            Log.d(tag,"Login-web");
            loginIntent=LineLoginApi.getLoginIntentWithoutLineAppAuth(mainActivity,lineChannelID,new LineAuthenticationParams.Builder()
                    .scopes(Arrays.asList(Scope.PROFILE))
                    // .nonce("<a randomly-generated string>") // nonce can be used to improve security
                    .build());
        }
        mainActivity.startActivityForResult(loginIntent,REQUEST_CODE);
    }
    //检查是否安装了app
    public boolean checkApkExist(Context context, String packageName){
        if(packageName==null){
            return false;
        }
        try{
            ApplicationInfo applicationInfo=context.getPackageManager()
                    .getApplicationInfo(packageName, PackageManager.GET_UNINSTALLED_PACKAGES);
            return true;
        }catch (Exception e){
            e.printStackTrace();
            return false;
        }
    }
    public void LoginCallBack(Intent data){
        LineLoginResult result = LineLoginApi.getLoginResultFromIntent(data);

        switch (result.getResponseCode()) {

            case SUCCESS:
                // Login successful
                String user_id=result.getLineProfile().getUserId();
                String user_name=result.getLineProfile().getDisplayName();
                String accessToken = result.getLineCredential().getAccessToken().getTokenString();
                break;

            case CANCEL:
                // Login canceled by user
//                Log.e("ERROR", "LINE Login Canceled by user.");
                break;

            default:
                // Login canceled due to other error
//                Log.e("ERROR", "Login FAILED!");
//                Log.e("ERROR", result.getErrorData().toString());
        }
    }

    public void Share(String jsonStr){
        try {
            JSONObject json = new JSONObject(jsonStr);
//            int shareType = json.getInt("shareType");
            String text = json.getString("text");
            String imgPath = json.getString("img");
            Bitmap bitmap = BitmapFactory.decodeFile(imgPath);

            ComponentName componentName = new ComponentName("jp.naver.line.android","jp.naver.line.android.activity.selectchat.SelectChatActivityLaunchActivity");
            Intent shareIntent = new Intent(Intent.ACTION_SEND);
            Uri uri = Uri.parse(MediaStore.Images.Media.insertImage(mainActivity.getContentResolver(), bitmap, null,null));
            shareIntent.putExtra(Intent.EXTRA_STREAM, uri);
            shareIntent.setType("image/jpeg"); //图片分享
//        shareIntent.setType("text/plain"); // 纯文本
            shareIntent.putExtra(Intent.EXTRA_SUBJECT, text);//分享的标题
            shareIntent.putExtra(Intent.EXTRA_TEXT, text);//分享内容
            shareIntent.setComponent(componentName);//跳到指定APP的Activity
            shareIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            mainActivity.startActivity(Intent.createChooser(shareIntent,text));
            //Line没有分享回调 默认回调成功
            mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Share.getCode(), new HashMap<String, String>(){
                {
                    put("state", "1");
                }
            });
        } catch (JSONException e)
        {
            mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Share.getCode(), new HashMap<String, String>(){
                {
                    put("state", "0");
                }
            });
        }

    }
	public void Share(String text,String imgPath){
		Bitmap bitmap = BitmapFactory.decodeFile(imgPath);
		ComponentName componentName = new ComponentName("jp.naver.line.android","jp.naver.line.android.activity.selectchat.SelectChatActivityLaunchActivity");
		Intent shareIntent = new Intent(Intent.ACTION_SEND);
		Uri uri = Uri.parse(MediaStore.Images.Media.insertImage(mainActivity.getContentResolver(), bitmap, null,null));
		shareIntent.putExtra(Intent.EXTRA_STREAM, uri);
		shareIntent.setType("image/jpeg"); //图片分享
//        shareIntent.setType("text/plain"); // 纯文本
		shareIntent.putExtra(Intent.EXTRA_SUBJECT, text);//分享的标题
		shareIntent.putExtra(Intent.EXTRA_TEXT, text);//分享内容
		shareIntent.setComponent(componentName);//跳到指定APP的Activity
		shareIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
		mainActivity.startActivity(Intent.createChooser(shareIntent,text));
		//Line没有分享回调 默认回调成功
		mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Share.getCode(), new HashMap<String, String>(){
			{
				put("state", "1");
			}
		});
    }
    public void shareToLine(Bitmap bitmap,String content){
        ComponentName componentName = new ComponentName("jp.naver.line.android","jp.naver.line.android.activity.selectchat.SelectChatActivityLaunchActivity");
        Intent shareIntent = new Intent(Intent.ACTION_SEND);
        Uri uri = Uri.parse(MediaStore.Images.Media.insertImage(mainActivity.getContentResolver(), bitmap, null,null));
        shareIntent.putExtra(Intent.EXTRA_STREAM, uri);
         shareIntent.setType("image/jpeg"); //图片分享
//        shareIntent.setType("text/plain"); // 纯文本
        shareIntent.putExtra(Intent.EXTRA_SUBJECT, "");//分享的标题
        shareIntent.putExtra(Intent.EXTRA_TEXT, content);//分享内容
        shareIntent.setComponent(componentName);//跳到指定APP的Activity
        shareIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        mainActivity.startActivity(Intent.createChooser(shareIntent,"content"));
    }
    public void Logout(){
        lineApiClient.logout();

    }
}
