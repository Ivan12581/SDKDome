
#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import <NaverCafeToolSDK/NaverCafeTool.h>
#import <RaStarOverseaSDK/RaStarCommom.h>
#import <LiveOpsToolSDK/LiveOpsPushTool.h>
#import <LiveOps/LiveOps.h>
#import <AbroadLineShareSDK/AbroadLineShareTool.h>

#import "UnityAppController.h"

@interface CeliaAppController : UnityAppController<RaStarLoginDelegate , RaStarPayDelegate , RaStarBindDelegate,RaStarFBShareDelegate,UICollectionViewDelegate,UIApplicationDelegate>

@end

@implementation CeliaAppController

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    
    #warning 先调用AF 数据统计激活 再调SDK初始化
    [self InitSDK];
    //FaceBook 启动调用必接
    [[FBSDKApplicationDelegate sharedInstance] application:application didFinishLaunchingWithOptions:launchOptions];
    //初始化推送
    [[LiveOpsPushTool sharePushTool] initPushWithLaunchOptions:launchOptions];
    //设置推送用户ID
    [[LiveOpsPushTool sharePushTool] setUserId:[RaStarCommom sharedInstance].deviceID];

//    //初始化接口
//    [self initNaverCafe];
    return YES;
}

// LiveOps
- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    //登录 Device Token
    [LiveOpsPush setDeviceToken:deviceToken];
    [self showToast: [NSString stringWithFormat:@"CLiveOpsPush登录成功 deviceToken %@",deviceToken]];
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError *)error {
    //Device Token 登录失败 Log 确认
    [self showToast: [NSString stringWithFormat:@"LiveOpsPush登录失败 Device Token Register Failed: %@", error]];
}

#if __IPHONE_OS_VERSION_MAX_ALLOWED < __IPHONE_7_0
#warning "Remote push open tracking is counted only when user touches notification center under iOS SDK 7.0"

// iOS version7 以下时
- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary *)userInfo
{
    [super application:application didReceiveRemoteNotification:userInfo];
    [LiveOpsPush handleRemoteNotification:userInfo fetchHandler:nil];
}
#else
// iOS version7 以上时
- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult))completionHandler
{
    [super application:application didReceiveRemoteNotification:userInfo fetchCompletionHandler:completionHandler];
    [LiveOpsPush handleRemoteNotification:userInfo fetchHandler:completionHandler];
    
    [self showToast: @"收到推送消息"];
}
#endif

//Local Push Handler
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification *)notification
{
    [super application:application didReceiveLocalNotification:notification];
    [LiveOpsPush handleLocalNotification:notification];
}

// AF
//AF 报告激活深度链接
- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray * _Nullable))restorationHandler
{
    [super application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
    [[RaStarCommom sharedInstance] afContinueUserActivity:userActivity restorationHandler:restorationHandler];
    return YES;
}
//iOS 10 以下
- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url
  sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
{
    BOOL handled = [[FBSDKApplicationDelegate sharedInstance] application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    // 在此添加任意自定义逻辑。
    [[RaStarCommom sharedInstance] enterForegroundWithApplication:application url:url];
    //AF追踪打开链接
    [[RaStarCommom sharedInstance] afOpenURL:url sourceApplication:sourceApplication annotation:annotation];

//    //使用Naver账号将登陆信息设置到登录客体。（有接入NaverCafe 才需要接入）
//    [[NaverCafeTool shareNaverCafeTool] finishNaverLoginWithURL:url];

    return handled;
}
//iOS 10 以上
- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
    [super application:application openURL:url options:options];

    BOOL handled = [[FBSDKApplicationDelegate sharedInstance] application:application openURL:url sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey] annotation:options[UIApplicationOpenURLOptionsAnnotationKey]];
    // 在此添加任意自定义逻辑。
    [[RaStarCommom sharedInstance] enterForegroundWithApplication:application url:url];
    //AF追踪打开链接
    [[RaStarCommom sharedInstance] afOpenURL:url options:options];

    return handled;
}

- (BOOL)application:(UIApplication* )application handleOpenURL:(NSURL* )url {
    [[RaStarCommom sharedInstance] enterForegroundWithApplication:application url:url];
    return YES;
}

- (void)applicationDidBecomeActive:(UIApplication *)application {
    [super applicationDidBecomeActive:application];
    [RaStarCommom trackAppLaunch];
}

#warning 需要在根控制器设置屏幕支持方向
- (UIInterfaceOrientationMask)supportedInterfaceOrientations{
    return UIInterfaceOrientationMaskPortrait;
}

BOOL isDebug = [[[[NSMutableDictionary alloc] initWithContentsOfFile:[[NSBundle mainBundle] pathForResource:@"RSOverseaSDK" ofType:@"plist"]] valueForKey:@"AppsFlyer_isDeBug"] isEqual:@"1"];
- (void)dismiss:(UIAlertController *)alert{
    [alert dismissViewControllerAnimated:YES completion:nil];
}
- (void)showToast:(NSString *)msg{
    if (not isDebug)
        return;
    
    UIAlertController *alert = [UIAlertController alertControllerWithTitle:msg message:nil preferredStyle:UIAlertControllerStyleAlert];
    [UnityGetGLViewController() presentViewController:alert animated:YES completion:nil];
    [self performSelector:@selector(dismiss:) withObject:alert afterDelay:2.0];
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
    eInit,
    eLogin,
    eSwitch,
    ePay,
    eUploadInfo,
    eExitGame,
    eLogout,
    
    eConfigInfo,
    eGoogleTranslate,
    eBind,
    eShare,
    eNaver,
};

// SDK回调
#pragma mark -- 初始化(无回调)
-(void)InitSDK{
    [RaStarCommom setAppsFlyer];
    [RaStarCommom setUserAgent];
    
    [self SendMessageToUnity: eInit DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
    [self showToast: @"初始化"];
}

#pragma mark -- 登录
-(void)Login{
    //登录接口
    [[RaStarCommom sharedInstance] loginWithDelegate:self];
    //添加绑定协议
    [[RaStarCommom sharedInstance] setRSBindDelegate:self];
}
#pragma mark - 绑定回调
- (void)onBindSeccess:(NSDictionary *)infoDic{
    [self showToast: [NSString stringWithFormat:@"绑定成功：%@",infoDic]];
}

#pragma mark - 登录回调
- (void)onLoginSuccess:(NSString *)accessToken BindInfo:(NSDictionary *)infoDic{
    //展示悬浮球 默认登录成功不展示悬浮球
    [[RaStarCommom sharedInstance] showMenuCenter];
    //隐藏悬浮球
    //[[RaStarCommom sharedInstance] hidenMenuCenter];
    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",accessToken,@"token",nil]];
    [self showToast: [NSString stringWithFormat:@"登录成功：%@",accessToken]];
}
- (void)onLoginFail{
    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",nil]];
    [self showToast: [NSString stringWithFormat:@"登录失败"]];
}

#pragma mark -- 注销登录
-(void)Switch{
    [[RaStarCommom sharedInstance] showSingleViewWithType:RSFunctionTypeSwitchAccount];
    [self showToast: [NSString stringWithFormat:@"注销登录"]];
}
- (void)onLogout {
    [self SendMessageToUnity: eLogout DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
    [self showToast: [NSString stringWithFormat:@"注销登录成功"]];
}
- (void)onCloseLoginView {
    [self SendMessageToUnity: eLogout DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",nil]];
    [self showToast: [NSString stringWithFormat:@"关闭登录界面"]];
}

#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
    // const char* --> nnstring
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    // nsstring -> nsdata
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    // nsdata -> nsdictionary
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    
    [[RaStarCommom sharedInstance]
         payWithAmount:[dict valueForKey:@"PayMoney"]
         Name:[dict valueForKey:@"RoleName"]
         Level:[dict valueForKey:@"RoleLevel"]
         Roleid:[dict valueForKey:@"RoleID"]
         Sid:[dict valueForKey:@"ServerID"]
         Sname:[dict valueForKey:@"ServerName"]
         Subject:[dict valueForKey:@"OrderName"]
         OrderNum:[dict valueForKey:@"OrderID"]
         Ext:[dict valueForKey:@"Extra"]
         Delegate:self];
}
#pragma mark - 支付回调
- (void)onPaySuccess:(NSString *)orderID{
    [self SendMessageToUnity: ePay DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",orderID,@"orderID",nil]];
}
- (void)onPayFailure:(NSString *)failure{
    [self SendMessageToUnity: ePay DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",failure,@"msg",nil]];
}
- (void)onPayCancel{
    [self SendMessageToUnity: ePay DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:2], @"state",nil]];
}

#pragma mark -- 上报数据
-(void)UploadInfo:(const char*) jsonData{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonData];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    
    int uploadType = [dict[@"UploadType"] intValue];
    switch (uploadType) {
        case 0:// 角色创建
            [RaStarCommom collectCreateRoleWithRoleID:[dict valueForKey:@"RoleID"] RoleLevel:[dict valueForKey:@"RoleLevel"] RoleName:[dict valueForKey:@"RoleName"] ServerId:[dict valueForKey:@"ServerID"] ServerName:[dict valueForKey:@"ServerName"] PartyName:[dict valueForKey:@"PartyName"] TimeLevelUp:@"-1" Vip:[dict valueForKey:@"VIPLevel"] TimeCreate:@"-1" Balance:[dict valueForKey:@"Balance"] Extra:[dict valueForKey:@"Extra"]];
            break;
        case 1:// 进入游戏
            [RaStarCommom collectEnterServerWithRoleID:[dict valueForKey:@"RoleID"] RoleLevel:[dict valueForKey:@"RoleLevel"] RoleName:[dict valueForKey:@"RoleName"] ServerId:[dict valueForKey:@"ServerID"] ServerName:[dict valueForKey:@"ServerName"] PartyName:[dict valueForKey:@"PartyName"] TimeLevelUp:@"-1" Vip:[dict valueForKey:@"VIPLevel"] TimeCreate:@"-1" Balance:[dict valueForKey:@"Balance"] Extra:[dict valueForKey:@"Extra"]];
            break;
        case 2:// 角色升级
            [RaStarCommom collectLevelUpWithRoleID:[dict valueForKey:@"RoleID"] RoleLevel:[dict valueForKey:@"RoleLevel"] RoleName:[dict valueForKey:@"RoleName"] ServerId:[dict valueForKey:@"ServerID"] ServerName:[dict valueForKey:@"ServerName"] PartyName:[dict valueForKey:@"PartyName"] TimeLevelUp:@"-1" Vip:[dict valueForKey:@"VIPLevel"] TimeCreate:@"-1" Balance:[dict valueForKey:@"Balance"] Extra:[dict valueForKey:@"Extra"]];
            break;
        case 3:// 完成新手
            break;
        case 4:// 更名
            break;
    }
}

#pragma mark -- 展示客服界面
-(void)OpenService{
    [[RaStarCommom sharedInstance] showCustomerService:^{
        [self showToast: [NSString stringWithFormat:@"客服界面关闭"]];
    }];
}

//分享链接
- (void)shareLinkAction {
    //测试分享地址
    [[RaStarCommom sharedInstance] shareWithDelegate:self];
    [[RaStarCommom sharedInstance] shareWithLink:@"http://www.baidu.com" fromViewController:UnityGetGLViewController()];
}

//分享图片
- (void)shareImageAction {
    //测试分享照片
    [[RaStarCommom sharedInstance] shareWithDelegate:self];
    [[RaStarCommom sharedInstance] shareWithImage:[UIImage imageNamed:@"Defatult-568"] fromViewController:UnityGetGLViewController()];
}

//分享视频
- (void)shareVideoAction {
    //测试分享视频
    [[RaStarCommom sharedInstance] shareWithDelegate:self];
    [[RaStarCommom sharedInstance] shareVideoWithFromViewController:UnityGetGLViewController()];
}

#pragma mark - 分享回调
- (void)fbShareCancel {
    [self showToast: [NSString stringWithFormat:@"FB取消分享"]];
}
- (void)fbShareFailure {
    [self showToast: [NSString stringWithFormat:@"FB分享失败"]];
}
- (void)fbShareSuccess {
    [self showToast: [NSString stringWithFormat:@"FB分享成功"]];
}

//LiveOps本地推送
- (void)localPushAction {
    NSDate *date = [NSDate dateWithTimeIntervalSince1970:time(NULL) + 10];
    //本地推送
    [[LiveOpsPushTool sharePushTool] registerLocalPushNotification:100003 date:date body:@"这是一条本地推送服务测试"];
    [self showToast: [NSString stringWithFormat:@"LiveOps本地推送"]];
}

//绑定界面
- (void)showBindView {
    [[RaStarCommom sharedInstance] showSingleViewWithType:RSFunctionTypeBind];
}

//Naver Cafe 初始化接口
- (void)initNaverCafe {
    //初始化接口
    [[NaverCafeTool shareNaverCafeTool] initWithLoginClientId:@"Vov2CmKCe0LYQIumH_Dd" loginClientSecret:@"F_yYqEe8IU" cafeId:29496130 loginURLScheme:@"COMGUANGHUANCZJYKR" parentViewController:self showScreenShot:YES showTransparentSlider:NO showFloatingBall:YES];
    
    [[NaverCafeTool shareNaverCafeTool] addCafeToolDelegate:self];
}
//Naver Cafe 主页
- (void)naverCafeHomeView {
    //如果是横屏游戏需要调用以下接口
//    [[UIApplication sharedApplication] statusBarOrientation];
//    if ([[UIApplication sharedApplication] statusBarOrientation] == UIInterfaceOrientationLandscapeLeft || [[UIApplication sharedApplication] statusBarOrientation] == UIInterfaceOrientationLandscapeRight) {
//        [[NaverCafeTool shareNaverCafeTool] setUIInterfaceOrientationIsLandscape];
//    }
    //加载主页
    [[NaverCafeTool shareNaverCafeTool] presentNaverCafeSDKViewWithType:NaverCafeSDKViewHome];
}
#pragma mark - NaverCafeToolDelegate
- (void)ncSDKToolViewDidLoad {
    [self showToast: [NSString stringWithFormat:@"NaverCafe回调展示界面"]];
}
- (void)ncSDKToolViewDidUnLoad {
    [self showToast: [NSString stringWithFormat:@"NaverCafe回调关闭界面"]];
}
- (void)ncSDKToolWidgetPostArticleWithImage {
    [self showToast: [NSString stringWithFormat:@"NaverCafe回调截图成功"]];
}

- (void)ncSDKToolPostedArticleAtMenuSuccess {
    [self showToast: [NSString stringWithFormat:@"NaverCafe上传帖子成功"]];
}

//Line分享相关
- (void)lineShareURL:(const char*) url{
    [self showToast: [NSString stringWithFormat:@"line分享链接"]];
    [[AbroadLineShareTool lineShareTool] shareWithURL:@"https://www.baidu.com"];
}
- (void)lineShareImage:(const char*) imageName{
    [self showToast: [NSString stringWithFormat:@"line分享图片"]];
    [[AbroadLineShareTool lineShareTool] shareWithImage:[UIImage imageNamed:[NSString stringWithUTF8String:imageName]]];
}

-(void)GetConfigInfo{
    [self showToast: [NSString stringWithFormat:@"获取参数"]];
    
    NSString *plistPath = [[NSBundle mainBundle] pathForResource:@"RSOverseaSDK" ofType:@"plist"];
    NSMutableDictionary *data=[[NSMutableDictionary alloc] initWithContentsOfFile:plistPath];
    
    [self SendMessageToUnity: eConfigInfo DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state", [RaStarCommom sharedInstance].deviceID, @"deviceID", [data valueForKey:@"RS_cch_ID"], @"cchID", [data valueForKey:@"RS_AppID"], @"appID", [data valueForKey:@"RS_md_ID"], @"mdID",nil]];
}

@end
IMPL_APP_CONTROLLER_SUBCLASS (CeliaAppController)

extern "C"
{
    void cInit(){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate InitSDK];
    }
    void cLogin(){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate Login];
    }
    void cSwitch(){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate Switch];
    }
    void cPay(const char* jsonString){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate Pay:jsonString];
    }
    void cUpLoadInfo(const char* jsonString){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate UploadInfo:jsonString];
    }
    void cOpenService(){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate OpenService];
    }
    void cGetConfigInfo(){
        [(CeliaAppController*)[UIApplication sharedApplication].delegate GetConfigInfo];
    }
}


