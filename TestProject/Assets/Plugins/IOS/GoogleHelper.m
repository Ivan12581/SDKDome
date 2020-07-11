//
//  GoogleHelper.m
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//

#import "GoogleHelper.h"
#import "UnityAppController.h"
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
//-(void)viewDidLoad{
//    [super viewDidLoad];
//        [GIDSignIn sharedInstance].clientID = @"554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun.apps.googleusercontent.com";
////    [GIDSignIn sharedInstance].clientID = @"com.googleusercontent.apps.554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun";
//
//    GIDSignIn *signIn = [GIDSignIn sharedInstance];
//    signIn.shouldFetchBasicProfile = YES;
//    signIn.delegate = self;
////    signIn.presentingViewController = self;
//    [signIn setScopes:[NSArray arrayWithObject:@"https://www.googleapis.com/auth/drive.readonly"]];
////    [[GIDSignIn sharedInstance] restorePreviousSignIn];
////     [[GIDSignIn sharedInstance] signIn];
//
//
//    //    UIViewController *vc = [[UIViewController alloc] init];
//    //    vc.view.backgroundColor = [UIColor greenColor];
//    //    vc.view.frame = [UIScreen mainScreen].bounds;
//    //
//        UIButton *btn = [[UIButton alloc]initWithFrame:CGRectMake(70, 530, 180, 30)];
//        btn.backgroundColor = [UIColor redColor];
//        [btn setTitle:@"跳转到Unity界面" forState:UIControlStateNormal];
//        [btn setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
//        [btn addTarget:self action:@selector(Login) forControlEvents:UIControlEventTouchUpInside];
//
//
////    GIDSignInButton *btn = [[GIDSignInButton alloc] init];
////
////    btn.frame = CGRectMake(100, 100, 300, 100);
////    [btn setTitle:@"跳转到Unity界面" forState:UIControlStateNormal];
//    [self.view addSubview:btn];
//
//}
-(void)InitSDK{
    NSLog(@"---GoogleHelper  Init---");
    [GIDSignIn sharedInstance].clientID = @"554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun.apps.googleusercontent.com";
    [GIDSignIn sharedInstance].delegate = self;
     
}


- (void)signIn:(GIDSignIn *)signIn presentViewController:(UIViewController *)viewController{
    NSLog(@"%@", signIn);
   //此处的rootViewControlle为当前显示的试图控制器
//    GetAppController().window.rootViewController
    [GetAppController().window.rootViewController presentViewController:viewController animated:YES completion:nil];
//    [self.rootViewController presentViewController:viewController animated:YES completion:nil];
}
//- (void)presentSignInViewController:(UIViewController *)viewController {
//    NSLog(@"--------presentSignInViewController----------");
//  [[self navigationController] pushViewController:viewController animated:YES];
//}

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
  NSString *fullName = user.profile.name;
  NSString *givenName = user.profile.givenName;
  NSString *familyName = user.profile.familyName;
  NSString *email = user.profile.email;
    
    NSLog(@"---userId--->%@", userId);
    NSLog(@"---idToken--->%@", idToken);
    NSLog(@"---fullName--->%@", fullName);
    NSLog(@"---givenName--->%@", givenName);
    NSLog(@"---familyName--->%@", familyName);
    NSLog(@"---email--->%@", email);
  // ...
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
//  [super viewDidLoad];

//  [GIDSignIn sharedInstance].presentingViewController = self;

  // Automatically sign in the user.


    [[GIDSignIn sharedInstance] signIn];
//  [[GIDSignIn sharedInstance] restorePreviousSignIn];

}
@end
