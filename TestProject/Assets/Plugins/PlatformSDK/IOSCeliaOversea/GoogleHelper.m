//
//  GoogleHelper.m
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//

#import "GoogleHelper.h"
#import "UnityAppController.h"
@implementation GoogleHelper{
        id IOSBridgeHelper;
}

static GoogleHelper *GoogleHelperIns = nil;
+(GoogleHelper*)sharedInstance{
    if (GoogleHelperIns == nil) {
        if (self == [GoogleHelper class]) {
            GoogleHelperIns = [GoogleHelper new];
        }
    }
    return GoogleHelperIns;
}
-(void)InitSDK:(id<cDelegate>)delegate{
    IOSBridgeHelper = delegate;
    [GIDSignIn sharedInstance].clientID = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"GoogleClientID"];
    [GIDSignIn sharedInstance].delegate = self;
    [GIDSignIn sharedInstance].shouldFetchBasicProfile = YES;
//    [GIDSignIn sharedInstance].presentingViewController = [[[UIApplication sharedApplication] delegate] window].rootViewController;
    NSLog(@"---GoogleHelper  InitSDK---");
}
- (void)Login {
     NSLog(@"---GoogleHelper  Login---");
    [GIDSignIn sharedInstance].presentingViewController = [[[UIApplication sharedApplication] delegate] window].rootViewController;
    GIDSignIn *signIn = [GIDSignIn sharedInstance];
    if ([signIn hasPreviousSignIn]) {
        [signIn restorePreviousSignIn];
    }else{
        [[GIDSignIn sharedInstance] signIn];
    }
//            [[GIDSignIn sharedInstance] signIn];
}
-(void)Logout{
    [[GIDSignIn sharedInstance] signOut];
}

- (void)signIn:(GIDSignIn *)signIn presentViewController:(UIViewController *)viewController{
       NSLog(@"---presentViewController--->%@", signIn);
   //此处的rootViewControlle为当前显示的试图控制器
//    GetAppController().window.rootViewController
    [GetAppController().window.rootViewController presentViewController:viewController animated:YES completion:nil];
//    [self.rootViewController presentViewController:viewController animated:YES completion:nil];
}


- (void)signIn:(GIDSignIn *)signIn didSignInForUser:(GIDGoogleUser *)user withError:(NSError *)error {
  if (error != nil) {
    if (error.code == kGIDSignInErrorCodeHasNoAuthInKeychain) {
      NSLog(@"The user has not signed in before or they have since signed out.");
                [IOSBridgeHelper LoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"-1", @"state",nil]];
    } else {
      NSLog(@"---didSignInForUser--->%@", error.localizedDescription);
        [IOSBridgeHelper LoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",nil]];
    }
    return;
  }
  // Perform any operations on signed in user here.
  NSString *userId = user.userID;                  // For client-side use only!
  NSString *idToken = user.authentication.idToken; // Safe to send to the server
    [IOSBridgeHelper LoginCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userId,@"uid",idToken,@"token",nil]];
}

- (void)signIn:(GIDSignIn *)signIn didDisconnectWithUser:(GIDGoogleUser *)user withError:(NSError *)error {
        NSLog(@"---GoogleHelper  didDisconnectWithUser---");
  // Perform any operations when the user disconnects from app here.
  // ...
}
////iOS 8 以上
//- (BOOL)application:(UIApplication *)app
//            openURL:(NSURL *)url
//            options:(NSDictionary<NSString *, id> *)options {
//  return [[GIDSignIn sharedInstance] handleURL:url];
//}
////iOS 8 以下
//- (BOOL)application:(UIApplication *)application
//            openURL:(NSURL *)url
//  sourceApplication:(NSString *)sourceApplication
//         annotation:(id)annotation {
//  return [[GIDSignIn sharedInstance] handleURL:url];
//}

@end
