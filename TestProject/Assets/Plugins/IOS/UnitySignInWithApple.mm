
#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import "UnityAppController.h"
#import <UIKit/UIKit.h>
#import "SSKeychain.h"
#import <GameKit/GameKit.h>
#import "PayInApple.h"
//******************************************************
//****************Apple 登录
//******************************************************
//API_AVAILABLE(ios(13.0), tvos(13.0))
//@interface UnitySignInWithApple : NSObject<ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>
//
////@property (nonatomic) SignInWithAppleCallback loginCallback;
////@property (nonatomic) CredentialStateCallback credentialStateCallback;
//
//@end

@interface UnitySignInWithApple : UnityAppController<ASAuthorizationControllerDelegate,ASAuthorizationControllerPresentationContextProviding>

@end

@implementation UnitySignInWithApple{
    NSString *accountName;
    NSString *forService;
    NSString *userIdentifier;
}
struct UserInfo
{
    const char * userId;
    const char * email;
    const char * displayName;

    const char * idToken;
    const char * error;

    ASUserDetectionStatus userDetectionStatus;
};

typedef void (*SignInWithAppleCallback)(int result, struct UserInfo s1);
API_AVAILABLE(ios(13.0), tvos(13.0))
typedef void (*CredentialStateCallback)(ASAuthorizationAppleIDProviderCredentialState state);


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



#pragma mark -处理授权
-(void)startRequest
{
    NSLog(@"---开始请求授权---");
    if (@available(iOS 13.0, *)) {
        // 基于用户的Apple ID授权用户，生成用户授权请求的一种机制
        ASAuthorizationAppleIDProvider *appleIDProvider = [[ASAuthorizationAppleIDProvider alloc] init];
        // 创建新的AppleID 授权请求
        ASAuthorizationAppleIDRequest *appleIDRequest = [appleIDProvider createRequest];
        // 在用户授权期间请求的联系信息
        appleIDRequest.requestedScopes = @[ASAuthorizationScopeFullName, ASAuthorizationScopeEmail];
        // 由ASAuthorizationAppleIDProvider创建的授权请求 管理授权请求的控制器
        ASAuthorizationController *authorizationController = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[appleIDRequest]];
        // 设置授权控制器通知授权请求的成功与失败的代理
        authorizationController.delegate = self;
        // 设置提供 展示上下文的代理，在这个上下文中 系统可以展示授权界面给用户
        authorizationController.presentationContextProvider = self;
        // 在控制器初始化期间启动授权流
        [authorizationController performRequests];
    }else{
        // 处理不支持系统版本
        NSLog(@"该系统版本不可用Apple登录");
    }
}

#pragma mark - Private functions
//! 观察授权状态 在启动时检查用户凭据
- (void)observeAuthticationState:(NSString *)userIdentifier {
    NSLog(@"检查用户的授权状态");
    if (@available(iOS 13.0, *)) {
        // A mechanism for generating requests to authenticate users based on their Apple ID.
        // 基于用户的Apple ID 生成授权用户请求的机制
        ASAuthorizationAppleIDProvider *appleIDProvider = [ASAuthorizationAppleIDProvider new];
        if (userIdentifier) {
            NSString* __block errorMsg = nil;
            NSString* __block isAuthorized = @"0";
            //Returns the credential state for the given user in a completion handler.
            // 在回调中返回用户的授权状态
            [appleIDProvider getCredentialStateForUserID:userIdentifier completion:^(ASAuthorizationAppleIDProviderCredentialState credentialState, NSError * _Nullable error) {
                switch (credentialState) {
                        // 苹果证书的授权状态
                    case ASAuthorizationAppleIDProviderCredentialRevoked:
                        // 苹果授权凭证失效
                        errorMsg = @"苹果授权凭证失效";
                        isAuthorized = @"0";
                        break;
                    case ASAuthorizationAppleIDProviderCredentialAuthorized:
                        // 苹果授权凭证状态良好
                        errorMsg = @"苹果授权凭证状态良好";
                        isAuthorized = @"1";
                        break;
                    case ASAuthorizationAppleIDProviderCredentialNotFound:
                        // 未发现苹果授权凭证
                        errorMsg = @"未发现苹果授权凭证";
                        isAuthorized = @"0";
                        // 可以引导用户重新登录
                        break;
                    case ASAuthorizationAppleIDProviderCredentialTransferred:
                        // 后面自己新加的 也让他重新登录处理
                        errorMsg = @"未发现苹果授权凭证转移";
                        isAuthorized = @"0";
                        // 可以引导用户重新登录
                        break;
                }
                NSLog(@"授权状态：%@", errorMsg);
                if ([isAuthorized isEqualToString:@"0"]){
                    [self startRequest];
                }else{
                    //开始登录自己服务器了
                    [self toGameLogin];
                }

            }];
            
        }
    }
}

//! 添加苹果登录的状态通知
- (void)observeAppleSignInState
{
    if (@available(iOS 13.0, *)) {
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(handleSignInWithAppleStateChanged:)
                                                     name:ASAuthorizationAppleIDProviderCredentialRevokedNotification
                                                   object:nil];
    }
}
//! 观察SignInWithApple状态改变
- (void)handleSignInWithAppleStateChanged:(NSNotification *)notification
{
    // Sign the user out, optionally guide them to sign in again
    NSLog(@"%@", notification.userInfo);
}

#pragma mark - iCloud KeyChain password
// 如果存在iCloud Keychain 凭证或者AppleID 凭证提示用户
- (void)perfomExistingAccountSetupFlows{
    NSLog(@"///索取现有凭证////");
    
    if (@available(iOS 13.0, *)) {
        // 基于用户的Apple ID授权用户，生成用户授权请求的一种机制
        ASAuthorizationAppleIDProvider *appleIDProvider = [[ASAuthorizationAppleIDProvider alloc] init];
        // 授权请求AppleID
        ASAuthorizationAppleIDRequest *appleIDRequest = [appleIDProvider createRequest];
        // 为了执行钥匙串凭证分享生成请求的一种机制
        ASAuthorizationPasswordProvider *passwordProvider = [[ASAuthorizationPasswordProvider alloc] init];
        ASAuthorizationPasswordRequest *passwordRequest = [passwordProvider createRequest];
        // 由ASAuthorizationAppleIDProvider创建的授权请求 管理授权请求的控制器
        ASAuthorizationController *authorizationController = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[appleIDRequest, passwordRequest]];
        // 设置授权控制器通知授权请求的成功与失败的代理
        authorizationController.delegate = self;
        // 设置提供 展示上下文的代理，在这个上下文中 系统可以展示授权界面给用户
        authorizationController.presentationContextProvider = self;
        // 在控制器初始化期间启动授权流
        [authorizationController performRequests];
    }else{
        // 处理不支持系统版本
        NSLog(@"该系统版本不可用Apple登录");
    }
}



#pragma mark - delegate
//@optional 授权成功地回调
- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization API_AVAILABLE(ios(13.0)){
//    NSLog(@"授权完成:::%@", authorization.credential);
//    NSLog(@"%s", __FUNCTION__);
//    NSLog(@"%@", controller);
//    NSLog(@"%@", authorization);
    
    if ([authorization.credential isKindOfClass:[ASAuthorizationAppleIDCredential class]]) {
         NSLog(@"----用户登录使用ASAuthorizationAppleIDCredential---------->");
        // 用户登录使用ASAuthorizationAppleIDCredential
        ASAuthorizationAppleIDCredential *appleIDCredential = authorization.credential;
        userIdentifier = appleIDCredential.user;
        // 使用过授权的，可能获取不到以下三个参数
        NSString *familyName = appleIDCredential.fullName.familyName;
        NSString *givenName = appleIDCredential.fullName.givenName;
        NSString *email = appleIDCredential.email;
        
        NSData *identityToken = appleIDCredential.identityToken;
        NSData *authorizationCode = appleIDCredential.authorizationCode;
        
        // 服务器验证需要使用的参数
        NSString *identityTokenStr = [[NSString alloc] initWithData:identityToken encoding:NSUTF8StringEncoding];
        NSString *authorizationCodeStr = [[NSString alloc] initWithData:authorizationCode encoding:NSUTF8StringEncoding];

        // Create an account in your system.

        // For the purpose of this demo app, store the userIdentifier in the keychain.
        //  需要使用钥匙串的方式保存用户的唯一信息 com.elex.girlsthrone.tw
        //[YostarKeychain save:KEYCHAIN_IDENTIFIER(@"userIdentifier") data:user];
        [self saveUserInKeychain:userIdentifier];
        [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userIdentifier,@"user",identityTokenStr,@"token",nil]];
//        [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userIdentifier,@"user",givenName,@"givenName",familyName,@"familyName",email,@"email",identityTokenStr,@"identityTokenStr",authorizationCodeStr,@"authorizationCodeStr",nil]];
        
    }else if ([authorization.credential isKindOfClass:[ASPasswordCredential class]]){
            NSLog(@"----这个获取的是iCloud记录的账号密码---------->");
        // 这个获取的是iCloud记录的账号密码，需要输入框支持iOS 12 记录账号密码的新特性，如果不支持，可以忽略
        // Sign in using an existing iCloud Keychain credential.
        // 用户登录使用现有的密码凭证
        ASPasswordCredential *passwordCredential = authorization.credential;
        // 密码凭证对象的用户标识 用户的唯一标识
        NSString *user = passwordCredential.user;
        // 密码凭证对象的密码
        NSString *password = passwordCredential.password;
        [self toGameLogin];
        [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"2", @"state",user,@"user",password,@"password",nil]];
        
    }else{
        NSLog(@"授权信息均不符");
        //重新授权请求
        [self startRequest];
        [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",@"授权信息均不符",@"errormsg",nil]];
    }
}

// 授权失败的回调
- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error API_AVAILABLE(ios(13.0)){
    // Handle error.
//    NSLog(@"授权失败的回调Handle error：%@", error);
    NSString *errorMsg = nil;
    switch (error.code) {
        case ASAuthorizationErrorCanceled:
            errorMsg = @"用户取消了授权请求";
            break;
        case ASAuthorizationErrorFailed:
            errorMsg = @"授权请求失败";
            break;
        case ASAuthorizationErrorInvalidResponse:
            errorMsg = @"授权请求响应无效";
            break;
        case ASAuthorizationErrorNotHandled:
            errorMsg = @"未能处理授权请求";
            break;
        case ASAuthorizationErrorUnknown:
            errorMsg = @"授权请求失败未知原因";
            break;
        default:
                errorMsg = @"授权请求失败未定义错误码";
            break;
    }
     NSLog(@"授权失败的回调Handle errorMsg：%@", errorMsg);
    //不用处理
//    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",errorMsg,@"errormsg",nil]];
}

// 告诉代理应该在哪个window 展示内容给用户
- (ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller API_AVAILABLE(ios(13.0)){
    NSLog(@"88888888888");
    // 返回window
    return [UIApplication sharedApplication].windows.lastObject;
}

-(void)getPasswordInkeychain{
    NSString *accountName = @"TWuserIdentifier";
    NSString *forService = @"com.elex.girlsthrone.tw";
    NSString *password = nil;
    //读取
    if (![SSKeychain passwordForService:forService account:accountName]) {
        NSLog(@ "没有 getPasswordInkeychain--");
    }
    else{
        password = [SSKeychain passwordForService:forService account:accountName];
        NSLog(@ "--getPasswordInkeychain--->%@",[SSKeychain passwordForService:forService account:accountName]);
    }
    
}

-(void)saveUserInKeychain:(NSString *)userIdentifier{

    
    NSLog(@ "--saveUserInKeychain--->%@",userIdentifier);
    NSString *accountName = @"TWuserIdentifier";
    NSString *forService = @"com.elex.girlsthrone.tw";
    //写入
    [SSKeychain setPassword:userIdentifier forService:forService account:accountName];
//    [SSKeychain setAccessibilityType:kSecAttrAccessibleAlwaysThisDeviceOnly];
    
//    //读取
//    if (![SSKeychain passwordForService:@ "blog " account:@ "hu "]) {
//        NSLog(@ "没有 ");
//    }
//    else{
//        NSLog(@ "%@ ",[SSKeychain passwordForService:@ "blog " account:@ "hu "]);
//    }
//
//    //写入
//    [SSKeychain setPassword:@ "damon " forService:@ "blog " account:@ "hu "];
//    [SSKeychain setAccessibilityType:kSecAttrAccessibleAlwaysThisDeviceOnly];
}

//about GameCenter
-(void)authenticateLocalPlayer {
    
    NSLog(@"尝试获取授权");
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
    localPlayer.authenticateHandler = ^(UIViewController * _Nullable viewController, NSError * _Nullable error) {
        if ([localPlayer isAuthenticated]) {
            //To Do something...
            //已经开启GameCenter并且有账号
            NSLog(@"已经授权1，playerID : %@", localPlayer.playerID);
        }else if(viewController){
            //To Do something...
            NSLog(@"已经授权2，playerID : %@", localPlayer.playerID);
        }
        else{
            if (!error) {
                NSLog(@"授权完成");
            }else{
                //To Do something...
                //没有开启GameCenter
                NSLog(@"取消授权");
                NSLog(@"AuthPlayer error: %@", [error localizedDescription]);
            }
        }
    };
}

#pragma mark -- 登录 授权 ASAuthorizationControllerDelegate
-(void)Login{
//    NSString *Msg = @"---startRequest---";
//    UnitySendMessage("SDKManager", "OnResult", [Msg UTF8String]);
//    [self startRequest];
     [self signin];
}

//sign in with Apple 入口
-(void)signin{
    //从Keychain中读取userIdentifier
    if (![SSKeychain passwordForService:forService account:accountName]) {
       NSLog(@ "--没有userIdentifier 说明之前没有登录过--需要请求授权--");
        //没有userIdentifier 说明之前没有登录过
        [self startRequest];
    }
    else{
        //有userIdentifier 说明之前有登录过 先检查授权状态
        NSLog(@ "--有userIdentifier 说明之前有登录过--需检查授权状态--");
        userIdentifier = [SSKeychain passwordForService:forService account:accountName];
        [self observeAuthticationState: userIdentifier];
    }
}

//开始注册登录自己的游戏服务器
-(void)toGameLogin{
     NSLog(@ "--开始注册登录自己的游戏服务器--");
    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"2", @"state",userIdentifier,@"user",nil]];
//	[self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"2", @"state",userIdentifier,@"user", @"",@"identityTokenStr",nil]];
}

-(void)InitSDK{
	accountName = @"TWuserIdentifier";
    forService = @"com.elex.girlsthrone.tw";
    userIdentifier = @"nil";
	
    NSLog(@"-ios--InitSDK----");
  //  [self signin];
     [self authenticateLocalPlayer];
    //登录之前先observeAuthticationState
//    [self observeAuthticationState];
//    [self getPasswordInkeychain];
//    [self perfomExistingAccountSetupFlows];
    //[self SendMessageToUnity: eInit DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",nil]];
    

}
#pragma mark -- 注销登录
-(void)Switch{

}
#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
    NSLog(@"-ios--Pay----");
//   PayInApple * PayInAppleIn = [[PayInApple alloc] init];
//    [PayInAppleIn Init];
//     [PayInApple test];

}

#pragma mark -- 上报数据
-(void)UploadInfo:(const char*) jsonData{

}

#pragma mark -- 展示客服界面
-(void)GetConfigInfo{

}
-(void)OpenService{

}
//-(void)GetIOSversion{
//
//}
@end

IMPL_APP_CONTROLLER_SUBCLASS (UnitySignInWithApple)

extern "C"
{
    void cInit(){
        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate InitSDK];
    }
    void cLogin(){
        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate Login];
    }
    void cSwitch(){
        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate Switch];
    }
    void cPay(const char* jsonString){
        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate Pay:jsonString];
    }
    void cUpLoadInfo(const char* jsonString){
        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate UploadInfo:jsonString];
    }
    void cOpenService(){
        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate OpenService];
    }
    void cGetConfigInfo(){
        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate GetConfigInfo];
    }
//    void cGetIOSversion(){
//        [(UnitySignInWithApple*)[UIApplication sharedApplication].delegate GetIOSversion];
//    }
}


