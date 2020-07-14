//
//  FBHelper.m
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//

#import "FBHelper.h"
#import "IOSBridgeHelper.h"

@implementation FBHelper

static FBHelper *_Instance = nil;
+(FBHelper*)sharedInstance{
    if (_Instance == nil) {
        if (self == [FBHelper class]) {
            _Instance = [[self alloc] init];
        }
    }
    return _Instance;
}
-(void)InitSDK{
//      [FBSDKSettings setAppID:@"949004278872387"];
    [FBSDKSettings setAppID:[[[NSBundle mainBundle] infoDictionary] objectForKey:@"FacebookAppID"]];
    
    NSLog(@"---FBHelper---InitSDK---");
}
- (BOOL)application:(UIApplication *)application
            openURL:(NSURL *)url
            options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
  NSLog(@"-ios---FBHelper---application--222--");
  BOOL handled = [[FBSDKApplicationDelegate sharedInstance] application:application
    openURL:url
    sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey]
    annotation:options[UIApplicationOpenURLOptionsAnnotationKey]
  ];
  // Add any custom logic here.
  return handled;
}
//******************************************************
//****************FaceBook Login
//******************************************************
-(void)Login{
        NSLog(@"---FBHelper---Login---");
    if ([FBSDKAccessToken currentAccessToken]) {
        if ([FBSDKAccessToken isCurrentAccessTokenActive]) {
            FBSDKAccessToken* AccessToken = [FBSDKAccessToken currentAccessToken];
            [self SendAccessTokenToServer:AccessToken];
            [self GetUserInfoWithUserID:AccessToken.userID];
        }else{
            [self FBLogin];
        }
    }else{
        [self FBLogin];
    }
}

//- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary*)options{
//    return [[FBSDKApplicationDelegate sharedInstance] application:app openURL:url sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey] annotation:options[UIApplicationOpenURLOptionsAnnotationKey]];
//}
#pragma mark -- FaceBook Login
-(void)FBLogin{
    [[FBSDKLoginManager new] logInWithPermissions:@[@"public_profile",@"email",@"user_friends"] fromViewController:[[[UIApplication sharedApplication] delegate] window].rootViewController handler:^(FBSDKLoginManagerLoginResult * _Nullable result, NSError * _Nullable error) {
        if (error) {
              NSLog(@"Unexpected login error: %@", error);
        }else{
            if (result.isCancelled) {
                NSLog(@"--login isCancelled---");
            }else{
                NSLog(@"---result.token---> %@", result.token);
                [self SendAccessTokenToServer:result.token];
                [self GetUserInfoWithUserID:result.token.userID];
            }
        }
    }];
}
#pragma mark -- 监听用户登录状态的变化
-(void)addObserverSwitchUserID{
        //您可以使用 NSNotificationCenter 中的 FBSDKAccessTokenDidChangeNotification 来追踪 currentAccessToken 变化。这样可方便您响应用户登录状态的变化
    [FBSDKProfile enableUpdatesOnAccessTokenChange:YES];
    [[NSNotificationCenter defaultCenter] addObserverForName:FBSDKProfileDidChangeNotification
                                                      object:nil
                                                       queue:[NSOperationQueue mainQueue]
                                                  usingBlock:
     ^(NSNotification *notification) {
       if ([FBSDKProfile currentProfile]) {
         // Update for new user profile
             NSLog(@"--- Update for new user profile--");
       }else{
           NSLog(@"--- no new user profile--");
       }
     }];
}
#pragma mark -- 将FaceBook的登录信息返回给自己服务器
-(void)SendAccessTokenToServer:(FBSDKAccessToken *)accessToken{
    NSString* UserID = accessToken.userID;
    NSString* Token = accessToken.tokenString;
    NSLog(@"---UserID--> %@", UserID);
    NSLog(@"---Token--> %@", Token);
    [IOSBridgeHelper LoginFaceBookCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",UserID,@"user",Token,@"token",nil]];
}
#pragma mark -- 通过userID 来获取用户的详细信息
-(void)GetUserInfoWithUserID:(NSString *)userID{
        NSDictionary*params= @{@"fields":@"id,name,email,age_range,first_name,last_name,link,gender,locale,picture,timezone,updated_time,verified"};
        
        FBSDKGraphRequest *request = [[FBSDKGraphRequest alloc]
                                      initWithGraphPath:userID
                                      parameters:params
                                      HTTPMethod:@"GET"];
        [request startWithCompletionHandler:^(FBSDKGraphRequestConnection *connection, id info, NSError *error) {
            NSLog(@"%@",info);
            // 下边的部分 是我用公司提供的开发者测试账号获取到的信息，剩下的信息可能是公司因为没有申请到权限，也有可能是该账号本身就没有公开那部分信息
//            {
//            email = "yfwpsttxgd_1594605349@tfbnw.net";
//            "first_name" = Ava;
//            id = 105403471250609;
//            "last_name" = Carrierosky;
//            name = "Ava Alecdbhfiadgj Carrierosky";
//            picture =     {
//                data =         {
//                    height = 50;
//                    "is_silhouette" = 1;
//                    url = "https://scontent-sjc3-1.xx.fbcdn.net/v/t1.30497-1/cp0/c15.0.50.50a/p50x50/84628273_176159830277856_972693363922829312_n.jpg?_nc_cat=1&_nc_sid=12b3be&_nc_ohc=OO0BRlU0LscAX99Meod&_nc_ht=scontent-sjc3-1.xx&oh=8c4164958b9a988cefc762ef11123aab&oe=5F321E38";
//                    width = 50;
//                };
//            };
            

        }];
}

-(void)Logout{
    [[FBSDKLoginManager new] logOut];
}
//******************************************************
//****************FaceBook Share
//******************************************************
- (void)facebookShareWithMessage:(id)message {
    NSData *data = [message dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *dictionary = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:nil];
    
    NSString *contentUrlString = dictionary[@"content_url"];
//    NSString *imageUrlString = dictionary[@"image_url"];
//    NSString *description = dictionary[@"description"];
//    NSString *title = dictionary[@"title"];
    NSString *quote = dictionary[@"quote"];
    
    FBSDKShareLinkContent *content = [[FBSDKShareLinkContent alloc] init];
    content.contentURL = [NSURL URLWithString:contentUrlString];
//    content.imageURL = [NSURL URLWithString:imageUrlString];
//    content.contentDescription = description;
//    content.contentTitle = title;
    content.quote = quote;
    
    FBSDKShareDialog *dialog = [[FBSDKShareDialog alloc] init];
    dialog.shareContent = content;
    dialog.fromViewController = self;
    dialog.delegate = self;
    dialog.mode = FBSDKShareDialogModeNative;
    [dialog show];
}

#pragma mark - FaceBook Share Delegate
- (void)sharer:(id<FBSDKSharing>)sharer didCompleteWithResults:(NSDictionary *)results {
    NSString *postId = results[@"postId"];
    FBSDKShareDialog *dialog = (FBSDKShareDialog *)sharer;
    if (dialog.mode == FBSDKShareDialogModeBrowser && (postId == nil || [postId isEqualToString:@""])) {
        // 如果使用webview分享的，但postId是空的，
        // 这种情况是用户点击了『完成』按钮，并没有真的分享
        NSLog(@"share Cancel");
    } else {
        NSLog(@"share Success");
    }
    
}

- (void)sharer:(id<FBSDKSharing>)sharer didFailWithError:(NSError *)error {
    FBSDKShareDialog *dialog = (FBSDKShareDialog *)sharer;
    if (error == nil && dialog.mode == FBSDKShareDialogModeNative) {
        // 如果使用原生登录失败，但error为空，那是因为用户没有安装Facebook app
        // 重设dialog的mode，再次弹出对话框
        dialog.mode = FBSDKShareDialogModeBrowser;
        [dialog show];
    } else {
        NSLog(@"share Failure");
    }
}

- (void)sharerDidCancel:(id<FBSDKSharing>)sharer {
    NSLog(@"sahre Cancel");
}
@end
