//
//  Utils.h
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#ifndef Utils_h
#define Utils_h
#import <Foundation/Foundation.h>
#import "SSKeychain.h"
@interface Utils : NSObject
+(id)sharedInstance;
-(NSString *)getValueWithKey:(NSString *)key;
-(void)saveValueWithKey:(NSString *)key value:(NSString *)value;
-(void)deleteValueWithKey:(NSString *)key;
-(NSString *)GetUUID;
-(BOOL *)IsHighLevel;
@end
#endif /* Utils_h */
