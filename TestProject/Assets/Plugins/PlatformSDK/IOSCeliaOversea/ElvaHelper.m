//
//  ElvaHelper.m
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import "ElvaHelper.h"

@implementation ElvaHelper{
    NSString *AIHelpAppID;
    NSString *playerName;
    NSString *playerUid;
    NSString *serverId;
    NSString *playerParseId;
    NSString *showConversationFlag;
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
    NSDictionary *infoDictionary = [[NSBundle mainBundle] infoDictionary];
    AIHelpAppID = [infoDictionary objectForKey:@"AIHelpAppID"];
    NSString *AIHelpAppKey = [infoDictionary objectForKey:@"AIHelpAppKey"];
    NSString *AIHelpDomain = [infoDictionary objectForKey:@"AIHelpDomain"];
    [ECServiceSdk setSDKInterfaceOrientationMask:UIInterfaceOrientationMaskPortrait];
    [ECServiceSdk init:AIHelpAppKey Domain:AIHelpDomain AppId:AIHelpAppID];
    playerParseId = @"";
//    [ECServiceSdk setName:@"少女的王座"];
//    [ECServiceSdk setServerId:@"ServerId-123456987456"];
    [ECServiceSdk setSDKLanguage:@"zh_TW"];
//    [ECServiceSdk setSDKLanguage:@"zh_en"];
//    [ECServiceSdk setSDKLanguage:@"zh_CN"];
    NSLog(@"--ElvaHelper---InitSDK---");
}
#pragma mark --入口
-(void)show:(const char*) jsonString{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    NSLog(@" ---ios ElvaHelper--show-: %@", dict);
    playerName = [dict valueForKey:@"playerName"];
    playerUid = [dict valueForKey:@"playerUid"];
    serverId = [dict valueForKey:@"ServerID"];

    showConversationFlag = [dict valueForKey:@"PlayershowConversationFlag"];
    
    int type = [[dict valueForKey:@"Type"] intValue];

    [ECServiceSdk setUserName:playerName];
    [ECServiceSdk setUserId:playerUid];

    if (type == 1) {
        [self showElva];
    }else if (type == 2){
        [self showFAQs];
    }
    else if (type == 3){
        [self showElvaOP];
    }
    else if (type == 4){
        [self showConversation];
    }
    else if (type == 5){
        [self showSuggestWindow:[dict valueForKey:@"formID"]];
    }
    else{
        [self showElva];
    }

}
#pragma mark --智能客服主界面启动，调用 showElva 方法，启动机器人界面
-(void)showElva{
    [ECServiceSdk showElva:playerName
                 PlayerUid:playerUid
                  ServerId:serverId
             PlayerParseId:playerParseId
    PlayershowConversationFlag:showConversationFlag];
}
#pragma mark --展示FAQ列表，调用 showFAQs 方法(必须确保设置用户名称信息 setUserName 和设置用户唯一id信息 setUserId 已经调用)
-(void)showFAQs{
    NSMutableDictionary *customData = [NSMutableDictionary dictionary];//定义自定义参数容器
    NSMutableDictionary *config = [NSMutableDictionary dictionary]; //定义config参数容器
    [config setObject:@"1" forKey:@"showContactButtonFlag"];
    [config setObject:@"1" forKey:@"directConversation"];
    [config setObject:customData forKey:@"elva-custom-metadata"]; //将customData存入容器
    [ECServiceSdk showFAQs:config];
    //[ECServiceSdk showFAQSection:@"158455" Config:config];//展示相关部分FAQ SectionID需要后台配置
}
#pragma mark --运营主界面启动
-(void)showElvaOP{
    [ECServiceSdk showElvaOP:playerName
                   PlayerUid:playerUid
                    ServerId:serverId
               PlayerParseId:playerParseId
  PlayershowConversationFlag:showConversationFlag
                      Config:nil];
}
#pragma mark --直接进行人工客服聊天，调用 showConversation 方法(必须确保设置用户名称信息 setUserName 已经调用)
-(void)showConversation{
    [ECServiceSdk showConversation:playerUid ServerId:serverId];
}
#pragma mark --外部打开反馈表单
-(void)showSuggestWindow:(NSString *)formID{
    NSString *name = [playerName stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
    NSString *urlStr = [NSString stringWithFormat:@"https://aihelp.net/questionnaire/#/?formId=%@&appId=%@&uid=%@&userName=%@",formID,AIHelpAppID,playerUid,name];

    NSURL *url = [NSURL URLWithString: urlStr];
    NSLog(@"-ios----showSuggestWindow---url----%@",url);
    if ([[UIApplication sharedApplication] canOpenURL:url]) {
        if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 10.0) {
                //设备系统为IOS 10.0或者以上的
                [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:nil];
            }else{
                //设备系统为IOS 10.0以下的
                [[UIApplication sharedApplication] openURL:url];
            }
    }


}
@end
