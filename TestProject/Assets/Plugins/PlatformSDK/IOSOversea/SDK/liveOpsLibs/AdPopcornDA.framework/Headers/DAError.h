//
//  DAError.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 5. 14..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

/// DA error domain.
extern NSString *const kDAErrorDomain;


/// NSError codes for GAD error domain.
typedef NS_ENUM(NSInteger, DAErrorCode) {
    DAException = 200,
    DAInvalidParameter = 1000,
    DAUnknownServerError = 9999,
    DAInvalidMediaKey = 2000,
    DAInvalidSpotKey = 2030,
    DAEmptyCampaign = 2100,
    DANativeSpotDoesNotInitialized = 3200,
    DAMediationInvalidIntegrationKey = 4000,
    DAMediationAdapterNotInitialized = 4001,
    DAServerTimeout = 5000,
    DALoadAdFailed = 5001,
    DANoAd = 5002
};

@interface DAError : NSError


@end
