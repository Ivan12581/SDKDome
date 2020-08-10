#import "IOSBridgeHelper.h"
#import "AppleHelper.h"
#import "GoogleHelper.h"
#import "FBHelper.h"
#import "ApplePurchase.h"
#import "AdjustHelper.h"
#import "LineHelper.h"
#import "ElvaHelper.h"
#import "Utils.h"

#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import <FBSDKLoginKit/FBSDKLoginKit.h>
#import <GoogleSignIn/GoogleSignIn.h>
#import <AdjustSdk/Adjust.h>
#import <ElvaChatServiceSDK/ElvaChatServiceSDK.h>
//******************************************************
//****************IOS中间文件
//******************************************************

@implementation IOSBridgeHelper
 - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {

     [super application:application didFinishLaunchingWithOptions:launchOptions];
     //Google 启动

     //Adjust 启动
     NSString *yourAppToken = @"1k2jm7bpansw";
     NSString *environment = ADJEnvironmentSandbox;
//     NSString *environment = ADJEnvironmentProduction;
     ADJConfig *adjustConfig = [ADJConfig configWithAppToken:yourAppToken
                                                 environment:environment];

     [Adjust appDidLaunch:adjustConfig];
     
     [adjustConfig setLogLevel:ADJLogLevelVerbose];
//     [adjustConfig setLogLevel:ADJLogLevelSuppress];
     //ad启动统计
     ADJEvent *event = [ADJEvent eventWithEventToken:@"4pvqgy"];
     [Adjust trackEvent:event];
     

     
//    FaceBook 启动调用必接
     [[FBSDKApplicationDelegate sharedInstance] application:application didFinishLaunchingWithOptions:launchOptions];
     [FBSDKSettings setAppID:@"949004278872387"];

     [self InitSDK];
     return YES;
 }

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary*)options{
//    return [[LineSDKLogin sharedInstance] handleOpenURL:url];
    return [[FBSDKApplicationDelegate sharedInstance] application:app openURL:url sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey] annotation:options[UIApplicationOpenURLOptionsAnnotationKey]];
}
- (void)applicationDidBecomeActive:(UIApplication *)application {
    [super applicationDidBecomeActive:application];
    [FBSDKAppEvents activateApp];
}
//- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
//    [super application:application openURL:url options:options];
//
//    BOOL handled = [[FBSDKApplicationDelegate sharedInstance] application:application openURL:url sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey] annotation:options[UIApplicationOpenURLOptionsAnnotationKey]];
//    return handled;
//}
extern void UnitySendMessage(const char *, const char *, const char *);
#pragma mark -- IOS To Unity
-(void)SendMessageToUnity:(int)msgID DictData:(NSMutableDictionary *) dict{
     NSLog(@"-ios----IOSBridgeHelper---SendMessageToUnity----");
    [dict setValue: [NSNumber numberWithInt:msgID] forKey: @"msgID"];
    NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:kNilOptions error:nil];
    NSString *jsonNSString = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    const char* jsonString = [jsonNSString UTF8String];
    UnitySendMessage("SDKManager", "OnResult", jsonString);
}


typedef NS_ENUM(NSInteger, SDKLoginType)
{
    tAccount,
    tSDKToken,//其实就是星辉
    tSuper,
    tGoogle,
    tApple,
    tFaceBook,
    tGameCenter,

};

#pragma mark --init
-(void)InitSDK{
    [[AppleHelper sharedInstance] setDelegate:self];
    [[AppleHelper sharedInstance] InitSDK];
    
    [[ApplePurchase sharedInstance] setDelegate:self];
    [[ApplePurchase sharedInstance] InitSDK];
    
    [[FBHelper sharedInstance] setDelegate:self];
    [[FBHelper sharedInstance] InitSDK];
    
    [[GoogleHelper sharedInstance] setDelegate:self];
    [[GoogleHelper sharedInstance] InitSDK];
    
    [[ElvaHelper sharedInstance] InitSDK];
}

-(void)InitSDKCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---InitSDKCallBack----");
    [self SendMessageToUnity: eInit DictData:dict];
}
#pragma mark -- 登录 jsonString 为登录方式 就是一个字符串
-(void)Login: (const char *) jsonString{
    NSLog(@"ios登录类型: %s", jsonString);
    NSInteger  type = [[NSString stringWithUTF8String:jsonString] integerValue];
    switch (type) {
        case tApple:
            [[AppleHelper sharedInstance] Login];
                break;
        case tGameCenter:
            [[AppleHelper sharedInstance] GamecnterLogin];
                break;
        case tFaceBook:
            [[FBHelper sharedInstance] Login];
                break;
        case tGoogle:
            [[GoogleHelper sharedInstance] Login];
                break;
        default:
            break;
    }
}
#pragma mark -- 登出
-(void)Logout: (const char *) jsonString{
    NSLog(@"ios登出类型: %s", jsonString);
    NSInteger  type = [[NSString stringWithUTF8String:jsonString] integerValue];
    switch (type) {
        case tApple:
            [[AppleHelper sharedInstance] Logout];
                break;
        case tGameCenter:
            [[AppleHelper sharedInstance] GameCenterLogout];
                break;
        case tFaceBook:
            [[FBHelper sharedInstance] Logout];
                break;
        case tGoogle:
            [[GoogleHelper sharedInstance] Logout];
                break;
        default:
            break;
    }
}
//cDelegate
-(void)AppleLoginCallBack:(NSMutableDictionary *) dict{
      NSLog(@"-ios----IOSBridgeHelper---LoginCallBack----");
    [dict setValue: [NSNumber numberWithInt:tApple] forKey: @"type"];
   [self SendMessageToUnity: eLogin DictData:dict];
}

-(void)LoginGameCenterCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---LoginGameCenterCallBack----");
    [dict setValue: [NSNumber numberWithInt:tGameCenter] forKey: @"type"];
   [self SendMessageToUnity: eLogin DictData:dict];
}
-(void)LoginGoogleCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---LoginGoogleCallBack----");
    [dict setValue: [NSNumber numberWithInt:tGoogle] forKey: @"type"];
   [self SendMessageToUnity: eLogin DictData:dict];
}
-(void)LoginFaceBookCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---LoginFaceBookCallBack----");
    [dict setValue: [NSNumber numberWithInt:tFaceBook] forKey: @"type"];
   [self SendMessageToUnity: eLogin DictData:dict];
}

#pragma mark -- 注销登录
-(void)Switch{

}
-(void)SwitchCallBack{
    
}
#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
    NSLog(@"-ios--Pay----");
     [[ApplePurchase sharedInstance] Pay:jsonString];
}

-(void)PayCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---PayCallBack----");
    [self SendMessageToUnity: ePay DictData:dict];
}
#pragma mark -- 获取设备UUID
-(void)GetDeviceId{
    NSString *UUID = [[Utils sharedInstance] GetUUID];;
     NSLog(@"-ios----didFinishLaunchingWithOptions---UUID----%@",UUID);
    [self SendMessageToUnity: eGetDeviceId DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",UUID, @"UUID",nil]];
}

#pragma mark -- 展示客服界面
-(void)CustomerService:(const char*) jsonData{
    [[ElvaHelper sharedInstance] show:jsonData];
}
-(void)CustomerServiceCallBack{
    
}
#pragma mark -- FaccBook分享
-(void)FaceBookShare:(const char*) jsonData{
    [[FBHelper sharedInstance] share:jsonData];
}
-(void)FaceBookShareCallBack:(NSMutableDictionary *) dict{
    
}
#pragma mark -- Line分享
-(void)LineShare:(const char*) jsonData{
    [[LineHelper sharedInstance] share:jsonData];
}
-(void)LineShareCallBack:(NSMutableDictionary *) dict{
    
}
#pragma mark -- FaccBook统计
-(void)FaceBookEvent:(const char*) jsonData{
    [[FBHelper sharedInstance] Event:jsonData];
}
#pragma mark -- Adjust统计
-(void)AdjustEvent:(const char*) jsonData{
    [[AdjustHelper sharedInstance] Event:jsonData];
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
    
    eWeiboShare = 301,
    eFaceBookShare = 302,
    eLineShare = 303,
    eConsumeGoogleOrder = 401,
    eCustomerService = 501,
    eFaceBookEvent = 601,
    eAdjustEvent = 602,
};
#pragma mark -- Unity To IOS
-(void)Call:(int) type andJsonStr:(const char*) jsonstring{
    NSLog(@"-ios----IOSBridgeHelper---Call----%i",type);
        switch (type) {
            case eInit:
                [self InitSDK];
                break;
            case eLogin:
                [self Login:jsonstring];
                break;
            case ePay:
                [self Pay:jsonstring];
                break;
            case eShare:
                break;
            case eGetDeviceId:
                [self GetDeviceId];
                break;
            case eFaceBookShare:
                break;
            case eLineShare:
                break;
            case eCustomerService:
                break;
            case eFaceBookEvent:
                break;
            case eAdjustEvent:
                break;
            default:
            NSLog(@"-ios----IOSBridgeHelper---该接口ios未实现----%i",type);
                break;
        }
}
@end

IMPL_APP_CONTROLLER_SUBCLASS (IOSBridgeHelper)

extern "C"
{
    void CallFromUnity(int type, const char* jsonString){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Call:type andJsonStr:jsonString];
    }
}


