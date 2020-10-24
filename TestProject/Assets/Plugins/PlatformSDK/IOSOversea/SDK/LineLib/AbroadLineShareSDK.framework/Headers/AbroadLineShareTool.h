//
//  AbroadLineShareTool.h
//  TestLineShareDemo
//
//  Created by 卢奇春 on 2018/11/12.
//  Copyright © 2018年 卢奇春. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN


@interface AbroadLineShareTool : NSObject

+ (instancetype)lineShareTool;



/**
 分享链接

 @param url 分享链接地址
 */
- (void)shareWithURL:(NSString *)url;


/**
 分享图片

 @param image 图片资源
 */
- (void)shareWithImage:(UIImage *)image;

@end

NS_ASSUME_NONNULL_END
