//
//  LiveOpsPushTool.h
//  LiveOperationDemo
//
//  Created by 卢奇春 on 2018/6/19.
//  Copyright © 2018年 卢奇春. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface LiveOpsPushTool : NSObject

typedef void (^LiveOpsPushToolPushEnableCallback)(BOOL result);

+ (instancetype)sharePushTool;


/**
 推送初始化
 */
- (void)initPushWithLaunchOptions:(NSDictionary *)launchOptions;


/**
 设置唯一用户标识
 必接 在登录成功回调跟切换登录成功回调中调用
 @param userId userId
 */
- (void)setUserId:(NSString *)userId;


/**
 本地推送

 @param localPid 唯一识别标识
 @param date 日期
 @param bodyText 要显示的文字
 */
- (void)registerLocalPushNotification:(NSInteger)localPid date:(NSDate*)date body:(NSString*)bodyText;
/**
 取消本地推送 (非必接)
 (若无此需求可不接)
 @param localPid 唯一标识
 */
- (void)cancelLocalPush:(NSInteger)localPid;

/**
 设置定向数据 (非必接)
 @param data 定向的用户数据
 @param key 设置用户自定义数据 Key
 @param isPushLiveOps 是否同步到LiveOps后台 YES 表示同步
 */
- (void)setTargetingData:(id)data withKey:(NSString *)key isPushLiveOps:(BOOL)isPushLiveOps;

/**
 呼叫 定向数据 (非必接)
 
 @param key 设置用户自定义数据 Key
 @return 返回定向用户数据
 */
- (id)getTargetingDataWithKey:(NSString *)key;


@end
