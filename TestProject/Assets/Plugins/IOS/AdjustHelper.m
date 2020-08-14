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
    NSLog(@"--AppleHelper---InitSDK---");
}
-(void)Event:(const char*) jsonString{
    NSString *evnetToken = [NSString stringWithUTF8String:jsonString];
    [self commonEvent:evnetToken];
}
#pragma mark --上报普通事件
-(void)commonEvent:(NSString *)evnetToken{
    ADJEvent *event = [ADJEvent eventWithEventToken:evnetToken];
    [Adjust trackEvent:event];
}
#pragma mark --上报支付数据
-(void)purchaseEvent:(NSString *)appleOrderID{
    if(![[Utils sharedInstance] getValueWithKey:@"price"]){
        return;
    }
    NSString *priceStr = [[Utils sharedInstance] getValueWithKey:@"price"];
    double priceDouble = [priceStr doubleValue];
    NSString *currency = [[Utils sharedInstance] getValueWithKey:@"CurrencyCode"];
    ADJEvent *event = [ADJEvent eventWithEventToken:@"q5u2a6"];
    [event setRevenue:priceDouble currency:currency];
    [event setTransactionId:appleOrderID];
    [Adjust trackEvent:event];
//    [[AdjustHelper sharedInstance] purchaseEvent:@"q5u2a6" andRevenue:&test andCurrency:currency];
}
#pragma mark --游戏启动登陆事件
-(void)launchEvent{
    ADJEvent *event = [ADJEvent eventWithEventToken:@"4pvqgy"];
    [Adjust trackEvent:event];
}
@end
