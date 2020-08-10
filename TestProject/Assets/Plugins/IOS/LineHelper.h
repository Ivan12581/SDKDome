//
//  LineHelper.h
//  Unity-iPhone
//
//  Created by mini on 8/6/20.
//

#import <Foundation/Foundation.h>
//#import <LineSDK/LineSDK.h>
NS_ASSUME_NONNULL_BEGIN

@interface LineHelper : NSObject
//@property (nonatomic, strong) LineSDKAPI *apiClient;
+(id)sharedInstance;
- (BOOL)shareMessage:(NSString *)message;
@end

NS_ASSUME_NONNULL_END
