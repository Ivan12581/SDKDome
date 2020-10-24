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
#import "cDelegate.h"
@interface IOSBridgeHelper:UnityAppController<UIApplicationDelegate,cDelegate>

@end
