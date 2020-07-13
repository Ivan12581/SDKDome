//
//  GVC.m
//  Unity-iPhone
//
//  Created by mini on 7/10/20.
//
#import "BYJumpEachOther.h"
#import "UnityAppController.h"
#import "GVC.h"

@implementation GVC
static GVC *_Instance = nil;
+(GVC*)sharedInstance{
    if (_Instance == nil) {
        if (self == [GVC class]) {
            _Instance = [[self alloc] init];
//            GetAppController().window.rootViewController = _Instance;
//            NSLog(@"---GVC---sharedInstance---");
        }
    }
    return _Instance;
}
-(void)InitSDK{

    
}
-(void)viewDidLoad{
    NSLog(@"---GVC---viewDidLoad---");
}
//- (void)viewDidLoad {
//    [super viewDidLoad];
//    NSLog(@"---GVC---viewDidLoad---");
//    NSLog(@"设置界面为IOS界面");
//    [GetAppController().window bringSubviewToFront:UnityGetGLViewController().view];
//    GetAppController().window.rootViewController = self;
//        UnityPause(true);
//    [GIDSignIn sharedInstance].delegate = self;
//    [GIDSignIn sharedInstance].shouldFetchBasicProfile = YES;
//
//     //Do any additional setup after loading the view.
//    [GIDSignIn sharedInstance].presentingViewController = self;

//            UIButton *btn = [[UIButton alloc]initWithFrame:CGRectMake(70, 530, 180, 30)];
//            btn.backgroundColor = [UIColor redColor];
//            [btn setTitle:@"Login" forState:UIControlStateNormal];
//            [btn setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
//            [btn addTarget:self action:@selector(Login) forControlEvents:UIControlEventTouchUpInside];
//     [self.view addSubview:btn];
//
//           UIButton *btnBack = [[UIButton alloc]initWithFrame:CGRectMake(90, 630, 150, 35)];
//           btnBack.backgroundColor = [UIColor greenColor];
//           [btnBack setTitle:@"TOUnity" forState:UIControlStateNormal];
//           [btnBack setTitleColor:[UIColor yellowColor] forState:UIControlStateNormal];
//           [btnBack addTarget:self action:@selector(Backto) forControlEvents:UIControlEventTouchUpInside];
//        [self.view addSubview:btnBack];
//
//    GIDSignInButton *button = [[GIDSignInButton alloc] init];
//    button.frame = CGRectMake(100, 100, 300, 100);
//     [self.view addSubview:button];
    //不重复，只调用一次。timer运行一次就会自动停止运行

//    [[GIDSignIn sharedInstance] signIn];
//}
-(void)Backto{
    [[BYJumpEachOther sharedInstance] setupUnity];
}
- (void)Login {
     NSLog(@"---GVC  Login---");
[GIDSignIn sharedInstance].delegate = self;
[GIDSignIn sharedInstance].shouldFetchBasicProfile = YES;

// Do any additional setup after loading the view.
    [GIDSignIn sharedInstance].presentingViewController = [[[UIApplication sharedApplication] delegate] window].rootViewController;
    
    [[GIDSignIn sharedInstance] signIn];
//    NSTimer *myTimer = [NSTimer scheduledTimerWithTimeInterval:5 target:self selector:@selector(tets) userInfo:nil repeats:NO];

    
      // Automatically sign in the user.
//  [[GIDSignIn sharedInstance] restorePreviousSignIn];




}
-(void)tets{

    NSLog(@"---tets---");
    [[GIDSignIn sharedInstance] signIn];
}
-(void)saveHan{
        GIDSignIn *signIn = [GIDSignIn sharedInstance];
        if ([signIn hasPreviousSignIn]) {
          [signIn restorePreviousSignIn];

          // If you ever changed the client ID you use for Google Sign-in, or
          // requested a different set of scopes, then also confirm that they
          // have the values you expect before proceeding.
    //        if (![signIn.currentUser.authentication.clientID  isEqual: @"YOUR_CLIENT_ID"]
    //          || !hasYourRequiredScopes(signIn.currentUser.grantedScopes)) {
    //        [signIn signOut];
    //      }
        }
    

    [signIn.currentUser.authentication
     getTokensWithHandler:^(GIDAuthentication *auth, NSError *error) {
      if (error != nil) { return; }
      NSString *token = auth.accessToken;  // You can also get idToken and refreshToken.
    }];
    [signIn.currentUser.authentication
     refreshTokensWithHandler:^(GIDAuthentication *auth, NSError *error) {
      if (error != nil) { return; }
      NSString *token = auth.accessToken;  // You can also get idToken and refreshToken.
    }];
}
-(void)dealloc {
     NSLog(@"---GVC  dealloc---");
//    [[GIDSignIn sharedInstance].presentingViewController.view removeFromSuperview]; // It works!
}
/*
#pragma mark - Navigation

// In a storyboard-based application, you will often want to do a little preparation before navigation
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    // Get the new view controller using [segue destinationViewController].
    // Pass the selected object to the new view controller.
}
*/

- (void)signIn:(GIDSignIn *)signIn didSignInForUser:(GIDGoogleUser *)user withError:(NSError *)error {
//    [[BYJumpEachOther sharedInstance] setupUnity];
    NSLog(@"---GVC  didSignInForUser---");
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
        NSLog(@"---GVC  didDisconnectWithUser---");
  // Perform any operations when the user disconnects from app here.
  // ...
}

- (BOOL)application:(UIApplication *)app
            openURL:(NSURL *)url
            options:(NSDictionary<NSString *, id> *)options {
  return [[GIDSignIn sharedInstance] handleURL:url];
}
- (void)encodeWithCoder:(nonnull NSCoder *)coder {
    NSLog(@"---GVC  encodeWithCoder---");
}

- (void)traitCollectionDidChange:(nullable UITraitCollection *)previousTraitCollection {
    NSLog(@"---GVC  traitCollectionDidChange---");
}

- (void)preferredContentSizeDidChangeForChildContentContainer:(nonnull id<UIContentContainer>)container {
    NSLog(@"---GVC  preferredContentSizeDidChangeForChildContentContainer---");
}

- (CGSize)sizeForChildContentContainer:(nonnull id<UIContentContainer>)container withParentContainerSize:(CGSize)parentSize {
      NSLog(@"---GVC  sizeForChildContentContainer---");
    return parentSize;
}

- (void)systemLayoutFittingSizeDidChangeForChildContentContainer:(nonnull id<UIContentContainer>)container {
      NSLog(@"---GVC  systemLayoutFittingSizeDidChangeForChildContentContainer---");

}

- (void)viewWillTransitionToSize:(CGSize)size withTransitionCoordinator:(nonnull id<UIViewControllerTransitionCoordinator>)coordinator {
      NSLog(@"---GVC  viewWillTransitionToSize---");
}

- (void)willTransitionToTraitCollection:(nonnull UITraitCollection *)newCollection withTransitionCoordinator:(nonnull id<UIViewControllerTransitionCoordinator>)coordinator {
      NSLog(@"---GVC  willTransitionToTraitCollection---");
}

- (void)didUpdateFocusInContext:(nonnull UIFocusUpdateContext *)context withAnimationCoordinator:(nonnull UIFocusAnimationCoordinator *)coordinator {
      NSLog(@"---GVC  didUpdateFocusInContext---");
}

- (void)setNeedsFocusUpdate {
      NSLog(@"---GVC  setNeedsFocusUpdate---");
}

- (BOOL)shouldUpdateFocusInContext:(nonnull UIFocusUpdateContext *)context {
      NSLog(@"---GVC  shouldUpdateFocusInContext---");
    return YES;
}

- (void)updateFocusIfNeeded {
      NSLog(@"---GVC  updateFocusIfNeeded---");
}

@end
