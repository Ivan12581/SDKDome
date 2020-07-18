#import "IOSBridgeHelper.h"
#import "AppleHelper.h"
#import "GoogleHelper.h"
#import "FBHelper.h"

#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import <FBSDKLoginKit/FBSDKLoginKit.h>
#import <GoogleSignIn/GoogleSignIn.h>
//******************************************************
//****************IOS中间文件
//******************************************************

@implementation IOSBridgeHelper
 - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {

     [super application:application didFinishLaunchingWithOptions:launchOptions];
     //Google 启动

//    FaceBook 启动调用必接
     [[FBSDKApplicationDelegate sharedInstance] application:application didFinishLaunchingWithOptions:launchOptions];
     [FBSDKSettings setAppID:@"949004278872387"];
     return YES;
 }

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary*)options{
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
    tNone,
    tApple,
    tGameCenter,
    tFacebook,
    tGoogle,//新增加请在Google、Rastar之间加入
    tRastar,

};

static IOSBridgeHelper *BridgeHelperIns = nil;
-(void)Init{
    if (BridgeHelperIns == nil) {
        BridgeHelperIns = [IOSBridgeHelper new];
    }
}

#pragma mark --init
-(void)InitSDK{
//    [self Init];
//    [[AppleHelper sharedInstance] InitSDK];
//    [[FBHelper sharedInstance] InitSDK];
    [[GoogleHelper sharedInstance] setDelegate:self];
    [[GoogleHelper sharedInstance] InitSDK];
    if (BridgeHelperIns == nil) {
        NSLog(@"-BridgeHelperIns == nil----");
    }
}
- (void)InitTestCallBack:(NSString *)dict {
    NSLog(@"-ios----IOSBridgeHelper---InitTestCallBack----");
      NSLog(@"---InitTestCallBack--->%@", dict);
}
-(void)InitSDKCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---InitSDKCallBack----");
    [BridgeHelperIns SendMessageToUnity: eInit DictData:dict];
}
#pragma mark -- 登录 jsonString 为登录方式 就是一个字符串
-(void)Login: (const char *) jsonString{
    NSLog(@"ios登录类型: %s", jsonString);
    
    NSInteger  type = [[NSString stringWithUTF8String:jsonString] integerValue];
    switch (type) {
        case tNone:
            break;
        case tApple:
            [[AppleHelper sharedInstance] Login];
                break;
        case tGameCenter:
            [[AppleHelper sharedInstance] authGamecnter];
                break;
        case tFacebook:
            [[FBHelper sharedInstance] Login];
                break;
        case tGoogle:
            [[GoogleHelper sharedInstance] Login];
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
    [dict setValue: [NSNumber numberWithInt:tFacebook] forKey: @"type"];
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
    [[AppleHelper sharedInstance] Pay:jsonString];
}

-(void)PayCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---PayCallBack----");
    [self SendMessageToUnity: ePay DictData:dict];
}
#pragma mark -- 上报数据
-(void)UploadInfo:(const char*) jsonData{

}
-(void)UploadInfoCallBack{
    
}
#pragma mark -- 展示客服界面
-(void)GetConfigInfo{

}
-(void)GetConfigInfoCallBack{
    
}
-(void)OpenService{

}
-(void)OpenServiceCallBack{
    
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
            case eNaver:
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
    void Call(int type, const char* jsonString){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Call:type andJsonStr:jsonString];
    }

//    void cInit(){
//        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate InitSDK];
//    }
//    void cLogin(const char* jsonString){
//        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Login:jsonString];
//    }
//    void cSwitch(){
//        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Switch];
//    }
//    void cPay(const char* jsonString){
//        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Pay:jsonString];
//    }
//    void cUpLoadInfo(const char* jsonString){
//        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate UploadInfo:jsonString];
//    }
//    void cOpenService(){
//        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate OpenService];
//    }
//    void cGetConfigInfo(){
//        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate GetConfigInfo];
//    }
//void cCustomerService(const char* jsonString){
//    [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate UploadInfo:jsonString];
//}
//void cShare(const char* jsonString){
//    [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate UploadInfo:jsonString];
//}

}


