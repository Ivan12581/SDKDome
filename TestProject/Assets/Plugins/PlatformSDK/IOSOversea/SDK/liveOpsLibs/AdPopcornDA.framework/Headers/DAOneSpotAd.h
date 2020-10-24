//
//  DAOneSpotAd.h
//  AdPopcornDALib
//
//  Created by 김민석 on 2017. 1. 2..
//  Copyright © 2017년 mick. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#import "DAError.h"

typedef NS_ENUM(NSInteger, VideoGuideViewType) {
    VideoGuideViewCPIType = 1,
    VideoGuideViewBrandingType = 2
};

@protocol DAOneSpotAdDelegate;

@interface DAOneSpotAd : NSObject


@property (nonatomic, unsafe_unretained) id<DAOneSpotAdDelegate> delegate;

/*!
 @abstract
 onespot ad instance 생성.
 @param appKey          app key
 @param spotKey         spot key
 @param viewController  onespot ad를 노출시킬 view controller
 */

- (instancetype)initWithKey:(NSString *)appKey spotKey:(NSString *)spotKey NS_DESIGNATED_INITIALIZER;

/*!
 @abstract
 onespot ad load가 완료되면(성공하면), presentFromViewController 메소드를 호출하여 노출시킨다.
 @param viewController  interstitial ad를 노출시킬 view controller
 */
- (BOOL)presentFromViewController:(UIViewController *)viewController;
@end

@protocol DAOneSpotAdDelegate <NSObject>

@optional

/*!
 @abstract
 onespot 광고 load 완료시(성공시), 호출된다.
 */
- (void)DAOneSpotAdDidLoad:(DAOneSpotAd *)onespotAd;

/*!
 @abstract
 onespot 광고 load 실패시, 호출된다.
 */
- (void)DAOneSpotAd:(DAOneSpotAd *)onespotAd didFailToReceiveAdWithError:(DAError *)error;

/*!
 @abstract
 onespot 광고 클릭시, 호출된다.
 */
- (void)DAOneSpotAdWillLeaveApplication:(DAOneSpotAd *)onespotAd;

/*!
 @abstract
 onespot 광고 open 직전에 호출된다.
 */
- (void)willOpenDAOneSpotAd;

/*!
 @abstract
 onespot 광고 open 직후에 호출된다.
 */
- (void)didOpenDAOneSpotAd;

/*!
 @abstract
 onespot 광고 close 직전에 호출된다.
 */
- (void)willCloseDAOneSpotAd;

/*!
 @abstract
 onespot 광고 close 직후에 호출된다.
 */
- (void)didCloseDAOneSpotAd;

/*!
 @abstract
 onespot 비디오 광고가 끝가지 재생 완료 된후에 호출된다.
 */
- (void)didCompleteDAOneSpotVideoAd;

@end
