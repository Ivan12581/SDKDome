//
//  ElvaHelper.m
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import "ElvaHelper.h"

@implementation ElvaHelper{
    id IOSBridgeHelper;
}
static ElvaHelper *ElvaHelperIns = nil;
+(ElvaHelper*)sharedInstance{
    if (ElvaHelperIns == nil) {
        ElvaHelperIns = [ElvaHelper new];
    }
    return ElvaHelperIns;
}
-(void)InitSDK:(id)delegate{
    IOSBridgeHelper = delegate;
    NSString *AIHelpAppID = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"AIHelpAppID"];
    NSString *AIHelpAppKey = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"AIHelpAppKey"];
    NSString *AIHelpDomain = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"AIHelpDomain"];

    [ECServiceSdk setUserName:@"UserName-PLAYER_NAME"];  //需要调用此方法
    [ECServiceSdk setUserId:@"UserId-123ABC567DEF"];   //需要调用此方法
    [ECServiceSdk setName:@"Girl for the Throne TW"];
    [ECServiceSdk setServerId:@"ServerId-123456987456"];
    [ECServiceSdk setSDKLanguage:@"zh_CN"];
//        [ECServiceSdk setSDKLanguage:@"en"];
    [ECServiceSdk setSDKInterfaceOrientationMask:UIInterfaceOrientationMaskPortrait];
        [ECServiceSdk init:AIHelpAppKey Domain:AIHelpDomain AppId:AIHelpAppID];
}
#pragma mark --入口
-(void)show:(const char*) jsonData{

}
#pragma mark --智能客服主界面启动，调用 showElva 方法，启动机器人界面
-(void)showElva{
    //您需要将相同的标签添加到“AIHELP Web控制台”才能生效。
    NSMutableArray * tags = [NSMutableArray array]; //定义tag容器
    [tags addObject:@"vip"];
    [tags addObject:@"pay1"];

    NSMutableDictionary *customData = [NSMutableDictionary dictionary];//定义自定义参数容器
    [customData setObject:tags forKey:@"elva-tags"]; //添加Tag值标签
    [customData setObject:@"1.0.0" forKey:@"VersionCode"];  //添加自定义的参数

    NSMutableDictionary *config = [NSMutableDictionary dictionary]; //定义config参数容器
    [config setObject:customData forKey:@"elva-custom-metadata"]; //将customData存入容器

    [ECServiceSdk showElva:@"USER_NAME"
                 PlayerUid:@"USER_ID"
                  ServerId:@"123"
             PlayerParseId:@""
    PlayershowConversationFlag:@"1"
                    Config:config];

    /* config 示例内容:
        {
            "elva-custom-metadata": {
                "elva-tags": [
                    "vip",
                    "pay1"
                ],
                "VersionCode": "1.0.0"
            }
    }
    */
}
#pragma mark --直接进行人工客服聊天，调用 showConversation 方法(必须确保设置用户名称信息 setUserName 已经调用)
-(void)showConversation{

    //您需要将相同的标签添加到“AIHELP Web控制台”才能生效。
    NSMutableArray * tags = [NSMutableArray array]; //定义tag容器
    [tags addObject:@"vip"];
    [tags addObject:@"pay1"];

    NSMutableDictionary *customData = [NSMutableDictionary dictionary];//定义自定义参数容器
    [customData setObject:tags forKey:@"elva-tags"]; //添加Tag值标签
    [customData setObject:@"1.0.0" forKey:@"VersionCode"];  //添加自定义的参数

    NSMutableDictionary *config = [NSMutableDictionary dictionary]; //定义config参数容器
    [config setObject:customData forKey:@"elva-custom-metadata"]; //将customData存入容器

    [ECServiceSdk showConversation:@"PLAYER_ID" ServerId:@"123" Config:config];
}

#pragma mark --展示FAQ列表，调用 showFAQs 方法(必须确保设置用户名称信息 setUserName 和设置用户唯一id信息 setUserId 已经调用)
-(void)showFAQs{


    //您需要将相同的标签添加到“AIHELP Web控制台”才能生效。
//    NSMutableArray * tags = [NSMutableArray array]; //定义tag容器
//    [tags addObject:@"vip"];
//    [tags addObject:@"pay1"];
    NSMutableDictionary *customData = [NSMutableDictionary dictionary];//定义自定义参数容器
//    [customData setObject:tags forKey:@"elva-tags"]; //添加Tag值标签
    [customData setObject:@"自定义的参数123456" forKey:@"自定义的参数"];  //添加自定义的参数

    [customData setObject:@"自定义人工欢迎语" forKey:@"private_welcome_str"]; //添加自定义人工欢迎语
    [customData setObject:@"自定义欢迎语" forKey:@"anotherWelcomeText"]; //添加自定义欢迎语
    
    NSMutableDictionary *config = [NSMutableDictionary dictionary]; //定义config参数容器
    [config setObject:@"1" forKey:@"showContactButtonFlag"];
    [config setObject:@"1" forKey:@"showConversationFlag"];
    [config setObject:customData forKey:@"elva-custom-metadata"]; //将customData存入容器
    //一、联系我们按钮显示逻辑
    //    0、默认：FAQ列表页和详情页不显示，点击“踩”，显示联系我们按钮。不用处理 config。
    //    1、一直显示：需要设置'showContactButtonFlag'，添加如下代码即可
    //        [config setObject:@"1" forKey:@"showContactButtonFlag"];
    //    2、永不显示：需要设置'hideContactButtonFlag'，添加如下代码即可
    //        [config setObject:@"1" forKey:@"hideContactButtonFlag"];

    //二、点击联系我们按钮（经过一步骤，显示了联系我们按钮的前提）进入客服页面的逻辑
    //    0、默认：进入机器人页面（无进行中客诉时，不显示人工客服按钮）。不用处理 config。
    //    1、直接进入人工页面：需要设置'directConversation'，添加如下代码即可
    //        [config setObject:@"1" forKey:@"directConversation"];
    //    2、进入机器人页面+人工客服入口按钮：需要设置'showConversationFlag'，添加如下代码即可
    //        [config setObject:@"1" forKey:@"showConversationFlag"];

    [ECServiceSdk showFAQs:config];
}
-(void)showElvaOP{
    //您需要将相同的标签添加到“AIHELP Web控制台”才能生效。
    NSMutableArray * tags = [NSMutableArray array]; //定义tag容器
    [tags addObject:@"vip"];
    [tags addObject:@"pay1"];

    NSMutableDictionary *customData = [NSMutableDictionary dictionary];//定义自定义参数容器
    [customData setObject:tags forKey:@"elva-tags"]; //添加Tag值标签
    [customData setObject:@"1.0.0" forKey:@"VersionCode"];  //添加自定义的参数

    NSMutableDictionary *config = [NSMutableDictionary dictionary]; //定义config参数容器
    [config setObject:customData forKey:@"elva-custom-metadata"]; //将customData存入容器

    [ECServiceSdk showElvaOP:@"USER_NAME"
                   PlayerUid:@"USER_ID"
                    ServerId:@"123"
               PlayerParseId:@""
    PlayershowConversationFlag:@"1"
                      Config:config];
}
@end
