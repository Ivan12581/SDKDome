//
//  NaverCafeTool.h
//  NaverDemo
//
//  Created by 卢奇春 on 2018/8/15.
//  Copyright © 2018年 卢奇春. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef NS_ENUM(NSUInteger,NaverCafeSDKViewType){
    /** 主页 */
    NaverCafeSDKViewHome = 1,//注：先调了主页后才能单独调以下
    /** 文章 */
    NaverCafeSDKViewArticles,
    /** 媒体 */
    NaverCafeSDKViewMedias,
    /** 搜索 */
    NaverCafeSDKViewSerach
    
};

@protocol NaverCafeToolDelegate <NSObject>
@optional

//展示NaverCafe SDK界面
- (void)ncSDKToolViewDidLoad;
//关闭NaverCafe SDK界面
- (void)ncSDKToolViewDidUnLoad;
//注册cafe成功
- (void)ncSDKToolJoinedCafeMember;
//点击了截图按钮
- (void)ncSDKToolWidgetPostArticleWithImage;
//上传帖子成功
- (void)ncSDKToolPostedArticleAtMenuSuccess;
//完成录制
- (void)ncSDKToolWidgetSuccessVideoRecord;

@end

@interface NaverCafeTool : NSObject<NaverCafeToolDelegate>

@property (nonatomic,weak) id<NaverCafeToolDelegate>cafeToolDelegate;

//创建单例对象
+ (instancetype)shareNaverCafeTool;


/**
 Naver cafe SDK 初始化接口

 @param clientId 必传
 @param loginClientSecret 必传
 @param cafeId 必传
 @param loginURLScheme 必传
 @param parentViewController 父控制器 必传
 @param showScreenShot 点击悬浮球 是否展示截图按钮 YES 表示展示
 @param showTransparentSlider 是否展示可调节透明度 NO表示展示可调节
 @param showFloatingBall 是否展示悬浮球 YES 表示展示
 */
- (void)initWithLoginClientId:(NSString *)clientId loginClientSecret:(NSString *)loginClientSecret cafeId:(NSInteger)cafeId loginURLScheme:(NSString *)loginURLScheme parentViewController:(id)parentViewController showScreenShot:(BOOL)showScreenShot showTransparentSlider:(BOOL)showTransparentSlider showFloatingBall:(BOOL)showFloatingBall;
//添加代理
- (void)addCafeToolDelegate:(id)cafeToolDelegate;


/**
 展示Nacer Cafe SDK 界面

 @param type 界面类型
 */
- (void)presentNaverCafeSDKViewWithType:(NaverCafeSDKViewType)type;

/**
 使用Naver账号将登陆信息设置到登录客体
 在AppDelegate.m 中调用
 @param url url
 */
- (void)finishNaverLoginWithURL:(NSURL *)url;

/**
 横屏的时候需要在加载主页界面前调用以下方法
 */
- (void)setUIInterfaceOrientationIsLandscape;

@end
