//
//  GoogleHelper.h
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//
#import <GoogleSignIn/GoogleSignIn.h>
#import <Foundation/Foundation.h>
#import "cDelegate.h"

@interface GoogleHelper:UIViewController<UIApplicationDelegate,GIDSignInDelegate>
//@property (nonatomic, weak) id<cDelegate> CbDelegate;
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)Login;
-(void)Logout;
@end
