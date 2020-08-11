//
//  FBHelper.h
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import <FBSDKLoginKit/FBSDKLoginKit.h>
#import <FBSDKShareKit/FBSDKShareKit.h>
#import "cDelegate.h"
@interface FBHelper : UIViewController <FBSDKSharingDelegate>
//@property (nonatomic, weak) id<cDelegate> CbDelegate;
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)Login;
-(void)Logout;
-(void)share:(const char*) jsonData;
-(void)FBShareUrl;
-(void)FBShareImage;
-(void)Event:(const char*) jsonData;
-(void)CustomEvent:(NSString *)eventName;
-(void)AchieveLevelEvent:(NSString *)level;
-(void)CompleteTutorialEvent:(NSString *)contentData contentId:(NSString *)contentId success:(BOOL)success;
-(void)purchaseEvent:(NSString *)appleOrderID AndProductID:(NSString *)productID;
@end
