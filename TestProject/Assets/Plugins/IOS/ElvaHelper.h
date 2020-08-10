//
//  ElvaHelper.h
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import <Foundation/Foundation.h>
#import <ElvaChatServiceSDK/ElvaChatServiceSDK.h>
NS_ASSUME_NONNULL_BEGIN

@interface ElvaHelper : NSObject
+(id)sharedInstance;
-(void)show:(const char*) jsonData;
@end

NS_ASSUME_NONNULL_END
