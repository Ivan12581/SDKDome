//
//  DAGetNativeAdvertisingCampaignMediationSchedule.h
//  AdPopcornDALib
//
//  Created by 김민석 on 2016. 8. 25..
//  Copyright © 2016년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface DAGetNativeAdvertisingCampaignMediationSchedule : NSObject

@property (nonatomic, copy) NSString *adNetworkName;
@property (nonatomic, strong) NSDictionary *integrationKey;
@property (nonatomic, unsafe_unretained) NSInteger integrationType;

@end