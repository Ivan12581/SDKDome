package celia.sdk;

// Unity3D

import com.adjust.sdk.Adjust;
import com.elex.girlsthrone.tw.gp.R;
import com.unity3d.player.*;
// Android
import android.content.Intent;

import android.os.Bundle;
import android.widget.Toast;

import java.util.HashMap;

import org.json.JSONException;
import org.json.JSONObject;

public class CeliaActivity extends UnityPlayerActivity {
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
        AdjustEvent(602);

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
        ShowLog("calling method:" + methedID);
        MsgID msgID = MsgID.GetMsgID(methedID);
        switch (msgID) {
            case Init:
                Init();
                break;
            case Login:
                Login(data);
                break;
            case Logout:
                Logout();
                break;
            case Pay:
                Pay(data);
                break;
            case ConsumeGoogleOrder:
                googlePay.Consume(data);
                break;
            case GetDeviceId:
                GetDeviceId();
                break;
            case FaceBookShare:
                faceBookHelper.Share(data);
                break;
            case LineShare:
                lineHelper.Share(data);
                break;
            case CustomerService:
                elvaHelper.Show(data);
                break;
            case FaceBookEvent:
                faceBookHelper.Event(data);
                break;
            case AdjustEvent:
                adjustHelper.CommonEvent(data);
                break;
            default:
                return;
        }
    }
    private final String TAG = "Celia";
    //debug等级，0关闭，1打印，2打印+toast
    private int isDebug = 1;
    private Toast mToast;

    GooglePay googlePay;
    AppleSignIn appleSignIn;
    FaceBookHelper faceBookHelper;
    AdjustHelper adjustHelper;
    ElvaHelper elvaHelper;
    LineHelper lineHelper;
    int CurLoginType = -1;

    public void ShowLog(String msg) {
        if (isDebug == 0) {
            return;
        }
        System.out.println(msg);
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

    public void SendMessageToUnity(int msgID, HashMap<String, String> dataMap) {
        dataMap.put("msgID", String.valueOf(msgID));
        JSONObject jsonObject = new JSONObject(dataMap);
        UnityPlayer.UnitySendMessage("SDKManager", "OnResult", jsonObject.toString());
    }

    public void Init() {
        Utils.getInstance().SetActivity(this);
        faceBookHelper = new FaceBookHelper(this);
        appleSignIn = new AppleSignIn(mUnityPlayer,this );
        googlePay = new GooglePay(this);
        elvaHelper = new ElvaHelper(this);
        adjustHelper = new AdjustHelper(this);
        lineHelper = new LineHelper(this);
        GetDeviceId();
        SendMessageToUnity(MsgID.Init.getCode(), new HashMap<String, String>(){ {put("state","1");} });
        SendMessageToUnity(MsgID.ConfigInfo.getCode(), new HashMap<String, String>(){ {
            put("state", "1");
            put("appID", "0");
            put("cchID", "0");
            put("mdID", "0");
            put("sdkVersion","0");
            put("deviceID", "0");
        } });

    }
//region 基础SDK接口

    public void Login(String type)
    {
        ShowLog("Login...:" + type);
        adjustHelper.CommonEvent("gvmnef");
        try {
            int loginType = Integer.parseInt(type);
            CurLoginType = loginType;
            if (loginType == 4){//google
                googlePay.Login();
            }else if (loginType == 5){//apple
                appleSignIn.OpenWeb();
            }else if (loginType == 6){//facebook
                faceBookHelper.Login();
            }
        } catch (NumberFormatException e) {
            e.printStackTrace();
        }
    }
    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        ShowLog("code:" + requestCode);
        ShowLog("resultCode:" + resultCode);
        ShowLog("data:" + data);
        if (CurLoginType == 4){//google
            googlePay.onActivityResult(requestCode, resultCode, data);
        }else if (CurLoginType == 5){//apple

        }else if (CurLoginType == 6 ||CurLoginType == -1){//facebook login||share
            faceBookHelper.callbackManager.onActivityResult(requestCode, resultCode, data);
        }
    }

    public void Pay(String msg)
    {
        ShowLog( "Pay...");
        try
        {
            JSONObject jsonObject = new JSONObject(msg);
            googlePay.Purchase(jsonObject.getString("PID"),jsonObject.getString("orderNumber"));
        }catch (JSONException e){
            e.printStackTrace();
        }


    }
    public void Logout()
    {
        if (CurLoginType == 4){//google
            googlePay.Logout();
        }else if (CurLoginType == 5){//apple
            appleSignIn.OpenWeb();
        }else if (CurLoginType == 6){//facebook
            faceBookHelper.Logout();
        }
        ShowLog("Logout..."+CurLoginType);
    }

    public void GetDeviceId(){
        String IMEIDeviceId = Utils.getInstance().getIMEIDeviceId();
        SendMessageToUnity(MsgID.ConfigInfo.getCode(), new HashMap<String, String>(){ {
            put("state", "1");
            put("UUID", IMEIDeviceId);
        } });
//        Utils.getInstance().getKeyHash();
        Utils.getInstance().getCurrencyInfo();
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
        Adjust.onPause();
//        AppEventsLogger.deactivateApp(this);
    }
    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();
        Adjust.onResume();
    }
//endregion

}