#import "IOSBridgeHelper.h"
#import "PayInApple.h"
#import "AppleHelper.h"
//******************************************************
//****************IOS中间文件
//******************************************************

@implementation IOSBridgeHelper

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

+(void)InitSDKCallBack{
    NSLog(@"-ios----IOSBridgeHelper---InitSDKCallBack----");
    [BridgeHelperIns SendMessageToUnity: eInit DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",@"13456",@"user", @"",@"identityTokenStr",nil]];
}
#pragma mark -- 登录
-(void)Login{
      [[AppleHelper sharedInstance] Login];
}
+(void)LoginCallBack:(NSMutableDictionary *) dict{
      NSLog(@"-ios----IOSBridgeHelper---LoginCallBack----");
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
    void cLogin(){
        [(IOSBridgeHelper*)[UIApplication sharedApplication].delegate Login];
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


