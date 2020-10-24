//
//  DAAdSize.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 4. 9..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "DABannerView.h"

typedef enum _DABannerViewRealSizeType
{
    DABannerViewSizeIphonePotriat320x50
} DABannerViewRealSizeType;

@interface DAAdSize : NSObject

+ (CGSize)adSize:(DABannerViewSizeType)bannerViewSizeType;

+ (CGSize)adRealSize:(DABannerViewRealSizeType)bannerViewRealSizeType;

@end
