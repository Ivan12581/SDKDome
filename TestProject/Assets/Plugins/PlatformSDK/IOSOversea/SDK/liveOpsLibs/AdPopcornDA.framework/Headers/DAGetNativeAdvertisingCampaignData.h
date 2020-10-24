//
//  DAGetNativeAdvertisingCampaignData.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 7. 11..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

#import "DAGetNativeAdvertisingCampaignDataIcon.h"

#import "DAGetNativeAdvertisingCampaignDataScreenshot.h"

@interface DAGetNativeAdvertisingCampaignData : NSObject

@property (nonatomic, copy) NSString *title;
@property (nonatomic, strong) DAGetNativeAdvertisingCampaignDataIcon *icon;
@property (nonatomic, strong) DAGetNativeAdvertisingCampaignDataScreenshot *screenShot;

@property (nonatomic, copy) NSString *summaryDescription;
@property (nonatomic, copy) NSString *detailDescription;
@property (nonatomic, copy) NSString *rating;
@property (nonatomic, copy) NSString *action;

@end
