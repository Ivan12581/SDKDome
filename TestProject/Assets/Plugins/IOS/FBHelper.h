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
@interface FBHelper : UIViewController <UIApplicationDelegate,FBSDKSharingDelegate>
@property (nonatomic, weak) id<cDelegate> CbDelegate;
-(void)InitSDK;
-(void)Login;
-(void)Logout;
-(void)FBShareUrl;
-(void)FBShareImage;
+(id)sharedInstance;
- (void)sharer:(id<FBSDKSharing>)sharer didCompleteWithResults:(NSDictionary *)results;
- (void)sharer:(id<FBSDKSharing>)sharer didFailWithError:(NSError *)error;
- (void)sharerDidCancel:(id<FBSDKSharing>)sharer;
@end
