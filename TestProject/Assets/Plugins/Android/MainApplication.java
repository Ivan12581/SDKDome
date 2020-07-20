package com.elex.girlsthrone.tw.gp;

import android.app.Application;

public class MainApplication extends Application {
    @Override
    public void onCreate()
    {
        super.onCreate();
        //放到Application中初始化
//        FacebookSdk.sdkInitialize(getApplicationContext());
//        AppEventsLogger.activateApp(this);
//
//        initAdjust();
    }
}
