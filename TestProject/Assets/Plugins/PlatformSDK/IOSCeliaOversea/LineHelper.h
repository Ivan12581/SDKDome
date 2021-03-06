//
//  LineHelper.h
//  Unity-iPhone
//
//  Created by mini on 8/6/20.
//

#import <Foundation/Foundation.h>
#import "cDelegate.h"
NS_ASSUME_NONNULL_BEGIN

@interface LineHelper : NSObject
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)share:(const char*) jsonString;
- (BOOL)shareMessage:(NSString *)message;
@end

NS_ASSUME_NONNULL_END
