//
//  AdBrixCommerceProductModel.h
//  AdBrixLib
//
//  Created by igaworks on 2017. 1. 13..
//  Copyright © 2017년 hoiil,jung. All rights reserved.
//

#ifndef AdBrixCommerceProductAttrModel_h
#define AdBrixCommerceProductAttrModel_h


#import <Foundation/Foundation.h>

@interface AdBrixCommerceProductAttrModel : NSObject

+ (AdBrixCommerceProductAttrModel*)create :(NSDictionary *)attrData;

- (void)AdBrixCommerceProductAttrModelsetKeyAndVal :(int)pIndex key:(NSString *)key value:(NSString *)value;

- (NSString *)AdBrixCommerceProductAttrModelgetKey :(int)pIndex;

- (NSString *)AdBrixCommerceProductAttrModelgetValue :(int)pIndex;

@end

#endif /* AdBrixCommerceProductAttrModel_h */
