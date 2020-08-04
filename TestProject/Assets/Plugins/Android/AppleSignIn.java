package celia.sdk;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Handler;
import android.os.Message;
import android.view.View;
import android.webkit.JavascriptInterface;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.ImageButton;

import androidx.annotation.MainThread;

import com.elex.girlsthrone.tw.gp.R;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.UnsupportedEncodingException;
import java.text.MessageFormat;
import java.util.HashMap;

public class AppleSignIn {
    CeliaActivity mainActivity;
    UnityPlayer mUnityPlayer;

    public Handler handler;
    WebView webView;
    String targetUrl;

    public AppleSignIn(UnityPlayer unity,CeliaActivity activity)
    {
        mainActivity = activity;
        mUnityPlayer = unity;
        BuildUrl();
        InitWeb();
    }

    void BuildUrl()
    {
        targetUrl = "<html><head><meta http-equiv=\"refresh\" content=\"0;url=" + MessageFormat.format("https://appleid.apple.com/auth/authorize?client_id={0}&redirect_uri={1}&response_type=code%20id_token&scope=name&response_mode=form_post",
                Constant.apple_client_id,Constant.apple_redirect_uri) + "\"></head></html>";
        mainActivity.ShowLog(targetUrl);
    }

    void InitWeb()
    {
        handler = new Handler() {
            @Override
            public void handleMessage(Message msg) {
                if (msg.arg1 == 1)
                {
                    webView.loadData(targetUrl,"text/html","utf-8");
                    mUnityPlayer.addView(webView);
                }else
                {
                    if (webView != null)
                    {
                        mUnityPlayer.removeView(webView);
                        //TODO: 登陆失败处理
                    }
                }
            }
        };

//set web
        webView = new WebView(mainActivity  );

        WebSettings webSettings = webView.getSettings();
        webSettings.setJavaScriptEnabled(true);
        webSettings.setUseWideViewPort(true); //将图片调整到适合webview的大小
        webSettings.setLoadWithOverviewMode(true); // 缩放至屏幕的大小
        webSettings.setSupportZoom(true); //支持缩放，默认为true。是下面那个的前提。
        webSettings.setBuiltInZoomControls(false); //设置内置的缩放控件。若为false，则该WebView不可缩放
        webView.addJavascriptInterface(new InJavaScriptLocalObj(), "java_obj");
        webView.setWebViewClient(new myWebClient());

//set button
        InputStream is = null;
        try {
            is = mainActivity.getAssets().open("close.png");
        } catch (IOException e) {
            e.printStackTrace();
        }
        Bitmap bitmap = BitmapFactory.decodeStream(is);
        ImageButton btn = new ImageButton(mainActivity);
        btn.setImageBitmap(bitmap);
        btn.getBackground().setAlpha(0);
        btn.setPadding(10,10,0,0);
        btn.setScaleX(2);
        btn.setScaleY(2);
        btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                CloseWeb();
            }
        });
        webView.addView(btn);
    }

    public  void OpenWeb()
    {
        Message msg = Message.obtain();
        msg.arg1 = 1;
        handler.sendMessage(msg);
    }

    public  void CloseWeb()
    {
        Message msg = Message.obtain();
        msg.arg1 = 0;
        handler.sendMessage(msg);
    }

    void ReportResult(String json)
    {
        mainActivity.ShowLog(json);
        try
        {
            JSONObject jsonObject = new JSONObject(json);
            if (jsonObject.has("error_code")){
                mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                    {
                        put("state","0");
                    } });
                CloseWeb();
            }else {
                mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                    {
                        put("state","1");
                        put("code",jsonObject.getString("code"));
                        put("uid",jsonObject.getString("user_identifier"));
                        put("token",jsonObject.getString("id_token"));
                    } });
                CloseWeb();
            }

        }catch (JSONException e){
            e.printStackTrace();
        }
    }

    //region web处理
    private class myWebClient extends WebViewClient
    {
        @Override
        public void onPageFinished(WebView view, String url) {
            System.out.println("Current url:" + url);
            if (url.equals("http://ttog-monitor.elexapp.com:54081/"))
            {
                System.out.println("Injected");
                view.loadUrl("javascript:window.java_obj.getSource(document.getElementsByTagName('body')[0].innerText);");
            }
            super.onPageFinished(view, url);
        }
    }
    final class InJavaScriptLocalObj {
        @JavascriptInterface
        public void getSource(String html) {
            ReportResult(html);
        }
    }

    //endregion
}
