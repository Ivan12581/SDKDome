package celia.sdk;


import com.ljoy.chatbot.sdk.ELvaChatServiceSdk;
import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;
import java.util.HashMap;

public class ElvaHelper {
    CeliaActivity mainActivity;
    UnityPlayer mUnityPlayer;

    public ElvaHelper(CeliaActivity activity)
    {
        mainActivity = activity;
        Init();
    }
    public void Init(){

    }
    //2.调用showElva接口，启动机器人客服聊天界面
    public void ShowElva(){
        HashMap<String,Object> map = new HashMap();
        ArrayList<String> tags = new ArrayList();
        // the tag names are variables
        tags.add("pay1");
        tags.add("s1");
        tags.add("vip2");

        // "elva-tags" 是key值 不可以变
        map.put("elva-tags",tags);

        HashMap<String,Object> config = new HashMap();

        // "elva-custom-metadata" 是key值 不可以变
        config.put("elva-custom-metadata",map);

        ELvaChatServiceSdk.showElva(
                "user_name",
                "user_id",
                "server_id",
                "1",
                config);
    }
    //3.调用showConversation方法，启动人工客服界面
    public void showConversation(){
        HashMap<String,Object> map = new HashMap();
        ArrayList<String> tags = new ArrayList();
        // the tag names are variables
        tags.add("pay1");
        tags.add("s1");
        tags.add("vip2");

        // "elva-tags" 是key值 不可以变
        map.put("elva-tags",tags);

        HashMap<String,Object> config = new HashMap();

        // "elva-custom-metadata" 是key值 不可以变
        config.put("elva-custom-metadata",map);

        ELvaChatServiceSdk.showConversation(
                "user_id",
                "server_id",
                config);
    }
    //4.调用showElvaOP方法，启动运营模块界面
    public void showElvaOP(){
        HashMap<String,Object> map = new HashMap();
        ArrayList<String> tags = new ArrayList();
        // the tag names are variables
        tags.add("pay1");
        tags.add("s1");
        tags.add("vip2");

        // "elva-tags" 是key值 不可以变
        map.put("elva-tags",tags);

        HashMap<String,Object> config = new HashMap();

        // "elva-custom-metadata" 是key值 不可以变
        config.put("elva-custom-metadata",map);

        ELvaChatServiceSdk.showElvaOP(
                "user_name",
                "user_id",
                "server_id",
                "1",
                config);
    }
    //5.展示FAQ列表，调用showFAQs 方法
    public void showFAQs(){
        HashMap<String, Object> config = new HashMap();
        HashMap<String, Object> map = new HashMap();
        ArrayList<String> tags = new ArrayList();
        tags.add("vip1");//第一种方式自定义 需要和后台保持一致(针对key形式)

        // "elva-tags" 是key值 不可以变
        map.put("elva-tags", tags);

        map.put("udid", "123456789");//第二种方式自定义 不需要去后台配置(针对key-value形式)

        // "elva-custom-metadata" 是key值 不可以变
        config.put("elva-custom-metadata", map);

        // 加入此参数,其中key是不可变的 优先级最高 加上后faq右上角则永不显示
        // (如果想显示 需要删除此参数 并加入 config.put("showContactButtonFlag", "1");
        config.put("hideContactButtonFlag", "1");

        // 显示可以从FAQ列表右上角进入机器人客服(如果不想显示 需要删除此参数)
        config.put("showContactButtonFlag", "1");

        // 点击FAQ右上角后 进入机器人界面右上角是否显示 (如果不想显示 需要删除此参数)
        config.put("showConversationFlag", "1");

        // 点击FAQ右上角后 直接会进入到人工客服页面(不加默认进入机器人界面 如果不需要则删除此参数)
        config.put("directConversation", "1");

        // 设置用户名 如果拿不到username，就传入空字符串""，会使用默认昵称"anonymous"
        ELvaChatServiceSdk.setUserName("user_name");

        // 设置用户ID 如果拿不到userid，就传入空字符串""，系统会生成一个唯一设备id
        ELvaChatServiceSdk.setUserId("user_id");

        // 设置服务ID
        ELvaChatServiceSdk.setServerId("server_id");

        ELvaChatServiceSdk.showFAQs(config);
    }
    //6.展示某一分类里的所有FAQ，调用showFAQSection方法
    public void showFAQSection(){
        HashMap<String, Object> config = new HashMap();
        HashMap<String, Object> map = new HashMap();
        ArrayList<String> tags = new ArrayList();
        tags.add("vip1");//第一种方式自定义 需要和后台保持一致(针对key形式)

        // "elva-tags" 是key值 不可以变
        map.put("elva-tags", tags);

        map.put("udid", "123456789");//第二种方式自定义 不需要去后台配置(针对key-value形式)

        // "elva-custom-metadata" 是key值 不可以变
        config.put("elva-custom-metadata", map);

        // 加入此参数,其中key是不可变的 优先级最高 加上后faq右上角则永不显示
        // (如果想显示 需要删除此参数 并加入 config.put("showContactButtonFlag", "1");
        config.put("hideContactButtonFlag", "1");

        // 显示可以从FAQ列表右上角进入机器人客服(如果不想显示 需要删除此参数)
        config.put("showContactButtonFlag", "1");

        // 点击FAQ右上角后 进入机器人界面右上角是否显示 (如果不想显示 需要删除此参数)
        config.put("showConversationFlag", "1");

        // 点击FAQ右上角后 直接会进入到人工客服页面(不加默认进入机器人界面 如果不需要则删除此参数)
        config.put("directConversation", "1");

        // 设置用户名 如果拿不到username，就传入空字符串""，会使用默认昵称"anonymous"
        ELvaChatServiceSdk.setUserName("user_name");

        // 设置用户ID 如果拿不到userid，就传入空字符串""，系统会生成一个唯一设备id
        ELvaChatServiceSdk.setUserId("user_id");

        // 设置服务ID
        ELvaChatServiceSdk.setServerId("server_id");

        ELvaChatServiceSdk.showFAQSection("1234",config);
    }
    //7.展示单条FAQ，调用showSingleFAQ方法
    public void showSingleFAQ(){

    }
}
