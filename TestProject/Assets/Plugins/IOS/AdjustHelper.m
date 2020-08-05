//
//  AdjustHelper.m
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import "AdjustHelper.h"

@implementation AdjustHelper
static AdjustHelper *AdjustHelperIns = nil;
+(AdjustHelper*)sharedInstance{
    if (AdjustHelperIns == nil) {
        AdjustHelperIns = [AdjustHelper new];
    }
    return AdjustHelperIns;
}
#pragma mark --上报普通事件
-(void)commonEvent:(NSString *)evnetToken{
    ADJEvent *event = [ADJEvent eventWithEventToken:evnetToken];
    [Adjust trackEvent:event];
}
#pragma mark --上报支付数据
-(void)purchaseEvent:(NSString *)evnetToken andRevenue:(double *)revenue andCurrency:(NSString *)currencyName{
    ADJEvent *event = [ADJEvent eventWithEventToken:evnetToken];

//MOP澳门币  CNY人名币 EUR欧元 GBP英镑 HKD港元 JPY日元 KRW 韩元 THB泰铢 TWD新台币 VND越南盾 AUD 澳元
    [event setRevenue:*revenue currency:currencyName];
    [Adjust trackEvent:event];
}
#pragma mark --游戏启动登陆事件
-(void)launchEvent{
    ADJEvent *event = [ADJEvent eventWithEventToken:@"4pvqgy"];
    [Adjust trackEvent:event];
}
@end
