package celia.sdk;

import com.adjust.sdk.Adjust;
import com.adjust.sdk.AdjustConfig;
import com.adjust.sdk.AdjustEvent;
import com.adjust.sdk.LogLevel;
public class AdjustHelper {
    CeliaActivity mainActivity;
    public AdjustHelper(CeliaActivity activity)
    {
        mainActivity = activity;
        Init();
    }

    private void Init(){
        String environment = AdjustConfig.ENVIRONMENT_SANDBOX;
//        String environment = AdjustConfig.ENVIRONMENT_PRODUCTION;
        AdjustConfig config = new AdjustConfig(mainActivity, Constant.Adjust_AppToken, environment);
        config.setAppSecret(1, 555720783, 475659758, 1930748874, 572648029);
        config.setLogLevel(LogLevel.VERBOSE);
//        config.setLogLevel(LogLevel.SUPRESS);
        Adjust.onCreate(config);

        //点击游戏图标，启动游戏后，触发该事件
        CommonEvent("nsmcez");

//        Adjust.getGoogleAdId(mainActivity, new OnDeviceIdsRead() {
//            @Override
//            public void onGoogleAdIdRead(String googleAdId) {
//                mainActivity.ShowLog("---AdjustHelper googleAdId--->" + googleAdId);
//            }
//        });
    }

    //in-app onClick
    public void CommonEvent(String EventName){
        if (EventName == null||EventName.isEmpty()){
                return;
        }
        AdjustEvent adjustEvent = new AdjustEvent(EventName);
        Adjust.trackEvent(adjustEvent);
    }
    //in-app purchase
    public void purchaseEvent(String price ,String currency,String order){
        //offical
        double revenue = Double.parseDouble(price);
        AdjustEvent adjustEvent = new AdjustEvent("k0eegp");
        adjustEvent.setRevenue(revenue, currency);
        adjustEvent.setOrderId(order);
        Adjust.trackEvent(adjustEvent);
    }
}
