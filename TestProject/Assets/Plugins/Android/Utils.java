package celia.sdk;

import android.Manifest;
import android.annotation.SuppressLint;
import android.content.Context;
import android.content.SharedPreferences;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.os.Build;
import android.preference.PreferenceManager;
import android.provider.Settings;
import android.telephony.TelephonyManager;
import android.util.Base64;

import androidx.annotation.RequiresApi;
import androidx.annotation.RequiresPermission;

import java.io.File;
import java.security.MessageDigest;
import java.util.Currency;
import java.util.Locale;
import java.util.UUID;

public class Utils {
    CeliaActivity mainActivity;
    private static Utils instance;
    private Utils(CeliaActivity activity){
        mainActivity = activity;
    }

    public Utils() {

    }

    public static Utils getInstance(){
        if (instance==null){
            instance = new Utils();
        }
        return instance;
    }

    public void SetActivity(CeliaActivity activity){
        mainActivity = activity;
    }

    //Build.VERSION_CODES.M 23
    //Build.VERSION_CODES.O 26
    //Build.VERSION_CODES.Q 29
    @SuppressLint("HardwareIds")
    public String getIMEIDeviceId() {
        mainActivity.ShowLog("---getIMEIDeviceId--->");
        String deviceId;
        String uniqueID = UUID.randomUUID().toString();
        mainActivity.ShowLog("---Build.VERSION.SDK_INT--->" + Build.VERSION.SDK_INT);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q)
        {
            deviceId = Settings.Secure.getString(mainActivity.getContentResolver(), Settings.Secure.ANDROID_ID);
        } else {
            final TelephonyManager mTelephony = (TelephonyManager) mainActivity.getSystemService(Context.TELEPHONY_SERVICE);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                if (mainActivity.checkSelfPermission(Manifest.permission.READ_PHONE_STATE) != PackageManager.PERMISSION_GRANTED) {
                    return "";
                }
            }
            assert mTelephony != null;
            if (mTelephony.getDeviceId() != null)
            {
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
                {
                    mainActivity.ShowLog( "---getImei-->");
                    deviceId = mTelephony.getImei();
                }else {
                    mainActivity.ShowLog("---getDeviceId-->");
                    deviceId = mTelephony.getDeviceId();
                }
            } else {
                mainActivity.ShowLog("---ANDROID_ID-->");
                deviceId = Settings.Secure.getString(mainActivity.getContentResolver(), Settings.Secure.ANDROID_ID);
            }
        }
        return deviceId;
    }
	 /* 获取签名Key的hash值facebook后台需要添加这个如果这个值跟后台的对不上那么会分享失败提示Key Hash值不对
	 */
    public void getKeyHash(){

        // Add code to print out the key hash
        try {
            PackageInfo info = mainActivity.getPackageManager().getPackageInfo(
                    "com.elex.girlsthrone.tw.gp", //替换成你app的包名
                    PackageManager.GET_SIGNATURES);
            for (Signature signature : info.signatures) {
                MessageDigest md = MessageDigest.getInstance("SHA");
                md.update(signature.toByteArray());
                mainActivity.ShowLog("---KeyHash-->"+Base64.encodeToString(md.digest(), Base64.DEFAULT));

            }
        } catch (Exception e) {

        }
    }
//获取目标默认货币信息
    public void getCurrencyInfo(){
        Currency curCurency = Currency.getInstance(Locale.getDefault());
        mainActivity.ShowLog("---Build.VERSION_CODES.KITKAT-->"+Build.VERSION_CODES.KITKAT);
        mainActivity.ShowLog("---curCurency.getCurrencyCode()-->"+curCurency.getCurrencyCode());
//        mainActivity.ShowLog("---curCurency.getCurrencyCode()-->"+curCurency.getDisplayName());
        mainActivity.ShowLog("---curCurency.getSymbol()-->"+curCurency.getSymbol());
//        mainActivity.ShowLog("---curCurency.getCurrencyCode()-->"+curCurency.getNumericCode());
        mainActivity.ShowLog("---curCurency.getDefaultFractionDigits()-->"+curCurency.getDefaultFractionDigits());

    }
    public void saveCacheData(String key,String value){
        SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(mainActivity);
        //获取editor对象
        SharedPreferences.Editor editor = preferences.edit();
        //存储数据时选用对应类型的方法
        editor.putString(key,value);
        //提交保存数据
        editor.commit();
    }
    public String getCacheData(String key){
        SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(mainActivity);
        //获取editor对象
        SharedPreferences.Editor editor = preferences.edit();
        //存储数据时选用对应类型的方法
        return preferences.getString(key,"");
    }
    //去除小数点后多余的0 或者小数点
    public String rvZeroAndDot(String s){
        if (s.isEmpty()) {
            return null;
        }
        if(s.indexOf(".") > 0){
            s = s.replaceAll("0+?$", "");//去掉多余的0
            s = s.replaceAll("[.]$", "");//如最后一位是.则去掉
        }
        return s;
    }
}
