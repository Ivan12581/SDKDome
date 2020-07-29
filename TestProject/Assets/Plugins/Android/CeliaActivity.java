package celia.sdk;

// Unity3D
import com.elex.girlsthrone.tw.gp.R;
import com.unity3d.player.*;
// Android
import android.app.Application;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.util.Log;
import android.view.View;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.ImageButton;
import android.widget.Toast;
import android.graphics.BitmapFactory;

// Java
import java.io.IOException;
import java.io.InputStream;
import java.util.HashMap;
import java.io.*;

//google
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInClient;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.tasks.Task;

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

        ConfigInfo(201),
        GoogleTranslate(202),
        Bind(203),
        Share(204),
        Naver(205),

        WeiboShare(301),

        ConsumeGoogleOrder(401);


        MsgID(int code)
        {
            this.code = code;
        }
        private int code;
        public int getCode() {
            return code;
        }
        public  static  MsgID GetMsgID(int code)
        {
            for (MsgID item:MsgID.values())
            {
                if(item.getCode() == code)
                {
                    return  item;
                }
            }
            return  Invalid;
        }
    }

    public  void CallFromUnity(int methedID,String data)
    {
        ShowLog("calling method:" + methedID);
        MsgID msgID = MsgID.GetMsgID(methedID);
        switch (msgID) {
            case Init:
                Init();
                break;
            case Login:
                Login(data);
                break;
            case Switch:
                Switch();
                break;
            case Pay:
                Pay(data);
                break;
            case ConsumeGoogleOrder:
                googlePay.Consume(data);
                break;
            default :
                return;
        }
    }


    private final String TAG = "Celia";
    //debug等级，0关闭，1打印，2打印+toast
    private int isDebug = 1;
    private Toast mToast;

    private GoogleSignInClient mGoogleSignInClient;
    private static final int RC_GET_TOKEN = 9002;

    GooglePay googlePay;
    AppleSignIn appleSignIn;

    public void ShowLog(String msg) {
        if (isDebug == 0) {
            return;
        }
        System.out.println(msg);
        if (isDebug == 1) {
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

    public void Init()
    {
        String server_client_id = Constant.google_server_client_id;
        ShowLog("google_server_client_id" + server_client_id);
        GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
                .requestIdToken(server_client_id)
                .requestEmail()
                .build();
        mGoogleSignInClient = GoogleSignIn.getClient(this, gso);

        SendMessageToUnity(MsgID.Init.getCode(), new HashMap<String, String>(){ {put("state","1");} });
        googlePay = new GooglePay(this);
        appleSignIn = new AppleSignIn(mUnityPlayer,this );

        SendMessageToUnity(MsgID.ConfigInfo.getCode(), new HashMap<String, String>(){ {
            put("state", "1");
            put("appID", "0");
            put("cchID", "0");
            put("mdID", "0");
            put("sdkVersion","0");
            put("deviceID", "0");
        } });
    }


//region login
    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        ShowLog("code:" + requestCode);

       if (requestCode == RC_GET_TOKEN) {
           Task<GoogleSignInAccount> task = GoogleSignIn.getSignedInAccountFromIntent(data);
            handelGetToken(task);
        }
    }

    private void handelGetToken(Task<GoogleSignInAccount> completedTask) {
    try {
        GoogleSignInAccount account = completedTask.getResult(ApiException.class);
        String token = account.getIdToken();
        String id = account.getId();
        SendMessageToUnity(MsgID.Login.getCode(), new HashMap<String, String>(){
            {
                put("state", "1");
                put("uid", account.getId());
                put("token",token);
            }
        });

    } catch (ApiException e) {
        ShowLog("signInResult:failed code=" + e.getStatusCode());
        SendMessageToUnity(MsgID.Login.getCode(), new HashMap<String, String>(){
            {
                put("state", "0");
                put("message", e.getStatusCode() + "");
            }
        });
    }
    }
//endregion

//region 基础SDK接口

    public void Login(String type)
    {
        ShowLog("Login...:" + type);
        if (type == null || type == "Google")
        {
            Intent signInIntent = mGoogleSignInClient.getSignInIntent();
            startActivityForResult(signInIntent, RC_GET_TOKEN);
        }else{
            appleSignIn.OpenWeb();
        }

    }
    public void Switch()
    {
        ShowLog("Switch...");
    }

    public void Pay(String msg)
    {
        ShowLog("Pay...");
        try
        {
            JSONObject jsonObject = new JSONObject(msg);
            googlePay.Purchase(jsonObject.getString("PID"),jsonObject.getString("orderNumber"));
        }catch (JSONException e){
            e.printStackTrace();
        }


    }
    public void ExitGame()
    {
        ShowLog("ExitGame...");
        
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
    }

    @Override protected void onRestart()
    {
        super.onRestart();
    }
    @Override protected void onStop()
    {
        super.onStop();
    }
    // Quit Unity
    @Override protected void onDestroy ()
    {
        super.onDestroy();
    }
    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();
    }
    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();
    }
//endregion

}