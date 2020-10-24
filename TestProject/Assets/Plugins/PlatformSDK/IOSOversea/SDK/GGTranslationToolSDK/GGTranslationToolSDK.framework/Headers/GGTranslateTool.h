//
//  GGTranslateTool.h
//  TranslationDemo
//
//  Created by 卢奇春 on 2018/3/12.
//  Copyright © 2018年 卢奇春. All rights reserved.
//

#import <Foundation/Foundation.h>

FOUNDATION_EXPORT NSString * const translateURL;
FOUNDATION_EXPORT NSString * const detectURL;
FOUNDATION_EXPORT NSString * const languageURL;

@interface GGTranslateTool : NSObject


//创建对象
+ (instancetype)shareTranslateTool;

/**
 翻译语言

 @param language 翻译的语言
 @param target 目标语言(如英语：en)必传
 @param source 源语言(如简体中文：zh-CN)非必传，不指定源语言翻译自动检测
 @param success 返回成功
 @param failure 返回失败
 */
- (void)translateLanguage:(NSString *)language target:(NSString *)target source:(NSString *)source success:(void(^)(NSString *targetLanguage))success failure:(void(^)(id error))failure;
/**
 检测语言

 @param language 检测的语言
 @param success 返回成功
 @param failure 返回失败
 */
- (void)detectLanguage:(NSString *)language success:(void(^)(NSString *source))success failure:(void(^)(id error))failure;

/**
 语言支持

 @param success 返回成功
 */
- (void)languageSupportWithSuccess:(void(^)(id responseObject))success failure:(void(^)(id error))failure;

@end
