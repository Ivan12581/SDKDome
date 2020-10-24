//
//  DANativeAd.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 7. 11..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>


#import "DAError.h"

#import "DAGetNativeAdvertisingResult.h"
#import "DAAdapter.h"

@protocol DANativeAdDelegate;

@interface DANativeAd : NSObject

@property (nonatomic, unsafe_unretained) id<DANativeAdDelegate> delegate;

@property (nonatomic, strong) DAGetNativeAdvertisingResult *nativeAdvertisingResult;

@property (nonatomic, strong) id nativeAdvertisingResultJson;

@property (nonatomic, strong) NSString *adNetworkName;

@property (nonatomic, strong) NSObject *mediationNativeAdResult;

/*!
 @abstract
 native ad instance 생성.
 @param appKey          app key
 @param spotKey         spot key
 */
- (instancetype)initWithKey:(NSString *)appKey spotKey:(NSString *)spotKey NS_DESIGNATED_INITIALIZER;

/*!
 @abstract
 native 광고 요청한다.
 */
- (void)loadAd;

/*!
 @abstract
 광고 노출시, 호출한다.
 */
- (void)callImpression:(NSString *)impressionUrl;

/*!
 @abstract
 광고 클릭시, 호출한다.
 */
- (void)click:(NSString *)clickUrl;

@end


@protocol DANativeAdDelegate <NSObject>

@optional

/*!
 @abstract
 native 광고 load 완료시(성공시), 호출된다.
 */
- (void)DANativeAdDidFinishLoading:(DANativeAd *)nativeAd;

/*!
 @abstract
 native 광고 load 실패시, 호출된다.
 */
- (void)DANativeAd:(DANativeAd *)nativeAd didFailWithError:(DAError *)error;


@end