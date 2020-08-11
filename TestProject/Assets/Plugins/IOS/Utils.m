//
//  Utils.m
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//



#import "Utils.h"
@implementation Utils{
    NSString *ServiceName;   //保存的名称集
    NSString *UUIDKeyName;   //UUID保存的
}
static Utils *UtilsIns = nil;
+(Utils*)sharedInstance{
    if (UtilsIns == nil) {
        UtilsIns = [Utils new];
    }
    return UtilsIns;
}

-(instancetype)init{
    self = [super init];
    if (self != nil) {
        ServiceName = [NSBundle mainBundle].bundleIdentifier;
        UUIDKeyName = @"UUID";
    }
    return self;
}
#pragma mark -取出
-(NSString *)getValueWithKey:(NSString *)key{
    return [SSKeychain passwordForService:ServiceName account:key];
}
#pragma mark - 保存
-(void)saveValueWithKey:(NSString *)key value:(NSString *)value{
    [SSKeychain setPassword:value forService:ServiceName account:key];
}
#pragma mark -删除
-(void)deleteValueWithKey:(NSString *)key{
    [SSKeychain deletePasswordForService:ServiceName account:key];
}
#pragma mark -取UUID
-(NSString *)GetUUID{
    NSString *UUID = [self getValueWithKey:UUIDKeyName];
    if(!UUID){
        UUID = [[UIDevice currentDevice] identifierForVendor].UUIDString;
        [self saveValueWithKey:UUIDKeyName value:UUID];
    }
    return UUID;
}
#pragma mark -取UUID
-(NSString *)GetDisPalyName{
    NSDictionary *infoDictionary = [[NSBundle mainBundle] infoDictionary];
    NSString *app_Name = [infoDictionary objectForKey:@"CFBundleDisplayName"];
    return app_Name;
}
@end
