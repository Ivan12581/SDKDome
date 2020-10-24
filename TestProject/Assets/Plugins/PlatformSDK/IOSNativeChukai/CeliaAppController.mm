
#import "UnityAppController.h"
#import <ChuKaiSDK/ChuKaiCommon.h>

@interface CeliaAppController : UnityAppController<ChuKaiInitDelegate,ChuKaiLoginDelegate,ChuKaiPayDelegate>

@end

@implementation CeliaAppController

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    
    [[ChuKaiCommon sharedInstance] setScreenOrientation:UIInterfaceOrientationMaskPortrait];
    
    [self InitSDK];
    [self showToast: @"Celia 程序启动成功"];
    return YES;
}

//-(BOOL)shouldAutorotate{
//    NSLog(@"yes");
//    return [[ChuKaiCommon sharedInstance] shouldAutorotate:GetAppController().unityView];
//}

- (UIInterfaceOrientationMask)supportedInterfaceOrientations{
    return [[ChuKaiCommon sharedInstance] supportedInterfaceOrientations];
}

BOOL isDebug = [[[[NSMutableDictionary alloc] initWithContentsOfFile:[[NSBundle mainBundle] pathForResource:@"Rastar" ofType:@"plist"]] valueForKey:@"DeBug"] isEqual:@"1"];
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
#pragma mark -- 初始化
-(void)InitSDK{
    NSString *plistPath = [[NSBundle mainBundle] pathForResource:@"Rastar" ofType:@"plist"];
    NSMutableDictionary *data=[[NSMutableDictionary alloc] initWithContentsOfFile:plistPath];
    
    [[ChuKaiCommon sharedInstance] registerSDKWithAppKey:[data valueForKey:@"App Key"] andAppid:[data valueForKey:@"App ID"] cch_ID:[data valueForKey:@"Cch ID"] md_ID:[data valueForKey:@"Md ID"]];
    [[ChuKaiCommon sharedInstance] addInitDelegate:self];
    
    [self showToast: [NSString stringWithFormat:@"Celia DeviceID：%@ App Key：%@ App ID：%@ Cch ID：%@ Md ID：%@", [ChuKaiCommon sharedInstance].deviceID, [data valueForKey:@"App Key"], [data valueForKey:@"App ID"], [data valueForKey:@"Cch ID"], [data valueForKey:@"Md ID"]]];
}
- (void)onInitSuccess {
    [self showToast: @"Celia 初始化成功"];
    [self SendMessageToUnity: eInit DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
}
- (void)onInitFail {
    [self showToast: @"Celia 初始化失败"];
    [self SendMessageToUnity: eInit DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",nil]];
}

#pragma mark -- 登录
-(void)Login{
    [self showToast: @"Celia 打开登陆"];
    [[ChuKaiCommon sharedInstance] showLoginView];
    [[ChuKaiCommon sharedInstance] addLoginDelegate:self];
}
-(void)onCommonLoginSuccess:(NSString *)token{
    [self showToast: [NSString stringWithFormat:@"Celia 登陆成功 %@", token]];
    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",token,@"token",nil]];
}
-(void)onCommonLoginFail{
    [self showToast: @"Celia 登陆失败"];
    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:0], @"state",nil]];
}

#pragma mark -- 注销登录
-(void)Switch{
    [self showToast: @"Celia 注销"];
    [[ChuKaiCommon sharedInstance] loginViewOut];
}
-(void)onCommonLoginOut{
    [self showToast: @"Celia 注销成功"];
    [self SendMessageToUnity: eLogout DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state",nil]];
    [[ChuKaiCommon sharedInstance] showLoginView];
}

#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
    // const char* --> nnstring
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    // nsstring -> nsdata
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    // nsdata -> nsdictionary
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    
//    NSDate * datenow = [NSDate date];
//    NSString * timeSp = [NSString stringWithFormat:@"%ld", (long)[datenow timeIntervalSince1970] + arc4random() % 100000];
    
    [[ChuKaiCommon sharedInstance] addPayDelegate:self];
    [[ChuKaiCommon sharedInstance] 
        payWithAmount:[dict valueForKey:@"PayMoney"] 
        andOrder_no:[dict valueForKey:@"OrderID"] 
        andSubject:[dict valueForKey:@"OrderName"] 
        andRoleid:[dict valueForKey:@"RoleID"] 
        andName:[dict valueForKey:@"RoleName"] 
        andLevel:[dict valueForKey:@"RoleLevel"] 
        andSid:[dict valueForKey:@"ServerID"] 
        andSname:[dict valueForKey:@"ServerName"] 
        andExt:@"Extra"];
}

-(void)onPayState:(ChukaiCode)RSPaycode{
    [self showToast: [NSString stringWithFormat:@"%ld", (long)RSPaycode]];
    [self SendMessageToUnity: ePay DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:[NSString stringWithFormat:@"%ld", (long)RSPaycode]], @"state",nil]];
}

#pragma mark -- 上报数据
-(void)UploadInfo:(const char*) jsonData{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonData];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    
    int uploadType = [dict[@"UploadType"] intValue];
    switch (uploadType) {
        case 0:// 角色创建
            [[ChuKaiCommon sharedInstance] uploadCreateInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"Extra"]];
            break;
        case 1:// 进入游戏
            [[ChuKaiCommon sharedInstance] uploadEnterInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"Extra"]];
            break;
        case 2:// 角色升级
            [[ChuKaiCommon sharedInstance] uploadUp_levelInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"Extra"]];
            break;
        case 3:// 完成新手
            break;
        case 4:// 更名
            [[ChuKaiCommon sharedInstance] uploadUpdateInfoRoleID:[dict valueForKey:@"RoleID"] roleName:[dict valueForKey:@"RoleName"] roleLevel:[dict valueForKey:@"RoleLevel"] serverID:[dict valueForKey:@"ServerID"] serverName:[dict valueForKey:@"ServerName"] balance:[dict valueForKey:@"Balance"] vip:[dict valueForKey:@"VIPLevel"] partyName:[dict valueForKey:@"PartyName"] extra:[dict valueForKey:@"OldName"]];
            break;
    }
}

#pragma mark -- 展示客服界面
-(void)OpenService{
    
}
-(void)serviceClose{
    
}

-(void)GetConfigInfo{
    [self showToast: [NSString stringWithFormat:@"获取参数"]];
    
    NSString *plistPath = [[NSBundle mainBundle] pathForResource:@"Rastar" ofType:@"plist"];
    NSMutableDictionary *data=[[NSMutableDictionary alloc] initWithContentsOfFile:plistPath];

    [self SendMessageToUnity: eConfigInfo DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithInt:1], @"state", [ChuKaiCommon sharedInstance].deviceID, @"deviceID", [data valueForKey:@"Cch ID"], @"cchID", [data valueForKey:@"App ID"], @"appID", [data valueForKey:@"Md ID"], @"mdID",nil]];
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


