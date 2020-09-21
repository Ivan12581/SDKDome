//
//  ApplePurchase.h
//  Unity-iPhone
//
//  Created by mini on 7/21/20.
//

#ifndef ApplePurchase_h
#define ApplePurchase_h

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import "SSKeychain.h"
#import "cDelegate.h"
@interface ApplePurchase:NSObject<SKProductsRequestDelegate,SKPaymentTransactionObserver>
//@property (nonatomic, weak) id<cDelegate> CbDelegate;
+(id)sharedInstance;
-(void)InitSDK:(id<cDelegate>)Delegate;
-(void)Pay: (const char *) jsonString;
@end
#endif /* ApplePurchase_h */
