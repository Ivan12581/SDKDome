package celia.sdk;

import com.adjust.sdk.Adjust;
import com.adjust.sdk.AdjustConfig;
import com.adjust.sdk.AdjustEvent;
import com.adjust.sdk.LogLevel;
import com.unity3d.player.UnityPlayer;

public class AdjustHelper {
    CeliaActivity mainActivity;
    UnityPlayer mUnityPlayer;
    AdjustConfig config;
    public AdjustHelper(CeliaActivity activity)
    {
        mainActivity = activity;
        Init();
    }
    private void Init(){
        String environment = AdjustConfig.ENVIRONMENT_SANDBOX;
//        String environment = AdjustConfig.ENVIRONMENT_PRODUCTION;
        config = new AdjustConfig(mainActivity, Constant.Adjust_AppToken, environment);
        config.setAppSecret(1, 555720783, 475659758, 1930748874, 572648029);
        Adjust.onCreate(config);
        setLogLevelState(false);
        EventStatistics("4pvqgy");
//        config.setDelayStart(5.5);//延迟启动
    }
    public void setLogLevelState(boolean state){
        //        config.setLogLevel(LogLevel.VERBOSE); // enable all logs
//        config.setLogLevel(LogLevel.DEBUG); // disable verbose logs
//        config.setLogLevel(LogLevel.INFO); // disable debug logs (default)
//        config.setLogLevel(LogLevel.WARN); // disable info logs
//        config.setLogLevel(LogLevel.ERROR); // disable warning logs
//        config.setLogLevel(LogLevel.ASSERT); // disable error logs
//        config.setLogLevel(LogLevel.SUPRESS); // disable all logs
        if (state){
            config.setLogLevel(LogLevel.SUPRESS); // disable all logs
        }else {
            config.setLogLevel(LogLevel.VERBOSE); // enable all logs
        }
    }
    //in-app onClick
    public void EventStatistics(String EventName){
        if (EventName == null||EventName.isEmpty()){
                return;
        }
        AdjustEvent adjustEvent = new AdjustEvent(EventName);
        Adjust.trackEvent(adjustEvent);
    }
    //in-app purchase
    public void purchaseEvent(double revenue ,String currency){
        AdjustEvent adjustEvent = new AdjustEvent("abc123");
        adjustEvent.setRevenue(0.01, "EUR");
//        adjustEvent.setOrderId("{OrderId}");
        Adjust.trackEvent(adjustEvent);
    }
    //in-app Event callback
    public void CallBackEvent(){
        AdjustEvent adjustEvent = new AdjustEvent("abc123");
        adjustEvent.addCallbackParameter("key", "value");
        adjustEvent.addCallbackParameter("foo", "bar");
        Adjust.trackEvent(adjustEvent);
    }
}
