//
//  DAGetNativeAdvertisingCampaign.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 7. 11..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

#import "DAGetNativeAdvertisingCampaignData.h"

@interface DAGetNativeAdvertisingCampaign : NSObject


@property (nonatomic, copy) NSString *dataString;
@property (nonatomic, strong) DAGetNativeAdvertisingCampaignData *data;

@property (nonatomic, copy) NSString *impressionURL;
@property (nonatomic, copy) NSString *key;
@property (nonatomic, copy) NSString *name;
@property (nonatomic, copy) NSString *redirectURL;

@end
