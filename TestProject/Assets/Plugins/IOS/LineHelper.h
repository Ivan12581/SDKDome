//
//  LineHelper.h
//  Unity-iPhone
//
//  Created by mini on 8/6/20.
//

#import <Foundation/Foundation.h>
#import "cDelegate.h"
//#import <LineSDK/LineSDK.h>
NS_ASSUME_NONNULL_BEGIN

@interface LineHelper : NSObject
//@property (nonatomic, strong) LineSDKAPI *apiClient;
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)share:(const char*) jsonString;
- (BOOL)shareMessage:(NSString *)message;
@end

NS_ASSUME_NONNULL_END
