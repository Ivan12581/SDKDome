//
//  GoogleHelper.m
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//

#import "GoogleHelper.h"

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
    [GIDSignIn sharedInstance].clientID = @"554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun.apps.googleusercontent.com";
    [GIDSignIn sharedInstance].delegate = self;
}

- (BOOL)application:(UIApplication *)application
didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {

  [GIDSignIn sharedInstance].clientID = @"554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun.apps.googleusercontent.com";
  [GIDSignIn sharedInstance].delegate = self;

  return YES;
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
- (IBAction)didTapSignOut:(id)sender {
  [[GIDSignIn sharedInstance] signOut];
}
- (void)Login {
//  [super viewDidLoad];

  [GIDSignIn sharedInstance].presentingViewController = self;

  // Automatically sign in the user.


    [[GIDSignIn sharedInstance] signIn];
//  [[GIDSignIn sharedInstance] restorePreviousSignIn];

}
@end
