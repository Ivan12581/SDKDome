//
//  AdjustHelper.m
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import "AdjustHelper.h"
#import "Utils.h"
@implementation AdjustHelper{
        id IOSBridgeHelper;
}
static AdjustHelper *AdjustHelperIns = nil;
+(AdjustHelper*)sharedInstance{
    if (AdjustHelperIns == nil) {
        AdjustHelperIns = [AdjustHelper new];
    }
    return AdjustHelperIns;
}
-(void)InitSDK:(id<cDelegate>)Delegate{
    IOSBridgeHelper = Delegate;
    //点击游戏图标，启动游戏后，触发该事件      //ad启动统计
    [self commonEvent:@"4pvqgy"];
    NSString *idfa = [Adjust idfa];
    NSLog(@" ---ios AdjustHelper--InitSDK-idfa--: %@", idfa);

}
-(void)Event:(const char*) jsonString{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    NSLog(@" ---ios AdjustHelper--Event-: %@", dict);
    NSString *evnetToken = [dict valueForKey:@"evnetToken"];

    [self commonEvent:evnetToken];
}
#pragma mark --上报普通事件
-(void)commonEvent:(NSString *)evnetToken{
    ADJEvent *event = [ADJEvent eventWithEventToken:evnetToken];
    [Adjust trackEvent:event];
}
#pragma mark --官方支付统计
-(void)OfficialPurchaseEvent:(NSString *)appleOrderID{
    if(![[Utils sharedInstance] getValueWithKey:@"price"]){
        return;
    }
    NSString *price = [[Utils sharedInstance] getValueWithKey:@"price"];
    NSString *currency = [[Utils sharedInstance] getValueWithKey:@"CurrencyCode"];
    [self PurchaseEvent:@"r7ugmi" andPrice:price andCurrency:currency andOrderID:appleOrderID];
    //[self PurchaseEvent:@"r7ugmi" andPrice:price andCurrency:currency andOrderID:[appleOrderID stringByAppendingString:@"-total"]];
}
#pragma mark --第三方MyCard支付统计
-(void)ThirdPurchaseEvent:(const char*) jsonString{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    NSLog(@" ---ios AdjustHelper--ThirdPurchaseEvent-: %@", dict);
    NSString *price = [dict valueForKey:@"price"];
    NSString *currency = [dict valueForKey:@"currency"];
//    NSString *productID = [dict valueForKey:@"productID"];
    NSString *orderID = [dict valueForKey:@"orderID"];
    [self PurchaseEvent:@"j33kyv" andPrice:price andCurrency:currency andOrderID:orderID];
    //[self PurchaseEvent:@"r7ugmi" andPrice:price andCurrency:currency andOrderID:[orderID stringByAppendingString:@"-total"]];
}
-(void)PurchaseEvent:(NSString *)eventToken andPrice:(NSString *)price andCurrency:(NSString *)currency andOrderID:(NSString *)orderID{
    
    ADJEvent *event = [ADJEvent eventWithEventToken:eventToken];
    [event setRevenue:[price doubleValue] currency:currency];
    [event setTransactionId:orderID];
    [Adjust trackEvent:event];
}
#pragma mark --游戏启动登陆事件
-(void)launchEvent{
    ADJEvent *event = [ADJEvent eventWithEventToken:@"4pvqgy"];
    [Adjust trackEvent:event];
}
@end
