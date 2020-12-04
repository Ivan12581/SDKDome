#import "UnityAppController.h"
#import <RaStarSDK/RaStarSDK.h>
@interface CeliaAppController : UnityAppController<RaStarInitDelegate,RaStarLoginDelegate,RaStarManagerDelegate,RaStarServiceDelegate>

@end

@implementation CeliaAppController

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    // SDK Delegate
    [[RaStarCommon sharedInstance] setUserAgent];
    [[RaStarCommon sharedInstance] setScreenOrientation:UIInterfaceOrientationMaskAllButUpsideDown];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(getShareResult:) name:RaStarShareResultNotificationName object:nil];
    [self InitSDK];
    return YES;
}
- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation{
    return [[RaStarCommon sharedInstance] openURL:url sourceApplication:sourceApplication annotation:annotation];
}
- (void)applicationWillTerminate:(UIApplication *)application{
    [super applicationWillTerminate:application];
    [[RaStarCommon sharedInstance] setCloseType];
}

- (void)applicationDidBecomeActive:(UIApplication *)application {
    [super applicationDidBecomeActive:application];
    [[RaStarCommon sharedInstance] setStartType];
}
- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(nonnull void (^)(NSArray<id<UIUserActivityRestoring>> * _Nullable))restorationHandler{
    [super application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
    if (![[RaStarCommon sharedInstance] handleUniversalLink:userActivity]) {
        // 其他SDK的回调
    }
    return YES;
}

- (void)dismiss:(UIAlertController *)alert{
    [alert dismissViewControllerAnimated:YES completion:nil];
}


extern void UnitySendMessage(const char *, const char *, const char *);

-(void)SendMessageToUnity:(int)msgID DictData:(NSMutableDictionary *) dict{
    [dict setValue: [NSNumber numberWithInt:msgID] forKey: @"msgID"];
    // 判断dict是否可转成json
    //BOOL isYes = [NSJSONSerialization isValidJSONObject:dict];
    // nsdictionary --> nsdata
    NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:kNilOptions error:nil];
    // nsdata -> nsstring
    NSString *jsonNSString = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    // nsstring -> const char*
    const char* jsonString = [jsonNSString UTF8String];
    UnitySendMessage("SDKManager", "OnResult", jsonString);
}

typedef NS_ENUM(NSInteger, MsgID)
{
    eInit = 100,
    eLogin = 101,
    eSwitch = 102,
    ePay = 103,
    eUploadInfo = 104,
    eExitGame = 105,
    eLogout = 106,
    
    eGetDeviceId = 200,
    eConfigInfo = 201,
    eGoogleTranslate = 202,
    eBind = 203,
    eShare = 204,
    eNaver = 205,
    
    eConsumeGoogleOrder = 401,
    eCustomerService = 501,
    eFaceBookEvent = 601,
    eAdjustEvent = 602,
    ePurchase3rdEvent = 603,
};

// SDK回调
#pragma mark -- 初始化
-(void)InitSDK{
    NSDictionary *info= [[NSBundle mainBundle] infoDictionary];
    
    NSLog(@"->Celia 初始化:%@",[NSString stringWithFormat:@"CFBundleShortVersionString--->%@&CFBundleVersion--->%@&",info[@"CFBundleShortVersionString"],info[@"CFBundleVersion"]]);
    [RaStarCommon sharedInstance].useSDKAlertView = YES;
    [RaStarCommon sharedInstance].mainBackGroundColor = [UIColor clearColor];
    
    [[RaStarCommon sharedInstance] addInitDelegate:self];
    [[RaStarCommon sharedInstance] registerSDK];
    [self GetConfigInfo];
}
- (void)onInitSuccess {
    NSLog(@"->Celia 初始化成功\n");
    [self SendMessageToUnity: eInit DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
}
- (void)onInitFail {
    NSLog(@"->Celia 初始化失败\n");
    [self SendMessageToUnity: eInit DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",nil]];
}

#pragma mark -- 登录
-(void)Login{
    [[RaStarCommon sharedInstance] addLoginDelegate:self];
    [[RaStarCommon sharedInstance] showLoginView];
}
- (void)onCommonLoginSuccess:(NSString *)lwToken Uid:(NSString *)uid {
    NSLog(@"->Celia 登陆成功返回 lwToken:%@\n",lwToken);
    NSLog(@"->Celia 登陆成功返回 uid:%@\n",uid);
    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",lwToken,@"token",uid,@"uid",nil]];
}
-(void)onCommonLoginFail{
    NSLog(@"->Celia 登陆失败\n");
    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",nil]];
}

#pragma mark -- 注销登录
-(void)Switch{
     NSLog(@"->Celia Switch 注销\n");
    [[RaStarCommon sharedInstance] loginViewOut];
}
- (void)logoutAction{
    NSLog(@"->Celia 退出登录:\n");
    [[RaStarCommon sharedInstance] loginViewOut];
}
-(void)onCommonLoginOut{
    NSLog(@"->Celia 注销成功\n");
    [self SendMessageToUnity: eLogout DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
    // [[RaStarCommon sharedInstance] showLoginView];
}

#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
    // const char* --> nnstring
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    NSLog(@"->Celia 支付:%@\n",jsonNSString);
    // nsstring -> nsdata
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    // nsdata -> nsdictionary
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];

    [[RaStarCommon sharedInstance] addManagerDelegate:self];
    [[RaStarCommon sharedInstance]
         managerWithAmount:[dict valueForKey:@"PayMoney"]
         andName:[dict valueForKey:@"RoleName"]
         andLevel:[dict valueForKey:@"RoleLevel"]
         andRoleid:[dict valueForKey:@"RoleID"]
         andSid:[dict valueForKey:@"ServerID"]
         andSname:[dict valueForKey:@"ServerName"]
         andSubject:[dict valueForKey:@"GoodsName"]
         andOrder_no:[dict valueForKey:@"OrderID"]
         andExt:[dict valueForKey:@"Extra"]];
}

-(void)onManagerState:(RSManagerResultCode)RSManagercode{
    NSLog(@"->Celia 支付状态码:%ld\n",(long)RSManagercode);
    // TODO:询问SDK同学RSManagercode支付状态码详情
    [self SendMessageToUnity: ePay DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSString stringWithFormat:@"%ld",(long)RSManagercode], @"state",nil]];
}
#pragma mark -- 分享
-(void)share:(const char *) jsonString{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    NSLog(@"->ios 分享:%@\n",jsonNSString);
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    NSString *imgStr =[dict valueForKey:@"img"];
//    NSString *text =[dict valueForKey:@"text"];
    NSInteger type = [[dict valueForKey:@"type"] integerValue];
    UIImage *image = [UIImage imageWithContentsOfFile:imgStr];
    [[RaStarCommon sharedInstance] shareWithImage:image ShareType:(RSShareType)type];
}
#pragma mark -- 分享结果
- (void)getShareResult:(NSNotification *)notification{
    NSString *errMessage = notification.object[@"message"];
    if (errMessage) {
        [self SendMessageToUnity: eShare DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",errMessage,@"message",nil]];
    }else{
        [self SendMessageToUnity: eShare DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
    }

}
/*
code-信息
200-可以聊天
1002-获取配置失败
1003-验证签名失败
1001-参数错误
2001-暂停登陆
2004-用户未登录
3002-账号信息不匹配
1518-该用户暂无实名认证信息
1522-该身份证已使用，请更换其他身份证
*/
#pragma mark -- 验证实名聊天
- (void)verifyRealNameChatAction{
    [[RaStarCommon sharedInstance] getUserVerifiedInfo:^(int code) {
//        self.textView.text = [NSString stringWithFormat:@"聊天状态代码为：%ld",(long)code];
    }];
}
#pragma mark -- 手动拉起实名认证
- (void)showUserVerifiedViewAction{
    [[RaStarCommon sharedInstance] showUserVerifiedView:^(BOOL verifiedType) {
        NSString *type = @"认证失败！";
        if (verifiedType) {
            type = @"认证成功！";
        }
//        self.textView.text = type;
    }];
}


#pragma mark -- 上报数据
-(void)UploadInfo:(const char*) jsonData{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonData];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    int uploadType = [dict[@"UploadType"] intValue];
    switch (uploadType) {
        case 0:// 角色创建
            [[RaStarCommon sharedInstance] uploadCreateInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"Extra"]];
            break;
        case 1:// 进入游戏
            [[RaStarCommon sharedInstance] uploadEnterInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"Extra"]];
            break;
        case 2:// 角色升级
            [[RaStarCommon sharedInstance] uploadUp_levelInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"Extra"]];
            break;
        case 3:// 完成新手
            break;
        case 4:// 更名
            [[RaStarCommon sharedInstance] uploadUpdateInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"OldName"]];
            break;
    }
}

#pragma mark -- 展示客服界面
-(void)OpenService{
    [[RaStarCommon sharedInstance] showService];
    //[[RaStarCommon sharedInstance] addServiceDelegate:self];
}
-(void)serviceClose{
    NSLog(@"->Celia 关闭客服界面成功:\n");
}

-(void)GetConfigInfo{
    [self SendMessageToUnity: eConfigInfo DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state", [RaStarCommon sharedInstance].deviceID, @"deviceID",nil]];
}

#pragma mark -- Unity To IOS
-(void)Call:(int) type andJsonStr:(const char*) jsonstring{
    NSLog(@"-ios----CeliaAppController---Call----%i",type);
        switch (type) {
            case eInit:
                [self InitSDK];
                break;
            case eLogin:
                [self Login];
                break;
            case eLogout:
                [self logoutAction];
                break;
            case ePay:
                [self Pay:jsonstring];
                break;
            case eGetDeviceId:
                
                break;
            case eShare:
                [self share:jsonstring];
                break;
            case eConfigInfo:
                [self GetConfigInfo];
                break;
            case eUploadInfo:
                [self UploadInfo:jsonstring];
                break;
			case eCustomerService:
                [self OpenService];
                break;
            default:
            NSLog(@"-ios----IOSBridgeHelper---该接口ios未实现----%i",type);
                break;
        }
}
@end

IMPL_APP_CONTROLLER_SUBCLASS (CeliaAppController)

extern "C"
{
    void CallFromUnity(int type, const char* jsonString){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate Call:type andJsonStr:jsonString];
    }
}


