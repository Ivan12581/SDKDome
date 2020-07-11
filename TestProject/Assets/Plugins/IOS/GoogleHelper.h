//
//  GoogleHelper.h
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//
#import <GoogleSignIn/GoogleSignIn.h>
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@interface GoogleHelper:UIResponder<UIApplicationDelegate,GIDSignInDelegate>
-(void)InitSDK;
-(void)Login;
+(id)sharedInstance;
@end
