//
//  FBHelper.m
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//

#import "FBHelper.h"
//#import "IOSBridgeHelper.h"

@implementation FBHelper{
    UIViewController* RVC;
}

static FBHelper *_Instance = nil;
+(FBHelper*)sharedInstance{
    if (_Instance == nil) {
        if (self == [FBHelper class]) {
            _Instance = [[self alloc] init];
        }
    }
    return _Instance;
}
-(void)setDelegate:(id<cDelegate>)delegate{
    self.CbDelegate = delegate;
}
-(void)InitSDK{
//      [FBSDKSettings setAppID:@"949004278872387"];
    [FBSDKSettings setAppID:[[[NSBundle mainBundle] infoDictionary] objectForKey:@"FacebookAppID"]];
    
    NSLog(@"---FBHelper---InitSDK---");
    RVC = [[[UIApplication sharedApplication] delegate] window].rootViewController;
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
//            [self GetUserInfoWithUserID:AccessToken.userID];
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
                [self.CbDelegate LoginFaceBookCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"-1", @"state",nil]];
        }else{
            if (result.isCancelled) {
                NSLog(@"--login isCancelled---");
                [self.CbDelegate LoginFaceBookCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",nil]];
            }else{
                NSLog(@"---result.token---> %@", result.token);
                [self SendAccessTokenToServer:result.token];
//                [self GetUserInfoWithUserID:result.token.userID];
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
    [self.CbDelegate LoginFaceBookCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",UserID,@"uid",Token,@"token",nil]];
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
-(void)FBShareUrl{
    //构建内容
    FBSDKShareLinkContent *linkContent = [[FBSDKShareLinkContent alloc] init];
    linkContent.contentURL = [NSURL URLWithString:@"https://image.baidu.com"];
    linkContent.quote = @"1234567899874653214";
//    linkContent.contentTitle = @"百度";
//    linkContent.contentDescription = [[NSString alloc] initWithFormat:@"%@",@"星空图片欣赏"];
//    linkContent.imageURL = [NSURL URLWithString:@"https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1561310690603&di=6fb462fc7c72ab479061c8045639f87b&imgtype=0&src=http%3A%2F%2Fe.hiphotos.baidu.com%2Fimage%2Fpic%2Fitem%2F4034970a304e251fb1a2546da986c9177e3e53c9.jpg"];
//    //分享对话框
    [FBSDKShareDialog showFromViewController:RVC withContent:linkContent delegate:self];
}
-(void)FBShareImage{
    //分享内容
    UIImage *image = [UIImage imageWithData:[NSData dataWithContentsOfURL:[NSURL URLWithString:@"https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1561310690603&di=6fb462fc7c72ab479061c8045639f87b&imgtype=0&src=http%3A%2F%2Fe.hiphotos.baidu.com%2Fimage%2Fpic%2Fitem%2F4034970a304e251fb1a2546da986c9177e3e53c9.jpg"]]];
    FBSDKSharePhoto *photo = [[FBSDKSharePhoto alloc] init];
    photo.image = image;
    photo.userGenerated = YES;
    FBSDKSharePhotoContent *content = [[FBSDKSharePhotoContent alloc] init];
    content.photos = @[photo];
    //分享对话框
    [FBSDKShareDialog showFromViewController:RVC withContent:content delegate:self];
}
//分享成功回调
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
//分享失败回调
- (void)sharer:(id<FBSDKSharing>)sharer didFailWithError:(NSError *)error {
    FBSDKShareDialog *dialog = (FBSDKShareDialog *)sharer;
    if (error == nil && dialog.mode == FBSDKShareDialogModeNative) {
         NSLog(@"---用户没有安装Facebook app---");
        // 如果使用原生登录失败，但error为空，那是因为用户没有安装Facebook app
        // 重设dialog的mode，再次弹出对话框
        dialog.mode = FBSDKShareDialogModeBrowser;
        [dialog show];
    } else {
        NSLog(@"share Failure");
    }
}
//分享取消回调
- (void)sharerDidCancel:(id<FBSDKSharing>)sharer {
    NSLog(@"sahre Cancel");
}
@end
