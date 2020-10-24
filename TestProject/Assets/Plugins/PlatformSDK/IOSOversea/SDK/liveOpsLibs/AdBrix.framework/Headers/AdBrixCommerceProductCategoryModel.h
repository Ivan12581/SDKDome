//
//  AdBrixCommerceProductModel.h
//  AdBrixLib
//
//  Created by igaworks on 2017. 1. 13..
//  Copyright © 2017년 hoiil,jung. All rights reserved.
//

#ifndef AdBrixCommerceProductCategoryModel_h
#define AdBrixCommerceProductCategoryModel_h


#import <Foundation/Foundation.h>
/*!
 @abstract
 AdBrixCommerceProductCategoryModel 입니다 create 셀렉터로 생성합니다

 */
@interface AdBrixCommerceProductCategoryModel : NSObject

/*!
@abstract
AdBrixCommerceProductCategoryModel의 AdBrixCommerceProductCategoryModel을 생성한다.
최대 5개까지 생성할 수 있다.

@param string:category1            category.(opnational)

*/
+ (AdBrixCommerceProductCategoryModel*)create :(NSString *)category1;
/*!
 @abstract
 AdBrixCommerceProductCategoryModel의 AdBrixCommerceProductCategoryModel을 생성한다.
 최대 5개까지 생성할 수 있다.
 범주가 큰 카테고리부터 작은 순으로 인자를 집어넣는다
 
 @param string:category1            category.(opnational)
 @param string:category2            category.(opnational)



 */

+ (AdBrixCommerceProductCategoryModel*)create :(NSString *)category1 category2:(NSString *)category2;
/*!
 @abstract
 AdBrixCommerceProductCategoryModel의 AdBrixCommerceProductCategoryModel을 생성한다.
 최대 5개까지 생성할 수 있다.
 범주가 큰 카테고리부터 작은 순으로 인자를 집어넣는다
 
 @param string:category1            category.(opnational)
 @param string:category2            category.(opnational)
 @param string:category3            category.(opnational)

 */

+ (AdBrixCommerceProductCategoryModel*)create :(NSString *)category1 category2:(NSString *)category2 category3:(NSString *)category3;
/*!
 @abstract
 AdBrixCommerceProductCategoryModel의 AdBrixCommerceProductCategoryModel을 생성한다.
 최대 5개까지 생성할 수 있다.
 범주가 큰 카테고리부터 작은 순으로 인자를 집어넣는다
 
 @param string:category1            category.(opnational)
 @param string:category2            category.(opnational)
 @param string:category3            category.(opnational)
 @param string:category4            category.(opnational)
 */

+ (AdBrixCommerceProductCategoryModel*)create :(NSString *)category1 category2:(NSString *)category2 category3:(NSString *)category3 category4:(NSString *)category4;
/*!
 @abstract
 AdBrixCommerceProductCategoryModel의 AdBrixCommerceProductCategoryModel을 생성한다.
 최대 5개까지 생성할 수 있다.
 범주가 큰 카테고리부터 작은 순으로 인자를 집어넣는다
 
 @param string:category1            category.(opnational)
 @param string:category2            category.(opnational)
 @param string:category3            category.(opnational)
 @param string:category4            category.(opnational)
 @param string:category5            category.(opnational)
 */

+ (AdBrixCommerceProductCategoryModel*)create :(NSString *)category1 category2:(NSString *)category2 category3:(NSString *)category3 category4:(NSString *)category4 category5:(NSString *)category5;

-(void)setCategoryFullString:(NSArray *) _categories;

- (NSString *)getFullString;

@end

#endif /* AdBrixCommerceProductCategoryModel_h */
