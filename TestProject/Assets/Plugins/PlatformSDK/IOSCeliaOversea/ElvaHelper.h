//
//  ElvaHelper.h
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import <Foundation/Foundation.h>
#import <ElvaChatServiceSDK/ElvaChatServiceSDK.h>
#import "cDelegate.h"
NS_ASSUME_NONNULL_BEGIN

@interface ElvaHelper : NSObject
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)show:(const char*) jsonString;
@end

NS_ASSUME_NONNULL_END
