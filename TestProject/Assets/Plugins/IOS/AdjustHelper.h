//
//  AdjustHelper.h
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import <Foundation/Foundation.h>
#import <AdjustSdk/Adjust.h>
#import "cDelegate.h"
NS_ASSUME_NONNULL_BEGIN

@interface AdjustHelper : NSObject
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)Event:(const char*) jsonData;
-(void)commonEvent:(NSString *)evnetToken;
-(void)purchaseEvent:(NSString *)appleOrderID;
@end

NS_ASSUME_NONNULL_END
