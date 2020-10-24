//
//  IgaworksCoupon.h
//  IgaworksCouponLib
//
//  Created by wonje,song on 2015. 6. 1..
//  Copyright (c) 2015년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol IgaworksCouponDelegate;

@interface IgaworksCoupon : NSObject


@property (nonatomic, unsafe_unretained) id<IgaworksCouponDelegate> delegate;

/*!
 @abstract
 singleton IgaworksCoupon 객체를 반환한다.
 */
+ (IgaworksCoupon *)shared;

/*!
 @abstract
 쿠폰 입력 창을 통해, 입력받은 쿠폰 코드를 검증한다.
 */
+ (void)showCoupon;

/*!
 @abstract
 쿠폰 코드를 검증한다.
 */
+ (void)checkCoupon:(NSString *)code;

@end


@protocol IgaworksCouponDelegate <NSObject>
@optional

/*!
 @abstract
 쿠폰 코드 검증이 완료되면, 결과를 전달한다.
 */
- (void)igaworksCouponValidationDidComplete:(BOOL)result message:(NSString *)message;

@end