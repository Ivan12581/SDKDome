package celia.sdk;

import com.adjust.sdk.Adjust;
import com.adjust.sdk.AdjustConfig;
import com.adjust.sdk.AdjustEvent;
import com.adjust.sdk.AdjustEventFailure;
import com.adjust.sdk.AdjustEventSuccess;
import com.adjust.sdk.LogLevel;
import com.adjust.sdk.OnDeviceIdsRead;
import com.adjust.sdk.OnEventTrackingFailedListener;
import com.adjust.sdk.OnEventTrackingSucceededListener;

import org.json.JSONException;
import org.json.JSONObject;

public class AdjustHelper {
    CeliaActivity mainActivity;
    private static final String officialEventToken  = "k08q87";
    private static final String thirdEventToken  = "1sqje0";
    //private static final String totalEventToken  = "k08q87";
    public AdjustHelper(CeliaActivity activity)
    {
        mainActivity = activity;
        Init();
    }
    public String googleAdId;
    private void Init(){
        //String environment = AdjustConfig.ENVIRONMENT_SANDBOX;
        String environment = AdjustConfig.ENVIRONMENT_PRODUCTION;
        AdjustConfig config = new AdjustConfig(mainActivity, Constant.Adjust_AppToken, environment);
        config.setAppSecret(1, 555720783, 475659758, 1930748874, 572648029);
        //config.setLogLevel(LogLevel.VERBOSE);
        config.setLogLevel(LogLevel.SUPRESS);
        // Set event success tracking delegate.
        config.setOnEventTrackingSucceededListener(new OnEventTrackingSucceededListener() {
            @Override
            public void onFinishedEventTrackingSucceeded(AdjustEventSuccess eventSuccessResponseData) {
                mainActivity.ShowLog("---AdjustEventSuccess--->" + eventSuccessResponseData.toString());
            }
        });

// Set event failure tracking delegate.
        config.setOnEventTrackingFailedListener(new OnEventTrackingFailedListener() {
            @Override
            public void onFinishedEventTrackingFailed(AdjustEventFailure eventFailureResponseData) {
                mainActivity.ShowLog("---AdjustEventFailure--->" + eventFailureResponseData.toString());
            }
        });
        config.setEventBufferingEnabled(true);
        config.setSendInBackground(true);
        Adjust.onCreate(config);

        //点击游戏图标，启动游戏后，触发该事件
        event("nsmcez");

        Adjust.getGoogleAdId(mainActivity, new OnDeviceIdsRead() {
            @Override
            public void onGoogleAdIdRead(String _googleAdId) {
                googleAdId = _googleAdId;
                mainActivity.ShowLog("---AdjustHelper googleAdId--->" + googleAdId);
            }
        });
    }

    //in-app onClick
    public void CommonEvent(String jsonStr){
        if (jsonStr == null||jsonStr.isEmpty()){
                return;
        }
        try{
            JSONObject jsonObject = new JSONObject(jsonStr);
            String EventName = jsonObject.getString("evnetToken");
            event(EventName);
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    public void event(String evnetToken){
        AdjustEvent adjustEvent = new AdjustEvent(evnetToken);
        Adjust.trackEvent(adjustEvent);
    }
    //官方支付统计
    public void OfficialPurchaseEvent(String price ,String currency,String orderID){
        if (!Utils.getInstance().isNoEmpty(price)||!Utils.getInstance().isNoEmpty(currency)) {
            return;
        }
        PurchaseEvent(officialEventToken,price,currency,orderID);
        //PurchaseEvent(totalEventToken,price,currency,orderID+"-total");
    }
    //第三方MyCard支付统计
    public void ThirdPurchaseEvent(String jsonStr){
        try{
            JSONObject jsonObject = new JSONObject(jsonStr);
            String price = jsonObject.getString("price");
            String currency = jsonObject.getString("currency");
            String productID = jsonObject.getString("productID");
            String orderID = jsonObject.getString("orderID");
            PurchaseEvent(thirdEventToken,price,currency,orderID);
            //PurchaseEvent(totalEventToken,price,currency,orderID+"-total");
        } catch (JSONException ex) {
            ex.printStackTrace();
        }
    }
    public void PurchaseEvent(String eventToken,String price ,String currency,String orderID){
        if (!Utils.getInstance().isNoEmpty(price)||!Utils.getInstance().isNoEmpty(currency)) {
            return;
        }
        double revenue = Double.parseDouble(price);
        AdjustEvent adjustEvent = new AdjustEvent(eventToken);
        adjustEvent.setRevenue(revenue, currency);
        adjustEvent.setOrderId(orderID);
        Adjust.trackEvent(adjustEvent);
    }
}
