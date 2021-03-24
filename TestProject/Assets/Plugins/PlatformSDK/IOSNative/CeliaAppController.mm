#import "UnityAppController.h"
#import <RaStarSDK/RaStarSDK.h>
#import "XGPush.h"
@interface CeliaAppController : UnityAppController<RaStarInitDelegate,RaStarLoginDelegate,RaStarManagerDelegate,RaStarServiceDelegate,XGPushDelegate>

@end

@implementation CeliaAppController{
    bool isTPNSRegistSuccess;
}

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    // SDK Delegate
    [[RaStarCommon sharedInstance] setUserAgent];
    [[RaStarCommon sharedInstance] setScreenOrientation:UIInterfaceOrientationMaskAllButUpsideDown];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(getShareResult:) name:RaStarShareResultNotificationName object:nil];
    [self GetConfigInfo];
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
    eReview = 206,

    eConsumeGoogleOrder = 401,
    eCustomerService = 501,
    eFaceBookEvent = 601,
    eAdjustEvent = 602,
    ePurchase3rdEvent = 603,
};
/// 启动TPNS
- (void)xgStart {
    /// 控制台打印TPNS日志，开发调试建议开启
    [[XGPush defaultManager] setEnableDebug:YES];
    /// 自定义通知栏消息行为，有自定义消息行为需要使用
    //    [self setNotificationConfigure];
    /// 非广州集群，请开启对应集群配置（广州集群无需使用），此函数需要在startXGWithAccessID函数之前调用
    //    [self configHost];
    /// 启动TPNS服务
    [[XGPush defaultManager] startXGWithAccessID:1600017944 accessKey:@"IBXZARNAVUOA" delegate:self];
    /// 角标数目清零,通知中心清空
    if ([XGPush defaultManager].xgApplicationBadgeNumber > 0) {
//        TPNS_DISPATCH_MAIN_SYNC_SAFE(^{
            [XGPush defaultManager].xgApplicationBadgeNumber = 0;
//        });
    }
}

#pragma mark *** XGPushDelegate ***

/// 注册推送服务成功回调
/// @param deviceToken APNs 生成的Device Token
/// @param xgToken TPNS 生成的 Token，推送消息时需要使用此值。TPNS 维护此值与APNs 的 Device Token的映射关系
/// @param error 错误信息，若error为nil则注册推送服务成功
- (void)xgPushDidRegisteredDeviceToken:(nullable NSString *)deviceToken xgToken:(nullable NSString *)xgToken error:(nullable NSError *)error {
    NSLog(@"%s, result %@, error %@", __FUNCTION__, error ? @"NO" : @"OK", error);
    NSString *errorStr = !error ? NSLocalizedString(@"success", nil) : NSLocalizedString(@"failed", nil);
    NSString *message = [NSString stringWithFormat:@"%@%@", NSLocalizedString(@"register_app", nil), errorStr];
    NSLog(@"---注册推送服务成功回调-->%@", message);
    //在注册完成后上报角标数目
    if (!error) {
        //重置服务端自动+1基数
        [[XGPush defaultManager] setBadge:0];
    }
    //设置是否注册成功
    isTPNSRegistSuccess = error ? false : true;
}

/// 注册推送服务失败回调
/// @param error 注册失败错误信息
- (void)xgPushDidFailToRegisterDeviceTokenWithError:(nullable NSError *)error {
    NSLog(@"%s, errorCode:%ld, errMsg:%@", __FUNCTION__, (long)error.code, error.localizedDescription);
    NSLog(@"---注册推送服务失败回调-->%@", error.localizedDescription);
}

/// 注销推送服务回调
- (void)xgPushDidFinishStop:(BOOL)isSuccess error:(nullable NSError *)error {
    NSString *errorStr = !error ? NSLocalizedString(@"success", nil) : NSLocalizedString(@"failed", nil);
    NSString *message = [NSString stringWithFormat:@"%@%@", NSLocalizedString(@"unregister_app", nil), errorStr];
    NSLog(@"---注销推送服务回调--->%@", message);
    //设置是否注册成功
    if (!error) {
        isTPNSRegistSuccess = false;
    }
}

/// 统一接收消息的回调
/// @param notification 消息对象(有2种类型NSDictionary和UNNotification具体解析参考示例代码)
/// @note 此回调为前台收到通知消息及所有状态下收到静默消息的回调（消息点击需使用统一点击回调）
/// 区分消息类型说明：xg字段里的msgtype为1则代表通知消息,msgtype为2则代表静默消息,msgtype为9则代表本地通知
- (void)xgPushDidReceiveRemoteNotification:(nonnull id)notification withCompletionHandler:(nullable void (^)(NSUInteger))completionHandler {
    NSDictionary *notificationDic = nil;
    if ([notification isKindOfClass:[UNNotification class]]) {
        notificationDic = ((UNNotification *)notification).request.content.userInfo;
        completionHandler(UNNotificationPresentationOptionBadge | UNNotificationPresentationOptionSound | UNNotificationPresentationOptionAlert);
    } else if ([notification isKindOfClass:[NSDictionary class]]) {
        notificationDic = notification;
        completionHandler(UIBackgroundFetchResultNewData);
    }
    NSLog(@"receive notification dic: %@", notificationDic);
}

/// 统一点击回调
/// @param response 如果iOS 10+/macOS 10.14+则为UNNotificationResponse，低于目标版本则为NSDictionary
/// 区分消息类型说明：xg字段里的msgtype为1则代表通知消息,msgtype为9则代表本地通知
- (void)xgPushDidReceiveNotificationResponse:(nonnull id)response withCompletionHandler:(nonnull void (^)(void))completionHandler {
    NSLog(@"[TPNS Demo] click notification");
    if ([response isKindOfClass:[UNNotificationResponse class]]) {
        /// iOS10+消息体获取
        NSLog(@"notification dic: %@", ((UNNotificationResponse *)response).notification.request.content.userInfo);
    } else if ([response isKindOfClass:[NSDictionary class]]) {
        /// <IOS10消息体获取
        NSLog(@"notification dic: %@", response);
    }
    completionHandler();
}

/// 角标设置成功回调
/// @param isSuccess 设置角标是否成功
/// @param error 错误标识，若设置不成功会返回
- (void)xgPushDidSetBadge:(BOOL)isSuccess error:(nullable NSError *)error {
    NSLog(@"%s, 角标设置回调result %@, error %@", __FUNCTION__, error ? @"NO" : @"OK", error);
}
// SDK回调
#pragma mark -- 初始化
-(void)InitSDK{
     NSLog(@"->Celia 初始化\n");
    [RaStarCommon sharedInstance].useSDKAlertView = YES;
    [RaStarCommon sharedInstance].mainBackGroundColor = [UIColor clearColor];
    
    [[RaStarCommon sharedInstance] addInitDelegate:self];
    [[RaStarCommon sharedInstance] registerSDK];

    
    
    [self xgStart];
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
    //伪分享回调 因为有些渠道分享没有回调
    [self SendMessageToUnity: eShare DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
}
#pragma mark -- 分享结果
- (void)getShareResult:(NSNotification *)notification{
    // NSString *errMessage = notification.object[@"message"];
    // if (errMessage) {
    //     [self SendMessageToUnity: eShare DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",errMessage,@"message",nil]];
    // }else{
    //     [self SendMessageToUnity: eShare DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
    // }

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
}
-(void)serviceClose{
    NSLog(@"->Celia 关闭客服界面成功:\n");
}

-(void)GetConfigInfo{
    NSDictionary *infoDictionary = [[NSBundle mainBundle] infoDictionary];
    NSString *adId = [infoDictionary objectForKey:@"SKStoreProductParameterAdNetworkIdentifier"];
    [self SendMessageToUnity: eConfigInfo DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state", [RaStarCommon sharedInstance].deviceID, @"deviceID",adId, @"adId",nil]];
}

#pragma mark --  引导进入App Store评论（请与运营&广告商议后确定调用时机。*非必接，未上线前调用无效）
-(void)guideToAppStoreReview{
    [[RaStarCommon sharedInstance] guideToAppStoreReview];
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
            case eReview:
                [self guideToAppStoreReview];
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


