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
#import "IOSBridgeHelper.h"
@implementation AppleHelper{
    NSString *accountName;
    NSString *forService;
    NSString *userIdentifier;
    NSString *goodID;   //商品ID
    NSInteger goodNum; //商品数量
    SKPaymentTransaction *order;
}
static AppleHelper *AppleHelperInstance = nil;
+(AppleHelper*)sharedInstance{
    if (AppleHelperInstance == nil) {
        if (self == [AppleHelper class]) {
            AppleHelperInstance = [[self alloc] init];
        }
    }
    return AppleHelperInstance;
//    static AppleHelper* instance = nil;
//    static dispatch_once_t once;
//
//    dispatch_once(&once, ^{
//        instance = [[self.class alloc] init];
//    });
//    return instance;
}
-(void)InitSDK{
    accountName = @"TWuserIdentifier";
    forService = @"com.elex.girlsthrone.tw";
    userIdentifier = @"nil";
    
    NSLog(@"-ios---AppleHelper---InitSDK----");
    //apple初始化的时候需要添加购买结果的监听 有可能之前支付ok 但是因为通信而导致存在未处理订单
    [self addListener];
    
    [IOSBridgeHelper InitSDKCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",accountName,@"TWuserIdentifier",forService,@"forService",nil]];
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
        // 使用过授权的，可能获取不到以下三个参数
//        NSString *familyName = appleIDCredential.fullName.familyName;
//        NSString *givenName = appleIDCredential.fullName.givenName;
//        NSString *email = appleIDCredential.email;
        
        NSData *identityToken = appleIDCredential.identityToken;
//        NSData *authorizationCode = appleIDCredential.authorizationCode;
        
        // 服务器验证需要使用的参数
        NSString *identityTokenStr = [[NSString alloc] initWithData:identityToken encoding:NSUTF8StringEncoding];
//        NSString *authorizationCodeStr = [[NSString alloc] initWithData:authorizationCode encoding:NSUTF8StringEncoding];

        // Create an account in your system.

        // For the purpose of this demo app, store the userIdentifier in the keychain.
        //  需要使用钥匙串的方式保存用户的唯一信息 com.elex.girlsthrone.tw
        //[YostarKeychain save:KEYCHAIN_IDENTIFIER(@"userIdentifier") data:user];
        [self saveUserInKeychain:userIdentifier];
        
        [IOSBridgeHelper LoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userIdentifier,@"user",identityTokenStr,@"token",nil]];
        
//        [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userIdentifier,@"user",identityTokenStr,@"token",nil]];
        
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
        
        [IOSBridgeHelper LoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"2", @"state",user,@"user",password,@"password",nil]];
        
//        [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"2", @"state",user,@"user",password,@"password",nil]];
        
    }else{
        NSLog(@"授权信息均不符");
        //重新授权请求
        [self startRequest];
        [IOSBridgeHelper LoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",@"授权信息均不符",@"errormsg",nil]];
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
    //不用处理
//    [self SendMessageToUnity: eLogin DictData:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",errorMsg,@"errormsg",nil]];
}

// 告诉代理应该在哪个window 展示内容给用户
- (ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller API_AVAILABLE(ios(13.0)){
    NSLog(@"88888888888");
    // 返回window
    return [UIApplication sharedApplication].windows.lastObject;
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
//    NSMutableDictionary *dict = [NSMutableDictionary dictionaryWithObjectsAndKeys:
//    @"2", @"state",
//    userIdentifier, @"user", nil];
//    [IOSBridgeHelper LoginCallBack:dict];
    [IOSBridgeHelper LoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"2", @"state",userIdentifier,@"user",nil]];
}


//******************************************************
//****************Apple GameCenter
//******************************************************
#pragma mark - GameCenter 授权状态查询
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
//******************************************************
//****************Apple In-App Purchase
//******************************************************
//苹果服务器验证地址:
//测试：https://sandbox.itunes.apple.com/verifyReceipt
//已上线Appstore：https://buy.itunes.apple.com/verifyReceipt

//此处后面要根据参数来确定是购买 还是将服务器校验结果返回（为了处理apple这边支付状态）
#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
       // const char* --> nnstring
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    // nsstring -> nsdata
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    // nsdata -> nsdictionary
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];

    NSString *_productsId = [dict valueForKey:@"GoodID"];
    goodID = [dict valueForKey:@"GoodID"];
    NSLog(@" Pay goodID: %@", goodID);
    [self buyProductsWithId:_productsId];

}
//收到产品返回信息
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response NS_AVAILABLE(10_7, 3_0) {
    NSArray *myProduct = response.products;
    if([myProduct count] == 0){
        NSLog(@"查找不到商品信息");
        return;
    }
   SKProduct *requestProduct = nil;
    for (SKProduct *product in myProduct) {
        NSLog(@"product info");
        NSLog(@"  基本描述: %@", [product description]);
        NSLog(@"  IAP的id: %@", product.productIdentifier);
        NSLog(@"  地区编码: %@", product.priceLocale.localeIdentifier);
        NSLog(@"  本地价格: %@", product.price);
        NSLog(@"  语言代码: %@", [product.priceLocale objectForKey:NSLocaleLanguageCode]);
        NSLog(@"  国家代码: %@", [product.priceLocale objectForKey:NSLocaleCountryCode]);
        NSLog(@"  货币代码: %@", [product.priceLocale objectForKey:NSLocaleCurrencyCode]);
        NSLog(@"  货币符号: %@", [product.priceLocale objectForKey:NSLocaleCurrencySymbol]);
        NSLog(@"  本地标题: %@", product.localizedTitle);
        NSLog(@"  本地描述: %@", product.localizedDescription);
        //如果后台消费条目的ID与我这里需要请求的一样（用于确保订单的正确性）
        if([product.productIdentifier isEqualToString:goodID]){
            requestProduct = product;
//                break;
        }
    }
    //发送购买请求
    NSLog(@"发送购买请求");
    SKMutablePayment *payment = [SKMutablePayment paymentWithProduct:requestProduct];
//    @property(nonatomic, readonly) NSInteger quantity;
//    The default value is 1, the minimum value is 1, and the maximum value is 10.
//    payment.quantity = goodNum;
        //可记录一个字符串，用于帮助苹果检测不规则支付活动 可以是userId，也可以是订单id，跟你自己需要而定
        //payment.applicationUsername = userId;
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}
#pragma mark - SKRequestDelegate
//请求失败
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error {
    // 购买失败
    NSLog(@"error:%@", error);
}
//请求结束
- (void)requestDidFinish:(SKRequest *)request{
    NSLog(@"请求结束");
}
#pragma mark - 购买结果返回 需监听购买结果委托 [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions {
    for (SKPaymentTransaction *tran in transactions) {
         NSLog(@"购买结果");
        switch (tran.transactionState) {
            case SKPaymentTransactionStatePurchased: // 交易完成
                // 发送自己服务器验证凭证
                NSLog(@"交易完成");
                [self completeTransaction:tran];
                //    等服务器验证完后再finish 这样每次启动app这个接口会再调用
               [[SKPaymentQueue defaultQueue]finishTransaction:tran];
                break;
            case SKPaymentTransactionStatePurchasing: // 购买中
                NSLog(@"商品添加进列表");
                break;
            case SKPaymentTransactionStateRestored: // 购买过 消耗型商品不用写
                NSLog(@"已经购买过商品");
                // 恢复逻辑
                //[[SKPaymentQueue defaultQueue] finishTransaction:tran];
                break;
            case SKPaymentTransactionStateFailed: // 交易失败
                 NSLog(@"交易失败");
                [[SKPaymentQueue defaultQueue]finishTransaction:tran];
                break;
            default:
                break;
        }
    }
}
//交易结束,验证支付信息是否都正确。
- (void)completeTransaction:(SKPaymentTransaction *)transaction {
    order = transaction;
    // 验证凭据，获取到苹果返回的交易凭据
    // appStoreReceiptURL iOS7.0增加的，购买交易完成后，会将凭据存放在该地址
    NSURL *receiptURL = [[NSBundle mainBundle] appStoreReceiptURL];
    // 从沙盒中获取到购买凭据
    NSData *receiptData = [NSData dataWithContentsOfURL:receiptURL];
    /**
        BASE64 常用的编码方案，通常用于数据传输，以及加密算法的基础算法，传输过程中能够保证数据传输的稳定性
        BASE64是可以编码和解码的
        关于验证：https:blog.csdn.net/qq_22080737/article/details/79786500?utm_medium=distribute.pc_relevant.none-task-blog-baidujs-2
    */
    NSString *encodeStr = [receiptData base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
    NSLog(@"交易结束,验证支付信息 : %@", encodeStr);
    //发给自己服务器
    [IOSBridgeHelper PayCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",encodeStr,@"encodeStr",nil]];
//    [self sendMineServer:encodeStr];
//    [self sendMineServer:transaction];
}
#pragma mark - In-App Purchase入口
- (void)buyProductsWithId:(NSString *)productsId{
//    goodNum = [NSNumber numberWithInteger:quantity];
    [self addListener];
    //允许程序内付费购买
    if ([SKPaymentQueue canMakePayments]) {
        //请求对应的产品信息
        NSArray *productArr = [[NSArray alloc] initWithObjects:productsId,nil];
        NSSet *nsset = [NSSet setWithArray:productArr];
        SKProductsRequest *request = [[SKProductsRequest alloc] initWithProductIdentifiers:nsset];
        request.delegate = self;
        [request start];
    } else {
        //您的手机没有打开程序内付费购买
        NSLog(@"用户不允许内购");
    }
}
#pragma mark --根据服务器返回结果来处理支付状态
-(void)HandlePayState{
    //验证成功之后需要关闭监听 订单消除
    if (1) {
         NSLog(@"服务器校验交易凭证为有效");
        [self completeTransaction:order];
        [self removeListener];
    }else{
         NSLog(@"服务器校验交易凭证为无效  这个咋处理啊 我也不知道啊");
        //我也不知道咋处理啊
    }
}
//监听购买结果
-(void)addListener{
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
}
//移除监听购买结果
-(void)removeListener{
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
}
@end


