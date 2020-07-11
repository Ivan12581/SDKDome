//
//  GVC.h
//  Unity-iPhone
//
//  Created by mini on 7/10/20.
//

#import <UIKit/UIKit.h>
#import <GoogleSignIn/GoogleSignIn.h>
NS_ASSUME_NONNULL_BEGIN

@interface GVC : UIViewController<GIDSignInDelegate>
-(void)InitSDK;
-(void)Login;
+(id)sharedInstance;
@end

NS_ASSUME_NONNULL_END
