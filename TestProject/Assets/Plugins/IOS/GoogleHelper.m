//
//  GoogleHelper.m
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//

#import "GoogleHelper.h"
#import "IOSBridgeHelper.h"
@implementation GoogleHelper

static GoogleHelper *GoogleHelperIns = nil;
+(GoogleHelper*)sharedInstance{
    if (GoogleHelperIns == nil) {
        if (self == [GoogleHelper class]) {
            GoogleHelperIns = [[self alloc] init];
        }
    }
    return GoogleHelperIns;
}

-(void)InitSDK{
    NSLog(@"---GoogleHelper  Init---");
    [GIDSignIn sharedInstance].clientID = @"554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn.apps.googleusercontent.com";
    [GIDSignIn sharedInstance].delegate = self;
    [GIDSignIn sharedInstance].shouldFetchBasicProfile = YES;
    [GIDSignIn sharedInstance].presentingViewController = [[[UIApplication sharedApplication] delegate] window].rootViewController;
}


- (void)signIn:(GIDSignIn *)signIn presentViewController:(UIViewController *)viewController{
       NSLog(@"---presentViewController--->%@", signIn);
   //此处的rootViewControlle为当前显示的试图控制器
//    GetAppController().window.rootViewController
    [GetAppController().window.rootViewController presentViewController:viewController animated:YES completion:nil];
//    [self.rootViewController presentViewController:viewController animated:YES completion:nil];
}


- (void)signIn:(GIDSignIn *)signIn
didSignInForUser:(GIDGoogleUser *)user
     withError:(NSError *)error {
  if (error != nil) {
    if (error.code == kGIDSignInErrorCodeHasNoAuthInKeychain) {
      NSLog(@"The user has not signed in before or they have since signed out.");
    } else {
      NSLog(@"%@", error.localizedDescription);
    }
    return;
  }
  // Perform any operations on signed in user here.
  NSString *userId = user.userID;                  // For client-side use only!
  NSString *idToken = user.authentication.idToken; // Safe to send to the server

    [IOSBridgeHelper LoginGoogleCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",userId,@"UserID",idToken,@"Token",nil]];
}

- (void)signIn:(GIDSignIn *)signIn
didDisconnectWithUser:(GIDGoogleUser *)user
     withError:(NSError *)error {
        NSLog(@"---GoogleHelper  didDisconnectWithUser---");
  // Perform any operations when the user disconnects from app here.
  // ...
}
- (BOOL)application:(UIApplication *)app
            openURL:(NSURL *)url
            options:(NSDictionary<NSString *, id> *)options {
  return [[GIDSignIn sharedInstance] handleURL:url];
}

- (void)Login {
     NSLog(@"---GoogleHelper  Login---");
    GIDSignIn *signIn = [GIDSignIn sharedInstance];
    if ([signIn hasPreviousSignIn]) {
        [signIn restorePreviousSignIn];
        
    }else{
        [[GIDSignIn sharedInstance] signIn];
    }

      // Automatically sign in the user.
    //  [[GIDSignIn sharedInstance] restorePreviousSignIn];

}
@end
