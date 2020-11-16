package celia.sdk;

// Unity3D

import com.adjust.sdk.Adjust;
import com.elex.girlsthrone.tw.gp.R;
import com.unity3d.player.*;
// Android
import android.app.AlertDialog;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
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
        ShowLog("CallFromUnity methedID:" + methedID);
        ShowLog("CallFromUnity data:" + data);
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
            case ConfigInfo:
                GetDeviceId();
                break;
             case Share:
                Share(data);
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
            case Purchase3rdEvent:
                adjustHelper.ThirdPurchaseEvent(data);
                faceBookHelper.ThirdPurchaseEvent(data);
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
    VitalitySender vitalitySender;
    int CurLoginType = -1;

    public void ShowLog(String msg) {
        if (isDebug == 0) {
            return;
        }
        Log.d(TAG, msg);
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
        vitalitySender = new VitalitySender(this);
        SendMessageToUnity(MsgID.Init.getCode(), new HashMap<String, String>(){ {put("state","1");} });

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
	public void Share(String jsonStr){
        try {
            JSONObject json = new JSONObject(jsonStr);
            int shareType = json.getInt("type");
            String text = json.getString("text");
            String imgPath = json.getString("img");
            if (shareType == 5){
                faceBookHelper.Share(text,imgPath);
            }else if (shareType == 6){
                lineHelper.Share(text,imgPath);
            }
        } catch (JSONException e)
        {

        }
    }
    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        ShowLog("code:" + requestCode+"  resultCode:"+resultCode+"  data:"+data);
        googlePay.onActivityResult(requestCode, resultCode, data);
        faceBookHelper.callbackManager.onActivityResult(requestCode, resultCode, data);
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

        }else if (CurLoginType == 6){//facebook
            faceBookHelper.Logout();
        }
        ShowLog("Logout..."+CurLoginType);
    }

    public String GetDeviceId(){
        String IMEIDeviceId = adjustHelper.googleAdId;
        if (!Utils.getInstance().isNoEmpty(IMEIDeviceId)){
            IMEIDeviceId = Utils.getInstance().getIMEIDeviceId();
        }
        String finalIMEIDeviceId = IMEIDeviceId;
        SendMessageToUnity(MsgID.ConfigInfo.getCode(), new HashMap<String, String>(){ {
            put("state", "1");
            put("deviceID", finalIMEIDeviceId);
        } });
//        Utils.getInstance().getKeyHash();
        //Utils.getInstance().getCurrencyInfo();
        return finalIMEIDeviceId;
    }


    public void PermissionAlertDialog(){
        SharedPreferences mPreferences = this.getSharedPreferences("ApkConfig",Context.MODE_PRIVATE);
        SharedPreferences.Editor mEditor = mPreferences.edit();
        if (mPreferences.getBoolean("isFirstStart",false)){

        }else {
            mEditor.putBoolean("isFirstStart",true);
            boolean result = mEditor.commit();
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setTitle("權限索取說明");
            builder.setMessage("為了讓您能正常遊戲，請在下一個對話框中同意開放以下權限：\n" +
                    "1.手機儲存空間存取 \n" +
                    "-用於下載、解壓縮資源包和進行遊戲 \n" +
                    "2.電話權限(部分機型)\n" +
                    "-僅用於識別設備碼信息，用於保障遊客帳號數據安全”");
            builder.setPositiveButton("确定", null);
            builder.create().show();
        }
    }

    //region Activity生命周期
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        // 目前SDK必须在onCreate调用，不然会有初始化失败、登录失败的问题
        Init();
		PermissionAlertDialog();
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