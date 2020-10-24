//
//  DAInterstitialView.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 4. 14..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "DAError.h"


@protocol DAInterstitialAdDelegate;

@interface DAInterstitialAd : NSObject


@property (nonatomic, unsafe_unretained) id<DAInterstitialAdDelegate> delegate;
@property (nonatomic, unsafe_unretained, getter = isInterstitialAdIsVisible) BOOL interstitialAdIsVisible;


/*!
 @abstract
 interstitial ad instance 생성.
 @param appKey          app key
 @param spotKey         spot key
 @param viewController  interstitial ad를 노출시킬 view controller
 */
- (instancetype)initWithKey:(NSString *)appKey spotKey:(NSString *)spotKey viewController:(UIViewController *)viewController NS_DESIGNATED_INITIALIZER;


/*!
 @abstract
 interstitial ad load가 완료되면(성공하면), presentFromViewController 메소드를 호출하여 노출시킨다.
 @param viewController  interstitial ad를 노출시킬 view controller
 */
- (BOOL)presentFromViewController:(UIViewController *)viewController;


/*!
 @abstract
 읿본 광고 네트웍사, NendAd, IMobile을 위한, interstitial ad를 노출시킬때 호출.
 */
- (void)showInterstitialAdForNendAd_IMobile;


@end

@protocol DAInterstitialAdDelegate <NSObject>

/*!
 @abstract
 intersitial 광고 load 실패시, 호출된다.
 */
- (void)DAInterstitialAd:(DAInterstitialAd *)interstitialAd didFailToReceiveAdWithError:(DAError *)error;

@optional
/*!
 @abstract
 intersitial 광고 load 완료시(성공시), 호출된다.
 */
- (void)DAInterstitialAdDidLoad:(DAInterstitialAd *)interstitialAd;

/*!
 @abstract
 intersitial 광고 클릭시, 호출된다.
 */
- (void)DAInterstitialAdWillLeaveApplication:(DAInterstitialAd *)interstitialAd;

/*!
 @abstract
 intersitial 광고 open 직전에 호출된다.
 */
- (void)willOpenDAInterstitialAd;

/*!
 @abstract
 intersitial 광고 open 직후에 호출된다.
 */
- (void)didOpenDAInterstitialAd;

/*!
 @abstract
 intersitial 광고 close 직전에 호출된다.
 */
- (void)willCloseDAInterstitialAd;

/*!
 @abstract
 intersitial 광고 close 직후에 호출된다.
 */
- (void)didCloseDAInterstitialAd;


@end
