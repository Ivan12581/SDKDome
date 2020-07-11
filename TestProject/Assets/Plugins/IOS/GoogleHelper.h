//
//  GoogleHelper.h
//  Unity-iPhone
//
//  Created by mini on 7/9/20.
//
#import <GoogleSignIn/GoogleSignIn.h>
#import <Foundation/Foundation.h>

@interface GoogleHelper:NSObject
-(void)InitSDK;
-(void)Login;
+(id)sharedInstance;
@end
