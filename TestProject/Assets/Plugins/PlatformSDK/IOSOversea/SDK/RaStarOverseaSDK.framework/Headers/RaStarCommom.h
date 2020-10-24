//
//  RaStarCommom.h
//  AbroadSDK
//
//  Created by 沈达 on 2017/6/5.
//  Copyright © 2017年 Vincent. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

typedef NS_ENUM(NSUInteger,RSFunctionViewType){
    /** 绑定页 */
    RSFunctionTypeBind = 1,
    /** 切换账号页面 */
    RSFunctionTypeSwitchAccount,
};

@protocol RaStarFBShareDelegate <NSObject>
@optional
//分享成功
-(void)fbShareSuccess;
//分享失败
-(void)fbShareFailure;
//取消分享
-(void)fbShareCancel;

@end

@protocol RaStarLoginDelegate <NSObject>
@optional
//登录成功返回Token、绑定信息
- (void)onLoginSuccess:(NSString *)accessToken BindInfo:(NSDictionary *)infoDic;
//登录失败
- (void)onLoginFail;
//注销登录
- (void)onLogout;
//关闭登录界面
- (void)onCloseLoginView;

@end
@protocol RaStarPayDelegate <NSObject>
@optional
//购买成功 订单号
-(void)onPaySuccess:(NSString *)orderID;
//购买失败 失败原因
-(void)onPayFailure:(NSString *)failure;
//取消支付
-(void)onPayCancel;

@end
@protocol RaStarBindDelegate <NSObject>
@optional
//绑定成功 绑定信息
- (void)onBindSeccess:(NSDictionary *)infoDic;

@end
@interface RaStarCommom : NSObject <RaStarLoginDelegate , RaStarPayDelegate , RaStarBindDelegate,RaStarFBShareDelegate>
@property (nonatomic , weak) id <RaStarLoginDelegate> loginDelegate;
@property (nonatomic , weak) id <RaStarPayDelegate> payDelegate;
@property (nonatomic , weak) id <RaStarBindDelegate> bindDelegate;
@property (nonatomic , weak) id <RaStarFBShareDelegate> shareDelegate;
@property (nonatomic, copy, readonly) NSString *deviceID;//设备ID

+ (instancetype)sharedInstance;


#pragma mark 初始化及登录方法
+ (void)setUserAgent;
/**
 设置AppsFlyer相关
 此方法需在 'didFinishLaunchingWithOptions' 中实现
 */
+ (void)setAppsFlyer;

/**
 追踪应用打开
 此方法需在 'applicationDidBecomeActive' 中实现
 */
+ (void)trackAppLaunch;

/**
 登录方法

 @param delegate 登录协议
 */
- (void)loginWithDelegate:(id)delegate;

/**
 添加绑定回调

 @param delegate 绑定协议
 */
- (void)setRSBindDelegate:(id)delegate;

#pragma mark 客服相关

/**
 展示客服提单

 @param close 关闭客服提单
 */
- (void)showCustomerService:(void(^)(void))close;

#pragma mark 数据收集相关
/**
 创建角色上传

 @param roleId 角色ID --必传
 @param roleLevel 角色等级 --必传
 @param roleName 角色名称 --必传
 @param serverId 服务器ID --必传
 @param serverName 服务器名称 --必传
 @param partyName 公会名称 --非必传 传默认值“无”
 @param timeLevelUp 等级升级时间戳 --非必传 传默认值-1
 @param vip vip等级 --非必传 传默认值0
 @param timeCreate 创建角色时间戳 --非必传 传默认值-1
 @param balance 账号余额 --非必传 传默认值0
 @param extra 扩展字段 --非必传 传默认值extra
 */
+ (void)collectCreateRoleWithRoleID:(NSString *)roleId RoleLevel:(NSString *)roleLevel RoleName:(NSString *)roleName ServerId:(NSString *)serverId ServerName:(NSString *)serverName PartyName:(NSString *)partyName TimeLevelUp:(NSString *)timeLevelUp Vip:(NSString *)vip TimeCreate:(NSString *)timeCreate Balance:(NSString *)balance Extra:(NSString *)extra;

/**
 进入服务器上传
 
 @param roleId 角色ID --必传
 @param roleLevel 角色等级 --必传
 @param roleName 角色名称 --必传
 @param serverId 服务器ID --必传
 @param serverName 服务器名称 --必传
 @param partyName 公会名称 --非必传 传默认值“无”
 @param timeLevelUp 等级升级时间戳 --非必传 传默认值-1
 @param vip vip等级 --非必传 传默认值0
 @param timeCreate 创建角色时间戳 --非必传 传默认值-1
 @param balance 账号余额 --非必传 传默认值0
 @param extra 扩展字段 --非必传 传默认值extra
 */
+ (void)collectEnterServerWithRoleID:(NSString *)roleId RoleLevel:(NSString *)roleLevel RoleName:(NSString *)roleName ServerId:(NSString *)serverId ServerName:(NSString *)serverName PartyName:(NSString *)partyName TimeLevelUp:(NSString *)timeLevelUp Vip:(NSString *)vip TimeCreate:(NSString *)timeCreate Balance:(NSString *)balance Extra:(NSString *)extra;

/**
 角色升级上传
 
 @param roleId 角色ID --必传
 @param roleLevel 角色等级 --必传
 @param roleName 角色名称 --必传
 @param serverId 服务器ID --必传
 @param serverName 服务器名称 --必传
 @param partyName 公会名称 --非必传 传默认值“无”
 @param timeLevelUp 等级升级时间戳 --非必传 传默认值-1
 @param vip vip等级 --非必传 传默认值0
 @param timeCreate 创建角色时间戳 --非必传 传默认值-1
 @param balance 账号余额 --非必传 传默认值0
 @param extra 扩展字段 --非必传 传默认值extra
 */
+ (void)collectLevelUpWithRoleID:(NSString *)roleId RoleLevel:(NSString *)roleLevel RoleName:(NSString *)roleName ServerId:(NSString *)serverId ServerName:(NSString *)serverName PartyName:(NSString *)partyName TimeLevelUp:(NSString *)timeLevelUp Vip:(NSString *)vip TimeCreate:(NSString *)timeCreate Balance:(NSString *)balance Extra:(NSString *)extra;

/**
 新手教程完成数据收集
 */
+ (void)collectTutorialCompletion;

#pragma mark 支付相关
/**
 发起支付
 
 @param amount 金额
 @param name 角色名称
 @param level 角色等级
 @param rid 角色id
 @param sid 区服号
 @param sname 区服名称
 @param subject 产品名称
 @param orderNum 订单号
 @param ext 扩展字段
 @param delegate 代理
 */
- (void)payWithAmount:(NSString *)amount Name:(NSString *)name Level:(NSString *)level Roleid:(NSString *)rid Sid:(NSString *)sid Sname:(NSString *)sname Subject:(NSString *)subject OrderNum:(NSString *)orderNum Ext:(NSString *)ext Delegate:(id)delegate;

/**
 展示悬浮球
 */
- (void)showMenuCenter;

/**
 隐藏悬浮球
 */
- (void)hidenMenuCenter;

/**
 facebook 分享

 @param shareDelegate 指定代理协议
 */
- (void)shareWithDelegate:(id)shareDelegate;

/**
 分享链接
 
 @param urlString 链接地址 --- 必传
 @param fromViewController 展示控制器 --- 必传
 */
- (void)shareWithLink:(NSString *)urlString fromViewController:(UIViewController *)fromViewController;


/**
 分享照片
 
 @param image 分享的图片资源
 @param fromViewController 控制器
 */
- (void)shareWithImage:(UIImage *)image fromViewController:(UIViewController *)fromViewController;

/**
 分享视频 视频来源于相册
 
 @param fromViewController 展示控制器
 */
- (void)shareVideoWithFromViewController:(UIViewController *)fromViewController;

/**
 AF数据上报打点

 @param action 上报节点名称
 */
- (void)afCollectRunLogWithAction:(NSString *)action;


/**
 展示独立的界面
 (登录成功才可以调用)
 @param type 界面类型
 */
- (void)showSingleViewWithType:(RSFunctionViewType)type;


/**
 返回应用
 */
- (void)enterForegroundWithApplication:(UIApplication *)application url:(NSURL *)url;


/**
 报告深度链接

 @param userActivity userActivity
 @param restorationHandler restorationHandler
 */
- (void)afContinueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray * _Nullable))restorationHandler;

/**
 AF追踪打开链接
 iOS10 以上
 @param url url
 @param options options
 */
- (void)afOpenURL:(NSURL *)url options:(NSDictionary *)options;

/**
 AF追踪打开链接
 iOS 10 以下
 @param url url
 @param sourceApplication sourceApplication
 @param annotation annotation
 */
- (void)afOpenURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;


//2.1.4 新增
/**
 无界面展示注销接口
 */
- (void)logoutInterface;

@end
