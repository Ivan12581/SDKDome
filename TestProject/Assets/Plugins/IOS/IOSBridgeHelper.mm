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

@implementation IOSBridgeHelper{
        NSInteger  CurLoginType;//登陆类型
}
 - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {

     [super application:application didFinishLaunchingWithOptions:launchOptions];
     //Google 启动

     //Adjust 启动
     NSString *yourAppToken = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"AdjustAppToken"];
     NSString *environment = ADJEnvironmentSandbox;
//     NSString *environment = ADJEnvironmentProduction;
     ADJConfig *adjustConfig = [ADJConfig configWithAppToken:yourAppToken environment:environment];
     [adjustConfig setAppSecret:1 info1:750848352 info2:1884995334 info3:181661496 info4:1073918938];
      [adjustConfig setLogLevel:ADJLogLevelVerbose];
 //     [adjustConfig setLogLevel:ADJLogLevelSuppress];
     [adjustConfig setSendInBackground:YES];
     [Adjust appDidLaunch:adjustConfig];



     
//    FaceBook 启动调用必接
     [[FBSDKApplicationDelegate sharedInstance] application:application didFinishLaunchingWithOptions:launchOptions];

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
    [[AppleHelper sharedInstance] InitSDK:self];
    [[ApplePurchase sharedInstance] InitSDK:self];
    [[FBHelper sharedInstance] InitSDK:self];
    [[GoogleHelper sharedInstance] InitSDK:self];
    [[LineHelper sharedInstance] InitSDK:self];
    [[ElvaHelper sharedInstance] InitSDK:self];
    [[AdjustHelper sharedInstance] InitSDK:self];
}

#pragma mark -- 登录 jsonString 为登录方式 就是一个字符串
-(void)Login: (const char *) jsonString{
    NSLog(@"ios登录类型: %s", jsonString);
    NSInteger  type = [[NSString stringWithUTF8String:jsonString] integerValue];
    CurLoginType = type;
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

#pragma mark -- 获取设备UUID
-(void)GetDeviceId{
    NSString *UUID = [[Utils sharedInstance] GetUUID];;
     NSLog(@"-ios----didFinishLaunchingWithOptions---UUID----%@",UUID);
    [self SendMessageToUnity: eGetDeviceId DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",UUID, @"UUID",nil]];
}
//cDelegate
-(void)InitSDKCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---InitSDKCallBack----");
    [self SendMessageToUnity: eInit DictData:dict];
}
-(void)LoginCallBack:(NSMutableDictionary *) dict{
       NSLog(@"-ios----IOSBridgeHelper---LoginCallBack----");
     [dict setValue: [NSNumber numberWithInt:(int)CurLoginType] forKey: @"type"];
    [self SendMessageToUnity: eLogin DictData:dict];
}
-(void)PayCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---PayCallBack----");
    [self SendMessageToUnity: ePay DictData:dict];
}
-(void)CustomerServiceCallBack{
    
}

-(void)FaceBookShareCallBack:(NSMutableDictionary *) dict{
    
}

-(void)LineShareCallBack:(NSMutableDictionary *) dict{
    
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
                [[ApplePurchase sharedInstance] Pay:jsonstring];
                break;
            case eGetDeviceId:
                [self GetDeviceId];
                break;
                case eShare:
                    [[FBHelper sharedInstance] share:jsonstring];
                    break;
            case eFaceBookShare:
                [[FBHelper sharedInstance] share:jsonstring];
                break;
            case eLineShare:
                [[LineHelper sharedInstance] share:jsonstring];
                break;
            case eCustomerService:
                [[ElvaHelper sharedInstance] show:jsonstring];
                break;
            case eFaceBookEvent:
                [[FBHelper sharedInstance] Event:jsonstring];
                break;
            case eAdjustEvent:
                [[AdjustHelper sharedInstance] Event:jsonstring];
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


