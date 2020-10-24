//
//  DABannerView.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 4. 8..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <UIKit/UIKit.h>

#import "DAError.h"


@protocol DABannerViewDelegate;

typedef enum _DABannerViewSizeType
{
    DABannerViewSize320x50
} DABannerViewSizeType;

@interface DABannerView : UIView


@property (nonatomic, unsafe_unretained) id<DABannerViewDelegate> delegate;

// 최소 30초에서 최대 120초로 설정.
@property (nonatomic, unsafe_unretained) NSInteger adRefreshRate;

/*!
 @abstract
 banner view instance 생성.
 @param size            banner view size : 현재는 320 * 50만 지원.
 @param origin          banner view를 노출시킬 position(x, y).
 @param appKey          app key
 @param spotKey         spot key
 @param viewController  banner view를 노출시킬 view controller
 */
- (instancetype)initWithBannerViewSize:(DABannerViewSizeType)size origin:(CGPoint)origin appKey:(NSString *)appKey spotKey:(NSString *)spotKey viewController:(UIViewController *)viewController NS_DESIGNATED_INITIALIZER;

/*!
 @abstract
 banner 광고 요청.
 rotation 변경시, 화면에 맞는 banner 광고 요청시 사용.
 */
- (void)loadRequest;


/*!
 @abstract
 banner 스케쥴 정지.
 */
- (void)stopAd;

@end


@protocol DABannerViewDelegate <NSObject>

@optional
/*!
 @abstract
 banner 광고 load 완료시(성공시), 호출된다.
 */
- (void)DABannerViewDidLoadAd:(DABannerView *)bannerView;

/*!
 @abstract
 banner 광고 load 실패시, 호출된다.
 */
- (void)DABannerView:(DABannerView *)bannerView didFailToReceiveAdWithError:(DAError *)error;

/*!
 @abstract
 banner 광고 클릭시, 호출된다.
 */
- (void)DABannerViewWillLeaveApplication:(DABannerView *)bannerView;


@end