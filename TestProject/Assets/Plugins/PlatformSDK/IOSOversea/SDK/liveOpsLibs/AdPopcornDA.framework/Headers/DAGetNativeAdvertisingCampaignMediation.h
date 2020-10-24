//
//  DAGetNativeAdvertisingCampaignMediation.h
//  AdPopcornDALib
//
//  Created by 김민석 on 2016. 8. 25..
//  Copyright © 2016년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

#import "DAGetNativeAdvertisingCampaignMediationSchedule.h"

@interface DAGetNativeAdvertisingCampaignMediation : NSObject

@property (nonatomic, unsafe_unretained) NSInteger mediationRefreshRate;

@property (nonatomic, unsafe_unretained, getter = isUseECPMOption) BOOL useECPMOption;

@property (nonatomic, strong) NSMutableArray *mediationScheduleArray;

- (void)addMediationScheduleArray:(NSArray *)objects;
- (void)removeMediationScheduleArray;
- (void)addMediationScheduleObject:(DAGetNativeAdvertisingCampaignMediationSchedule *)object;
- (void)removeMediationScheduleObject:(DAGetNativeAdvertisingCampaignMediationSchedule *)object;

@end