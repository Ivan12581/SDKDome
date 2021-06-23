//
//  RAStarCommon.h
//  RaStarGame
//
//  Created by 朱运 on 2017/11/6.
//  Copyright © 2017年 zhuyun. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

/// 分享结果通知 除复制链接及关闭页面外所有返回均为第三方应用的原始数据返回
FOUNDATION_EXPORT NSNotificationName const _Nonnull RaStarShareResultNotificationName;

typedef NS_ENUM(NSInteger,RSManagerResultCode){
    /** 购买成功 */
    RSManagerResultSucceed = 1,
    /** 关闭支付 */
    RSManagerResultCancel,
    /** 请求AppStore失败 */
    RSManagerResultFail_AppStore,
    /** 网络请求失败 */
    RSManagerResultFail_NetWork,
    /** 下单失败 */
    RSManagerResultFail_Bills,
    /** 初始化失败 */
    RSManagerResultFail_Init,
    /** 没有购买产品 */
    RSManagerResultFail_NonProducts,
    /** 发货失败 */
    RSManagerResultFail_Delivery,
    /** 支付结果未知,需要去服务端查询 */
    RSManagerResultFail_Unknow,
    /** 当前货币禁止支付 */
    RSManagerResultFail_DisableForeignCurrency,
    /** 账号异常 */
    RSManagerResultFail_AbnormalPurchase,
    /** web失败 */
    RSManagerResultFail_Error,
    /** 未实名认证禁止支付 */
    RSManagerResultFail_UnAuthenticatedUser,
    /** 有参数为空 */
    RSManagerResultFail_NULL,
    /** 下单操作进行中 */
    RSManagerResultFail_Creating,
    /** 重复订阅 */
    RSManagerResultFail_DuplicateSubscriptions,
    /** 其他 */
    RSManagerResultFail_Other,
    
};

typedef NS_ENUM(int, RSShareType) {
    RSShareType_WeChat = 0,
    RSShareType_TimeLine,
    RSShareType_Weibo,
    RSShareType_QQ,
    RSShareType_QZone,
};

typedef NS_ENUM(NSInteger, RSUserActionType){
    /// 创角
    RSUserActionType_CreateRole = 1,
    /// 进入服务器
    RSUserActionType_EnterServer,
    /// 角色升级
    RSUserActionType_RoleUpgrade,
    /// 角色改名
    RSUserActionType_ChangeRoleName,
};

@protocol RaStarInitDelegate,RaStarLoginDelegate,RaStarManagerDelegate,RaStarServiceDelegate;
@interface RaStarCommon : NSObject
@property (nonatomic,weak) id<RaStarInitDelegate> initCallback;
@property (nonatomic,weak) id<RaStarLoginDelegate> loginCallback;
@property (nonatomic,weak) id<RaStarManagerDelegate> managerCallback;
@property (nonatomic,weak) id<RaStarServiceDelegate> serviceCallBack;
@property (nonatomic,getter=successInit)BOOL  isSuceessInit;
/**
 设备ID
 */
@property (nonatomic, copy) NSString *deviceID;
/**
 是否使用SDK 充值提示弹窗（默认NO）
 */
@property (nonatomic, assign) BOOL useSDKAlertView;
/**
 设置蒙层颜色 (默认灰色)
 */
@property (nonatomic, assign) UIColor *mainBackGroundColor;
/**
 Google 翻译Key（必须在调用Google翻译方法前设置）
 */
@property (nonatomic, copy) NSString *googleTranslateKey;
#pragma mark - 基本功能
/**
 初始化
 waring 请确认网络状态正常后再行调用初始化方法
 */
-(void)registerSDK;
-(void)addInitDelegate:(id)RaStarInitDelegate;
/**
 登录界面展示
 */
-(void)showLoginView;
/**
 主动注销登录
 */
-(void)loginViewOut;
-(void)addLoginDelegate:(id)RaStarLoginDelegate;
/**
 发起支付
 
 @param amount   金额,单位是元;
 @param name     角色名字
 @param level    角色等级;
 @param ID       角色id;
 @param s_id     游戏的频道号;
 @param s_name   游戏频道名称
 @param subject  购买的商品名称
 @param order_no 订单号(不能重复);
 @param ext      扩展字段(不超过255个字符,默认值为:@"0")
 */
-(void)managerWithAmount:(NSString *)amount andName:(NSString *)name andLevel:(NSString *)level andRoleid:(NSString *)ID andSid:(NSString *)s_id andSname:(NSString *)s_name andSubject:(NSString *)subject andOrder_no:(NSString *)order_no andExt:(NSString *)ext;
-(void)addManagerDelegate:(id)RaStarpayDelegate;

/**
 展示客服界面
 */
-(void)showService;
-(void)addServiceDelegate:(id)delegate;
#pragma mark - 聊天及数据上报
/// 游戏角色上报
/// @param createTime 创角时间，单位ms
/// @param action 角色执行动作
/// @param roleID 角色id
/// @param roleName 角色名称
/// @param roleLevel 角色等级
/// @param serverID 角色选服的服务器id（可以是合服后的服务器id）
/// @param serverName 角色选服的服务器名称（可以是合服后的服务器名称）
/// @param realServerName 角色创建所在的服务器名称（非合服后的服务器名称)
/// @param realServerID 角色创建所在的服务器id（非合服后的服务器id）
/// @param vip 角色vip等级
/// @param partyName 加入的公会名称
- (void)uploadUserRoleCreateRoleTime:(UInt64)createTime Action:(RSUserActionType)action RoleID:(NSString *)roleID RoleName:(NSString *)roleName RoleLevel:(int)roleLevel ServerID:(NSString *)serverID ServerName:(NSString *)serverName RealServerName:(NSString *)realServerName RealServerID:(NSString *)realServerID Vip:(int)vip PartyName:(NSString *_Nullable)partyName;

/**
 消息聊天
 
 @param where 发言频道（1公屏/世界 2私聊）
 @param guild_id 公会id
 @param message 消息内容
 @param role_level 角色等级
 @param vip vip等级
 @param pay 充值金额（元）
*/

-(void)sendChat:(NSString *)where Guild_id:(NSString *)guild_id Message:(NSString *)message Role_level:(NSString *)role_level Vip:(NSString *)vip Pay:(NSString *)pay;

/*
code -  信息
200 - 可以聊天
1002 - 获取配置失败
1003 - 验证签名失败
1001 - 参数错误
2001 - 暂停登陆
2004 - 用户未登录
3002 - 账号信息不匹配
1518 - 该用户暂无实名认证信息
1522 - 该身份证已使用，请更换其他身份证
*/
/// 获取用户实名信息
/// @param infoCode 实名信息代码
- (void)getUserVerifiedInfo:(void(^)(int code))infoCode;

/// 手动显示实名认证页面
- (void)showUserVerifiedView:(void(^)(BOOL verifiedType))verifyType;

#pragma mark - 游戏网络连接相关
/// 游戏断线（非退出账号，仅游戏状态断线）
- (void)gameOffline;

/// 游戏重连（游戏状态断线后重新连接，非切换星辉账号重新登陆）
-(void)gameBackOnline;
#pragma mark - 分享
#pragma mark 使用星辉分享UI
/// 显示分享页面（链接） - 如参数传空则使用星辉默认模板
/// @param title 分享标题
/// @param message 分享内容
/// @param image 分享图片
/// @param shareUrl 分享链接
- (void)shareWithTitle:(NSString * _Nullable)title Message:(NSString * _Nullable)message Image:(UIImage * _Nullable)image ShareUrl:(NSString * _Nullable)shareUrl;

/// 显示分享页面（纯文本） - 如果参数为空则使用星辉默认模板
/// @param message 分享内容
- (void)shareWithMessage:(NSString * _Nullable)message;

/// 显示分享页面（图片） - 如果参数为空则使用星辉默认图片
/// @param image 支持UIImage
- (void)shareWithImage:(UIImage* _Nullable)image;

/// 显示分享页面（视频）
/// @param title 分享标题 - 空则使用星辉默认标题
/// @param message 视频描述 - 空则使用星辉默认描述
/// @param videoUrl 视频地址 - 必填，为空则直接返回错误
- (void)shareWithVideoTitle:(NSString * _Nullable)title Message:(NSString * _Nullable)message VideoURL:(NSString * _Nonnull)videoUrl;
#pragma mark 自定义分享UI
/// 显示分享页面（链接） - 如参数传空则使用星辉默认模板
/// @param title 分享标题
/// @param message 分享内容
/// @param image 分享图片
/// @param shareUrl 分享链接
/// @param type 分享类型
- (void)shareWithTitle:(NSString * _Nullable)title Message:(NSString * _Nullable)message Image:(UIImage * _Nullable)image ShareUrl:(NSString * _Nullable)shareUrl ShareType:(RSShareType)type;

/// 显示分享页面（纯文本） - 如果参数为空则使用星辉默认模板
/// @param message 分享内容
/// @param type 分享类型
- (void)shareWithMessage:(NSString * _Nullable)message ShareType:(RSShareType)type;

/// 显示分享页面（图片） - 如果参数为空则使用星辉默认图片
/// @param image 支持UIImage
/// @param type 分享类型
- (void)shareWithImage:(UIImage* _Nullable)image ShareType:(RSShareType)type;

/// 显示分享页面（视频）
/// @param title 分享标题 - 空则使用星辉默认标题
/// @param message 视频描述 - 空则使用星辉默认描述
/// @param videoUrl 视频地址 - 必填，为空则直接返回错误
/// @param type 分享类型
- (void)shareWithVideoTitle:(NSString * _Nullable)title Message:(NSString * _Nullable)message VideoURL:(NSString * _Nonnull)videoUrl ShareType:(RSShareType)type;
#pragma mark - 基本设置
/**
 设置UA 请于 AppDelegate 中的 didFinishLaunchingWithOptions 中实现
 */
- (void)setUserAgent;
/**
 设置关闭 请于 AppDelegate 中的 applicationWillTerminate 中实现
 */
- (void)setCloseType;
/**
 设置开启 请于 AppDelegate 中的 applicationDidBecomeActive 中实现
*/
- (void)setStartType;
/**
 设置系统回调:跳转返回
 */
-(BOOL)openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
/**
 设置系统回调:Universal Links系统回调
 - (BOOL)application:(UIApplication *)applicati_Nonnullon continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void(^)(NSArray * __nullable restorableObjects))restorationHandler
 */
-(BOOL)handleUniversalLink:(NSUserActivity *)userActivity;
/**
 设置系统handleOpenURL方法
 - (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
 */
- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;

/// 设置系统 application: handleOpenURL: 方法
- (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url;

/**
 设置游戏屏幕方向
 UIInterfaceOrientationMaskPortrait
 UIInterfaceOrientationMaskLandscape
 @param screenOrientation 方向
 */
-(void)setScreenOrientation:(UIInterfaceOrientationMask)screenOrientation;
/**
 支持的方向

 @return 支持的方向
 */
-(UIInterfaceOrientationMask)supportedInterfaceOrientations;
/**
 是否支持旋转

 @param viewController  viewController
 @return bool
 */
-(BOOL)shouldAutorotate:(UIViewController *)viewController;

/**
 引导进入App Store评论（请与运营&广告商议后确定调用时机。*非必接，未上线前调用无效）
 */
- (void)guideToAppStoreReview;
#pragma mark - 翻译相关
/**
 翻译语言
 
 @param language 翻译的语言
 @param target 目标语言
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
 @param failure 返回失败
 */
- (void)languageSupportWithSuccess:(void(^)(id responseObject))success failure:(void(^)(id error))failure;

+(RaStarCommon *)sharedInstance;
@end

//1,初始化
@protocol RaStarInitDelegate<NSObject>
@required
/**
 初始化成功
 */
-(void)onInitSuccess;

/**
 初始化失败
 */
-(void)onInitFail;
@end

@protocol RaStarLoginDelegate<NSObject>
/**
 登录成功

 @param lwToken 登录成功后的token值
 */
-(void)onCommonLoginSuccess:(NSString *)lwToken Uid:(NSString *)uid;
/**
 登录失败
 */
-(void)onCommonLoginFail;
/**
 注销登录
 */
-(void)onCommonLoginOut;
@end

@protocol RaStarManagerDelegate<NSObject>
/**
 支付状态
 
 @param RSManagercode 支付状态说明
 */
-(void)onManagerState:(RSManagerResultCode)RSManagercode;
@end
@protocol RaStarServiceDelegate<NSObject>
/**
 关闭客服界面
 */
-(void)serviceClose;

/**
 提交客服的问题描述信息

 返回一个字典数据 对应key value 值如下：
 openid   对应返回openid
 role_id 对应返回角色ID
 question_title 对应返回问题标题
 question_desc 对应返回问题描述
 image_url 图片地址数组 可能为空数组 研发拿数组值前先判断数组元素 避免发生不必要的闪退
 */
- (void)callBackServiceIssueSubmitWithMsgDict:(NSDictionary *)msgDict;

@end


NS_ASSUME_NONNULL_END
