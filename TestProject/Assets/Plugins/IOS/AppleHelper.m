//
//  AppleHelper.m
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//
//******************************************************
//****************Apple相关
//******************************************************
#import "AppleHelper.h"
//#import "IOSBridgeHelper.h"
@implementation AppleHelper{
    NSString *accountName;
    NSString *forService;
    NSString *userIdentifier;
    id IOSBridgeHelper;
}
static AppleHelper *AppleHelperInstance = nil;
+(AppleHelper*)sharedInstance{
    if (AppleHelperInstance == nil) {
        if (self == [AppleHelper class]) {
            AppleHelperInstance = [[self alloc] init];
        }
    }
    return AppleHelperInstance;
}
-(void)setDelegate:(id<cDelegate>)delegate{
    self.CbDelegate = delegate;
    IOSBridgeHelper = self.CbDelegate;
}
-(void)InitSDK{
    NSString * bundleID = [NSBundle mainBundle].bundleIdentifier;
    accountName = @"TWuserIdentifier";
    forService = @"com.elex.girlsthrone.tw";
    userIdentifier = @"nil";

    NSLog(@"-ios---AppleHelper---InitSDK---bundleID-%@",bundleID);
    //TODO：apple初始化的时候需要添加购买结果的监听 有可能之前支付ok 但是因为通信而导致存在未处理订单 但是此时还没链接服务器 所有购买监听应该在链接逻辑服成功后开启
//    [self addListener];
    IOSBridgeHelper = self.CbDelegate;
    if (@available(iOS 13.0, *)) {
        [IOSBridgeHelper InitSDKCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",nil]];
    }else{
        //此处返回不能为0
        [IOSBridgeHelper InitSDKCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"2", @"state",nil]];
    }

}
//******************************************************
//****************Apple Sign In With Apple
//******************************************************
#pragma mark -处理授权
-(void)startRequest{
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
        [IOSBridgeHelper AppleLoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"-1", @"state",nil]];
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
    NSLog(@"--添加苹果登录的状态通知--");
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
    NSLog(@"--状态改变通知--%@", notification.userInfo);
}



#pragma mark - 授权成功回调
- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization API_AVAILABLE(ios(13.0)){
    
    if ([authorization.credential isKindOfClass:[ASAuthorizationAppleIDCredential class]]) {
         NSLog(@"----用户登录使用ASAuthorizationAppleIDCredential---------->");
        // 用户登录使用ASAuthorizationAppleIDCredential
        ASAuthorizationAppleIDCredential *appleIDCredential = authorization.credential;
        userIdentifier = appleIDCredential.user;
        
        NSData *identityToken = appleIDCredential.identityToken;
//      NSData *authorizationCode = appleIDCredential.authorizationCode;

        // 服务器验证需要使用的参数
        NSString *identityTokenStr = [[NSString alloc] initWithData:identityToken encoding:NSUTF8StringEncoding];
//        NSString *authorizationCodeStr = [[NSString alloc] initWithData:authorizationCode encoding:NSUTF8StringEncoding];

        [self saveUserInKeychain:userIdentifier];
        
        [IOSBridgeHelper AppleLoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userIdentifier,@"uid",identityTokenStr,@"token",nil]];
        
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
        
        [IOSBridgeHelper AppleLoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",user,@"uid",password,@"password",nil]];
        
    }else{
        NSLog(@"授权信息均不符");
        //重新授权请求
        [self startRequest];
    }
}

#pragma mark --  授权失败回调
- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error API_AVAILABLE(ios(13.0)){
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
    [IOSBridgeHelper AppleLoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",nil]];
}

#pragma mark - 获取userIdentifier
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
#pragma mark - 保存userIdentifier
-(void)saveUserInKeychain:(NSString *)userIdentifier{
    NSLog(@ "--saveUserInKeychain--->%@",userIdentifier);
    NSString *accountName = @"TWuserIdentifier";
    NSString *forService = @"com.elex.girlsthrone.tw";
    //写入
    [SSKeychain setPassword:userIdentifier forService:forService account:accountName];
}



#pragma mark -- 登录 授权 ASAuthorizationControllerDelegate
-(void)Login{
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

#pragma mark -- 开始注册登录自己的游戏服务器
-(void)toGameLogin{
     NSLog(@ "--开始注册登录自己的游戏服务器--");
    [IOSBridgeHelper AppleLoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userIdentifier,@"uid",nil]];
}


//******************************************************
//****************Apple GameCenter
//******************************************************
//是否支持GameCenter
- (BOOL) isGameCenterAvailable
{
    Class gcClass = (NSClassFromString(@"GKLocalPlayer"));
    NSString *reqSysVer = @"4.1";
    NSString *currSysVer = [[UIDevice currentDevice] systemVersion];
    BOOL osVersionSupported = ([currSysVer compare:reqSysVer options:NSNumericSearch] != NSOrderedAscending);
    
    return (gcClass && osVersionSupported);
}
#pragma mark - GameCenter 授权状态查询
-(void)authenticateLocalPlayer{
    NSLog(@"尝试获取GameCenter授权");
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
    localPlayer.authenticateHandler = ^(UIViewController * _Nullable viewController, NSError * _Nullable error) {
        if ([[GKLocalPlayer localPlayer] isAuthenticated]) {
            //To Do something...
            //已经开启GameCenter并且有账号

            NSLog(@"已经授权1，playerID : %@", [GKLocalPlayer localPlayer].playerID);
//            self.succBlock([GKLocalPlayer localPlayer].playerID);
        }else if(viewController){
            //To Do something...
            NSLog(@"已经授权2，playerID : %@", [GKLocalPlayer localPlayer].playerID);
//            [self presentViewController:viewController animated:YES completion:nil];
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
-(void)authGamecnter{
       NSLog(@"尝试获取GameCenter授权");
    [[GKLocalPlayer localPlayer] authenticateWithCompletionHandler:^(NSError * _Nullable error){
        if (error == nil) {
            //yes
            NSLog(@"1--alias--%@",[GKLocalPlayer localPlayer].alias);
            NSLog(@"2--authenticated--%d",[GKLocalPlayer localPlayer].authenticated);
            NSLog(@"3--isFriend--%d",[GKLocalPlayer localPlayer].isFriend);
            NSLog(@"4--playerID--%@",[GKLocalPlayer localPlayer].playerID);
            NSLog(@"4--displayName--%@",[GKLocalPlayer localPlayer].displayName);
//            if (@available(iOS 12.4, *)) {
//                NSLog(@"4--playerID--%@",[GKLocalPlayer localPlayer].teamPlayerID);
//            } else {
//                // Fallback on earlier versions
//            }
            //fetchItemsForIdentityVerificationSignature
            [[GKLocalPlayer localPlayer] generateIdentityVerificationSignatureWithCompletionHandler:^(NSURL * publicKeyUrl, NSData * signature, NSData * salt, uint64_t timestamp, NSError * error) {
                if (error) {
                    NSLog(@"--ERROR: %@",error);
                    [IOSBridgeHelper LoginGameCenterCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",nil]];
                }else{
                    NSString *_publicKeyUrl =[publicKeyUrl absoluteString];
                    NSString *_signature =[signature base64EncodedStringWithOptions:0];
                    NSString *_salt =[salt base64EncodedStringWithOptions:0];
                    NSString *_timestamp =[NSString stringWithFormat:@"%llu",timestamp];
                    
                    NSLog(@"1--_publicKeyUrl--%@",_publicKeyUrl);
                    NSLog(@"2--_signature--%@",_signature);
                    NSLog(@"3--_salt--%@",_salt);
                    NSLog(@"4--timestamp--%@",_timestamp);
                    NSLog(@"5--app_bundle_id--%@",[[NSBundle mainBundle] bundleIdentifier]);

                    [IOSBridgeHelper LoginGameCenterCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",_publicKeyUrl,@"publicKeyUrl",_signature,@"signature",_salt,@"salt",_timestamp,@"timestamp",[GKLocalPlayer localPlayer].playerID,@"uid",nil]];

                }
            }];
            
            
        }else{
            NSLog(@"失败  %@",error);
            [IOSBridgeHelper LoginGameCenterCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",nil]];
        }
    }];
    
}
//用戶變更檢測
- (void)registerFoeAuthenticationNotification{
    NSNotificationCenter *nc = [NSNotificationCenter defaultCenter];
    [nc addObserver:self selector:@selector(authenticationChanged) name:GKPlayerAuthenticationDidChangeNotificationName object:nil];
}

- (void)authenticationChanged{
    if([GKLocalPlayer localPlayer].isAuthenticated){
        
    }else{
        
    }
}


- (void)gameCenterViewControllerDidFinish:(nonnull GKGameCenterViewController *)gameCenterViewController { 
    NSLog(@"----gameCenterViewControllerDidFinish----这个是干嘛的------>");
}

@end



