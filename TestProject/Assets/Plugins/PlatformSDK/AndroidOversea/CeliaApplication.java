package celia.sdk;

import android.app.Application;

import com.rastargame.sdk.oversea.na.api.RastarSDKProxy;

public class CeliaApplication extends Application
{
    @Override public void onCreate() {
        super.onCreate();

        // 这个方法必须在Application的onCreate中调用，此接口无需先初始化SDK，初始化SDK接口放在主Activity的onCreate中进行
        RastarSDKProxy.getInstance().onApplicationCreate(this);
    }
}
