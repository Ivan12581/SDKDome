//
//  IOSBridgeHelper.h
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//

#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import "UnityAppController.h"
#import <UIKit/UIKit.h>
#import "SSKeychain.h"
#import <GameKit/GameKit.h>
@interface IOSBridgeHelper:UnityAppController
//+(id)sharedInstance;
+(void)LoginCallBack:(NSMutableDictionary *)dict;
+(void)InitSDKCallBack;
+(void)UploadInfoCallBack;
//+(void)ExitGameCallBack;
+(void)GetConfigInfoCallBack;
+(void)SwitchCallBack;
+(void)PayCallBack:(NSMutableDictionary *)dict;
+(void)OpenServiceCallBack;

@end
