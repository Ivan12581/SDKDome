#import "IOSBridgeHelper.h"
#import "AppleHelper.h"
#import "GoogleHelper.h"
#import "FBHelper.h"
#import "GVC.h"
#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import <FBSDKLoginKit/FBSDKLoginKit.h>
#import "BYJumpEachOther.h"
#import <GoogleSignIn/GoogleSignIn.h>
//******************************************************
//****************IOS中间文件
//******************************************************

@implementation IOSBridgeHelper
 - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
     NSLog(@"-ios---IOSBridgeHelper---application--000-");
      NSLog(@"-ios---IOSBridgeHelper---application--000-");
      NSLog(@"-ios---IOSBridgeHelper---application--000-");
      NSLog(@"-ios---IOSBridgeHelper---application--000-");
     [super application:application didFinishLaunchingWithOptions:launchOptions];
     //Google 启动

     
         [GIDSignIn sharedInstance].clientID = @"589453917038-qaoga89fitj2ukrsq27ko56fimmojac6.apps.googleusercontent.com";

//     self.window = [[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
//     GoogleHelper *masterViewController =
//         [[GoogleHelper alloc] initWithNibName:@"GoogleHelper"
//                                                bundle:nil];
//     self.navigationController =
//         [[UINavigationController alloc]
//             initWithRootViewController:masterViewController];
//     self.window.rootViewController = self.navigationController;
//     [self.window makeKeyAndVisible];
//     [GIDSignIn sharedInstance].delegate = self;
     
//    FaceBook 启动调用必接
     [[FBSDKApplicationDelegate sharedInstance] application:application didFinishLaunchingWithOptions:launchOptions];
     [FBSDKSettings setAppID:@"949004278872387"];
     return YES;
 }

- (BOOL)application:(UIApplication *)app
            openURL:(NSURL *)url
            options:(NSDictionary<NSString *, id> *)options {
  return [[GIDSignIn sharedInstance] handleURL:url];
}

- (BOOL)application:(UIApplication *)application
            openURL:(NSURL *)url
  sourceApplication:(NSString *)sourceApplication
         annotation:(id)annotation {
  return [[GIDSignIn sharedInstance] handleURL:url];
}

extern void UnitySendMessage(const char *, const char *, const char *);

-(void)SendMessageToUnity:(int)msgID DictData:(NSMutableDictionary *) dict{
     NSLog(@"-ios----IOSBridgeHelper---SendMessageToUnity----");
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
    [self Init];
//    [[AppleHelper sharedInstance] InitSDK];
//     [[GVC sharedInstance] InitSDK];
//    [[GoogleHelper sharedInstance] InitSDK];
    if (BridgeHelperIns == nil) {
        NSLog(@"-BridgeHelperIns == nil----");
    }
}

+(void)InitSDKCallBack:(NSMutableDictionary *) dict{
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
            
            [[GVC sharedInstance] Login];
//            [[BYJumpEachOther sharedInstance] setupIOS];
//            [[GoogleHelper sharedInstance] Login];
                break;
        default:
            break;
    }

    
}
+(void)LoginCallBack:(NSMutableDictionary *) dict{
      NSLog(@"-ios----IOSBridgeHelper---LoginCallBack----");
    [dict setValue: [NSNumber numberWithInt:tApple] forKey: @"type"];
   [BridgeHelperIns SendMessageToUnity: eLogin DictData:dict];
}
-(void)LoginGameCenter{
      [[AppleHelper sharedInstance] authGamecnter];
}
+(void)LoginGameCenterCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---LoginCallBack----");
    [dict setValue: [NSNumber numberWithInt:tGameCenter] forKey: @"type"];
   [BridgeHelperIns SendMessageToUnity: eLogin DictData:dict];
}

#pragma mark -- 注销登录
-(void)Switch{

}
+(void)SwitchCallBack{
    
}
#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
    NSLog(@"-ios--Pay----");
    [[AppleHelper sharedInstance] Pay:jsonString];
}

+(void)PayCallBack:(NSMutableDictionary *) dict{
    NSLog(@"-ios----IOSBridgeHelper---PayCallBack----");
    [BridgeHelperIns SendMessageToUnity: ePay DictData:dict];
}
#pragma mark -- 上报数据
-(void)UploadInfo:(const char*) jsonData{

}
+(void)UploadInfoCallBack{
    
}
#pragma mark -- 展示客服界面
-(void)GetConfigInfo{

}
+(void)GetConfigInfoCallBack{
    
}
-(void)OpenService{

}
+(void)OpenServiceCallBack{
    
}

@end

IMPL_APP_CONTROLLER_SUBCLASS (IOSBridgeHelper)

extern "C"
{
    void cInit(){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate InitSDK];
    }
    void cLogin(const char* jsonString){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Login:jsonString];
    }
    void cSwitch(){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Switch];
    }
    void cPay(const char* jsonString){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Pay:jsonString];
    }
    void cUpLoadInfo(const char* jsonString){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate UploadInfo:jsonString];
    }
    void cOpenService(){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate OpenService];
    }
    void cGetConfigInfo(){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate GetConfigInfo];
    }

}


