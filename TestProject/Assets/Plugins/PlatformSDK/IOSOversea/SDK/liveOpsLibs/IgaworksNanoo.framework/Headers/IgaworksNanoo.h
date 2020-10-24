//
//  IgaworksNanoo.h
//  IgaworksNanooLib
//
//  Created by wonje,song on 2015. 6. 3..
//  Copyright (c) 2015년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@protocol IgaworksNanooDelegate;

@interface IgaworksNanoo : NSObject

@property (nonatomic, unsafe_unretained) id<IgaworksNanooDelegate> delegate;


/*!
 @abstract
 singleton IgaworksNanoo 객체를 반환한다.
 */
+ (IgaworksNanoo *)shared;

/*!
 @abstract
 getNanooFanPage를 요청합니다.
 @param viewController             nanoo fan page를 modal로 노출합니다. viewController가 nil이면, nanoo fan page를 노출하지 않습니다.
 */
+ (void)getNanooFanPage:(UIViewController *)viewController;


@end

@protocol IgaworksNanooDelegate <NSObject>

// getNanooFanPage 요청이 성공하면, nanoo fan page url을 return합니다.
- (void)getNanooFanPageDidComplete:(NSString *)nanooFanPageurl;

//  getNanooFanPage 요청이 실패한경우, error를 return합니다.
- (void)getNanooFanPageFailedWithError:(NSError *)error;

/*!
 @abstract
 nanoo fan page가 열리기 전에 호출된다.
 
 @discussion
 */
- (void)willOpenNanooFanPage;

/*!
 @abstract
 nanoo fan page가 열린직 후 호출된다.
 
 @discussion
 */
- (void)didOpenNanooFanPage;


/*!
 @abstract
 nanoo fan page가 닫히기 전에 호출된다.
 
 @discussion
 */
- (void)willCloseNanooFanPage;

/*!
 @abstract
 nanoo fan page가 닫힌직 후 호출된다.
 
 @discussion
 */
- (void)didCloseNanooFanPage;

@end
