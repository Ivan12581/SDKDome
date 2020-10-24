//
//  DAPopupAd.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 4. 22..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#import "DAError.h"


@protocol DAPopupAdDelegate;

@interface DAPopupAd : NSObject


@property (nonatomic, unsafe_unretained) id<DAPopupAdDelegate> delegate;

/*!
 @abstract
 popup ad instance 생성.
 @param appKey          app key
 @param spotKey         spot key
 @param viewController  popup ad를 노출시킬 view controller
 */

- (instancetype)initWithKey:(NSString *)appKey spotKey:(NSString *)spotKey NS_DESIGNATED_INITIALIZER;

/*!
 @abstract
 popup ad load가 완료되면(성공하면), presentFromViewController 메소드를 호출하여 노출시킨다.
 @param viewController  interstitial ad를 노출시킬 view controller
 */
- (BOOL)presentFromViewController:(UIViewController *)viewController;

@end

@protocol DAPopupAdDelegate <NSObject>

@optional

/*!
 @abstract
 popup 광고 load 완료시(성공시), 호출된다.
 */
- (void)DAPopupAdDidLoad:(DAPopupAd *)popupAd;

/*!
 @abstract
 popup 광고 load 실패시, 호출된다.
 */
- (void)DAPopupAd:(DAPopupAd *)popupAd didFailToReceiveAdWithError:(DAError *)error;

/*!
 @abstract
 popup 광고 클릭시, 호출된다.
 */
- (void)DAPopupAdWillLeaveApplication:(DAPopupAd *)popupAd;

/*!
 @abstract
 popup 광고 open 직전에 호출된다.
 */
- (void)willOpenDAPopupAd;

/*!
 @abstract
 popup 광고 open 직후에 호출된다.
 */
- (void)didOpenDAPopupAd;

/*!
 @abstract
 popup 광고 close 직전에 호출된다.
 */
- (void)willCloseDAPopupAd;

/*!
 @abstract
 popup 광고 close 직후에 호출된다.
 */
- (void)didCloseDAPopupAd;

@end
