package celia.sdk;
import android.content.Intent;
import android.net.Uri;

import com.ljoy.chatbot.sdk.ELvaChatServiceSdk;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.MessageFormat;
import java.util.HashMap;

public class ElvaHelper {
    CeliaActivity mainActivity;
    String playerUid = "userId123";
    String playerName = "userName123";
    String serverID = "serverId123";
    String gameName = "Girl for the Throne TW";
    String PlayershowConversationFlag = "1";
    public ElvaHelper(CeliaActivity activity)
    {
        mainActivity = activity;
        Init();
    }
    public void Init(){
//        ELvaChatServiceSdk.setName(gameName);
//        ELvaChatServiceSdk.setUserId(playerUid);
//        ELvaChatServiceSdk.setUserName(playerName);
//        ELvaChatServiceSdk.setServerId(serverID);
//        ELvaChatServiceSdk.setSDKLanguage("zh_CN");
        setInitCallback();
        ELvaChatServiceSdk.init(mainActivity,Constant.Elva_AppKey,Constant.Elva_Domain,Constant.Elva_AppId);
        String languageAlias = "zh_TW";
        ELvaChatServiceSdk.setSDKLanguage(languageAlias);

    }
    // 在调用初始化init方法之前，设置初始化回调函数
    public void setInitCallback() {
        ELvaChatServiceSdk.setOnInitializedCallback(new ELvaChatServiceSdk.OnInitializationCallback() {
            @Override
            public void onInitialized() {
                mainActivity.ShowLog("---ElvaHelper---");
            }
        });
    }
    public void Show(String jsonStr){
        try{
            JSONObject jsonObject = new JSONObject(jsonStr);
            playerName = jsonObject.getString("playerName");
            playerUid = jsonObject.getString("playerUid");
            serverID = jsonObject.getString("ServerID");
            PlayershowConversationFlag = jsonObject.getString("PlayershowConversationFlag");
            ELvaChatServiceSdk.setUserId(playerUid);
            ELvaChatServiceSdk.setUserName(playerName);
            int type = Integer.parseInt(jsonObject.getString("Type"));
            if (type == 1){
                ShowElva();
            }else if (type == 2){
                showFAQs();
            }
            else if (type == 3){
                showElvaOP();
            }
            else if (type == 4){
                showConversation();
}            else if (type == 5){
                showSuggestWindow(jsonObject.getString("formID"));
            }else {
                ShowElva();
            }
        }catch (JSONException e){
            e.printStackTrace();
        }
    }
    //1.调用showElva接口，启动机器人客服聊天界面
    public void ShowElva(){
        ELvaChatServiceSdk.showElva(playerName, playerUid, serverID, PlayershowConversationFlag);
    }
    //2.调用showConversation方法，启动人工客服界面
    public void showConversation(){
        ELvaChatServiceSdk.showConversation(playerUid, serverID);
    }
    //3.调用showElvaOP方法，启动运营模块界面
    public void showElvaOP(){
        ELvaChatServiceSdk.showElvaOP(playerName, playerUid, serverID, PlayershowConversationFlag);
    }
    //4.展示FAQ列表，调用showFAQs 方法
    public void showFAQs(){
        HashMap<String, Object> config = new HashMap();
        HashMap<String, Object> map = new HashMap();
        // "elva-custom-metadata" 是key值 不可以变
        config.put("elva-custom-metadata", map);
        config.put("showContactButtonFlag", "1");
        config.put("directConversation", "1");
        ELvaChatServiceSdk.showFAQs(config);
//        ELvaChatServiceSdk.showFAQSection("1234",config);
    }
    //5.外部打开反馈表单
    public void showSuggestWindow(String formId){
        String s = MessageFormat.format("https://aihelp.net/questionnaire/#/?formId={0}&appId={1}&uid={2}&userName={3}",formId,
                Constant.Elva_AppId,playerUid,playerName);
        Uri uri=Uri.parse("https://aihelp.net/questionnaire/#/?formId=59a639c15027487ba447551b1f8d16cb&appId=elextech_platform_c72b5e80-ec84-45d7-9fe6-212c0800c423&uid=123&userName=456");
        Intent intent=new Intent(Intent.ACTION_VIEW,uri);
        mainActivity.startActivity(intent);
    }
}
