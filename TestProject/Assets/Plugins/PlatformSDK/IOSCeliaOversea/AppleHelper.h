//
//  AppleHelper.h
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "SSKeychain.h"
#import <AdjustSdk/Adjust.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import <GameKit/GameKit.h>
#import "cDelegate.h"
@interface AppleHelper:NSObject<ASAuthorizationControllerDelegate,ASAuthorizationControllerPresentationContextProviding,GKGameCenterControllerDelegate>
//@property (nonatomic, weak) id<cDelegate> CbDelegate;
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)Login;
-(void)Logout;
-(void)GamecnterLogin;
-(void)GameCenterLogout;
@end

