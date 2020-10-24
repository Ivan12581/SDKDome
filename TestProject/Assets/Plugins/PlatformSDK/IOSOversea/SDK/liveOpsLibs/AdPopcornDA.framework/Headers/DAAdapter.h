//
//  DAAdapter.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 4. 10..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

#import <UIKit/UIKit.h>

#import "DABannerView.h"

#import "DADemo.h"


@protocol DaAdapterDelegate;

typedef enum _DAMediationGender
{
    DAMediationGenderMale,
    DAMediationGenderFemale
} DAMediationGender;

typedef enum _DAAdType
{
    DAAdBannerType,
    DAAdInterstitialType,
    DANativeAdType
} DAAdType;



// Set Logging Component
#undef AdPopcornLogComponent
#define AdPopcornLogComponent lcl_cAdPopcorn

@interface DAAdapter : NSObject
{
    UIViewController *_viewController;
    CGPoint _origin;
    CGSize _size;
    DAAdType _adType;
    DABannerView *_bannerView;
}

@property (nonatomic, unsafe_unretained) id<DaAdapterDelegate> delegate;

@property (nonatomic, strong) NSDictionary *integrationKey;

@property (nonatomic, unsafe_unretained, readonly) BOOL isSupportInterstitialAd;

- (void)setViewController:(UIViewController *)viewController origin:(CGPoint)origin size:(CGSize)size bannerView:(DABannerView *)bannerView;
- (void)setViewController:(UIViewController *)viewController;
- (void)setNativeAdType;
- (void)loadAd;
- (void)closeAd;
- (void)loadRequest;

@property (NS_NONATOMIC_IOSONLY, readonly) CGSize adSize;

- (void)setAge:(NSInteger)age;
- (void)setGender:(DAGender)gender;


@end

@protocol DaAdapterDelegate <NSObject>

@optional

- (void)DAAdapterBannerViewDidLoadAd:(UIView *)bannerView adapter:(DAAdapter *)adapter;
- (void)DAAdapterBannerView:(UIView *)bannerView didFailToReceiveAdWithError:(NSError *)error adapter:(DAAdapter *)adapter;

- (void)DAAdapterBannerViewWillLeaveApplication:(UIView *)bannerView;

- (void)DAAdapterInterstitialAdDidLoadAd:(NSObject *)interstitialAd;

- (void)DAAdapterInterstitial:(NSObject *)interstitialAd didFailToReceiveAdWithError:(NSError *)error adapter:(DAAdapter *)adapter;

- (void)DAAdapterInterstitialWillLeaveApplication:(NSObject *)interstitialAd;

- (void)DAAdapterNativeAdDidLoadAd:(NSObject *)nativeAd networkName:(NSString *)networkName;
- (void)DAAdapterNativeAd:(NSObject *)nativeAd didFailToReceiveAdWithError:(NSError *)error adapter:(DAAdapter *)adapter;

@end
