//
//  GoogleHelper.h
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//
#import <GoogleSignIn/GoogleSignIn.h>
#import <Foundation/Foundation.h>
#import "cDelegate.h"

@interface GoogleHelper:UIViewController<GIDSignInDelegate>
@property (nonatomic, weak) id<cDelegate> CbDelegate;

-(void)InitSDK;
-(void)Login;
-(void)Logout;
+(id)sharedInstance;
@end
