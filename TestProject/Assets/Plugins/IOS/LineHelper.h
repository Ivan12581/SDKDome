//
//  LineHelper.h
//  Unity-iPhone
//
//  Created by mini on 8/6/20.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface LineHelper : NSObject
+(id)sharedInstance;
- (BOOL)shareMessage:(NSString *)message;
@end

NS_ASSUME_NONNULL_END
