#import "IOSBridgeHelper.h"
#import "AppleHelper.h"
//#import <FBSDKCoreKit/FBSDKCoreKit.h>
//#import <FBSDKLoginKit/FBSDKLoginKit.h>

//******************************************************
//****************IOS中间文件
//******************************************************

@implementation IOSBridgeHelper
// - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    // NSLog(@"-ios---IOSBridgeHelper---application--000-");
    // [super application:application didFinishLaunchingWithOptions:launchOptions];
    
    ////FaceBook 启动调用必接
    // [[FBSDKApplicationDelegate sharedInstance] application:application didFinishLaunchingWithOptions:launchOptions];
    // [FBSDKSettings setAppID:@"949004278872387"];
    // return YES;
// }
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
//+(IOSBridgeHelper*)sharedInstance{
//    if (IOSBridgeHelperInstance == nil) {
//        if (self == [IOSBridgeHelper class]) {
//            IOSBridgeHelperInstance = [[self alloc] init];
//        }
//    }
//    return IOSBridgeHelperInstance;
//}

-(void)Init{
    if (BridgeHelperIns == nil) {
//        if (self == [IOSBridgeHelper class]) {
            BridgeHelperIns = [IOSBridgeHelper new];
//            IOSBridgeHelperInstance = [[self alloc] init];
//        }
    }
}

#pragma mark --init
-(void)InitSDK{
    [self Init];
    [[AppleHelper sharedInstance] InitSDK];
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
                break;
        case tGoogle:
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
-(void)Pay{
    NSLog(@"-ios--Pay----");
//    [[AppleHelper sharedInstance] Pay:jsonString];
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


