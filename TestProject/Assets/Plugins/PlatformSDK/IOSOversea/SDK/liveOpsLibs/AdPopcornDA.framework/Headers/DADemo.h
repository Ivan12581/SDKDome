//
//  DADemo.h
//  IgaworksAd
//
//  Created by wonje,song on 2014. 6. 2..
//  Copyright (c) 2014년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef enum _DAGender
{
    DAGenderFemale = 1,
    DAGenderMale = 2,
    DAGenderUnknown = 3
} DAGender;

@interface DADemo : NSObject


@property (nonatomic, unsafe_unretained) DAGender gender;
@property (nonatomic, unsafe_unretained) NSInteger age;

@property (nonatomic, unsafe_unretained, readonly) double latitude;
@property (nonatomic, unsafe_unretained, readonly) double longitude;
@property (nonatomic, unsafe_unretained, readonly) double accuracyInMeters;

+ (DADemo *)sharedInstance;

- (void)setLocationWithLatitude:(double)latitude
                      longitude:(double)longitude
                       accuracy:(double)accuracyInMeters;


@end
