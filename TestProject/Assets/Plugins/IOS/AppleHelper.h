//
//  AppleHelper.h
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "SSKeychain.h"

#import <AuthenticationServices/AuthenticationServices.h>
#import <GameKit/GameKit.h>
#import <StoreKit/StoreKit.h>
#import "cDelegate.h"
@interface AppleHelper:NSObject<ASAuthorizationControllerDelegate,ASAuthorizationControllerPresentationContextProviding,SKProductsRequestDelegate,SKPaymentTransactionObserver,GKGameCenterControllerDelegate>
@property (nonatomic, weak) id<cDelegate> CbDelegate;
-(void)InitSDK;
-(void)Login;
-(void)authGamecnter;
-(void)Pay: (const char *) jsonString;
+(id)sharedInstance;
@end

