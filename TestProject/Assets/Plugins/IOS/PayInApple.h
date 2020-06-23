//
//  PayInApple.h
//  Unity-iPhone
//
//  Created by mini on 6/23/20.
//
#import "UnityAppController.h"
#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>
#import <Foundation/Foundation.h>
@interface PayInApple: UnityAppController<SKProductsRequestDelegate,SKPaymentTransactionObserver>
+(void)test;
-(void)Init;
@end
