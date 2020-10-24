//
//  AdBrixCommerceProductModel.h
//  AdBrixLib
//
//  Created by igaworks on 2017. 1. 13..
//  Copyright © 2017년 hoiil,jung. All rights reserved.
//

#ifndef AdBrixCommerceProductModel_h
#define AdBrixCommerceProductModel_h

#import "AdBrixCommerceProductAttrModel.h"
#import "AdBrixCommerceProductCategoryModel.h"


#import <Foundation/Foundation.h>

@interface AdBrixCommerceProductModel : NSObject

- (AdBrixCommerceProductModel*)create :(NSString*)productId productName:(NSString*)productName price:(double)price discount:(double)discount quantity:(NSUInteger)quantity currencyString:(NSString *)currencyString category:(AdBrixCommerceProductCategoryModel*)categories extraAttrsMap:(AdBrixCommerceProductAttrModel *)extraAttrs;

- (NSString *)getProductName;
- (NSUInteger)getQuantity;
- (NSString *)getProductId;
- (double)getPrice;
- (double)getDiscount;
- (NSString *)getCurrencyString;
- (NSString *)getCategories;
- (NSDictionary *)getExtraAttrs;

@end

#endif /* AdBrixCommerceProductModel_h */
