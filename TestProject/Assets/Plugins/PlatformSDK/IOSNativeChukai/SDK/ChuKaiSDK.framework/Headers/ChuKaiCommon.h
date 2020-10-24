//

//  Game
//
//  Created by 朱运 on 2017/11/6.
//  Copyright © 2017年 zhuyun. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
typedef NS_ENUM(NSInteger,ChukaiCode){
    /** 购买成功 */
    ChuKaiResultSucceed = 1,
    /** 关闭支付 */
    ChuKaiResultCancel,
    /** 请求AppStore失败 */
    ChuKaiResultFail_AppStore,
    /** 网络请求失败 */
    ChuKaiResultFail_NetWork,
    /** 下单失败 */
    ChuKaiResultFail_Bills,
    /** 初始化失败 */
    ChuKaiResultFail_Init,
    /** 没有购买产品 */
    ChuKaiResultFail_NonProducts,
    /** 发货失败 */
    ChuKaiResultFail_Delivery,
    /** 支付结果未知,需要去服务端查询 */
    ChuKaiResultFail_Unknow,
    /** 当前货币禁止支付 */
    ChuKaiResultFail_DisableForeignCurrency,
    /** 账号异常 */
    ChuKaiResultFail_AbnormalPurchase,
    /** 未实名认证禁止支付 */
    ChuKaiResultFail_UnAuthenticatedUser,
    /** 有参数为空 */
    ChuKaiResultFail_Null,
    /** 下单操作进行中 */
    ChuKaiResultFail_Creating,
    /** 重复订阅 */
    ChuKaiResultFail_DuplicateSubscriptions,
    
};

@protocol ChuKaiInitDelegate,ChuKaiLoginDelegate,ChuKaiPayDelegate;
@interface ChuKaiCommon : NSObject
@property (nonatomic,weak) id<ChuKaiInitDelegate> initCallback;
@property (nonatomic,weak) id<ChuKaiLoginDelegate> loginCallback;
@property (nonatomic,weak) id<ChuKaiPayDelegate> payCallback;
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
 初始化
 
 @param anAppKey appkey
 @param appID    appid
 @param cch_id   cch_id（渠道号）
 @param md_id    md_id（广告号）
 */
-(void)registerSDKWithAppKey:(NSString *)anAppKey andAppid:(NSString *)appID cch_ID:(NSString *)cch_id md_ID:(NSString *)md_id;
-(void)addInitDelegate:(id)ChuKaiInitDelegate;
/**
 登录界面展示
 */
-(void)showLoginView;
/**
 主动注销登录
 */
-(void)loginViewOut;
-(void)addLoginDelegate:(id)ChuKaiLoginDelegate;
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
-(void)payWithAmount:(NSString *)amount andName:(NSString *)name andLevel:(NSString *)level andRoleid:(NSString *)ID andSid:(NSString *)s_id andSname:(NSString *)s_name andSubject:(NSString *)subject andOrder_no:(NSString *)order_no andExt:(NSString *)ext;
-(void)addPayDelegate:(id)ChuKaipayDelegate;

/**
 创建游戏角色的时候，上传角色信息
 
 @param roleID 角色ID --必传
 @param roleName 角色名称 --必传
 @param roleLevel 角色等级 --必传
 @param serverID 服务器ID --必传
 @param serverName 服务器名称 --必传
 @param balance 账号余额 --非必传 传默认值0
 @param vip vip等级 --非必传 传默认值0
 @param partyName 公会名称 --非必传 传默认值“无”
 @param extra 扩展字段 --非必传 传默认值“0”
 */
-(void)uploadCreateInfoRoleID:(NSString *)roleID roleName:(NSString *)roleName roleLevel:(NSString *)roleLevel serverID:(NSString *)serverID serverName:(NSString *)serverName balance:(NSString *)balance vip:(NSString *)vip partyName:(NSString *)partyName extra:(NSString *)extra;

/**
 角色进入服务器,上传角色信息
 
 @param roleID 角色ID --必传
 @param roleName 角色名称 --必传
 @param roleLevel 角色等级 --必传
 @param serverID 服务器ID --必传
 @param serverName 服务器名称 --必传
 @param balance 账号余额 --非必传 传默认值0
 @param vip vip等级 --非必传 传默认值0
 @param partyName 公会名称 --非必传 传默认值“无”
 @param extra 扩展字段 --非必传 传默认值“0”
 */
-(void)uploadEnterInfoRoleID:(NSString *)roleID roleName:(NSString *)roleName roleLevel:(NSString *)roleLevel serverID:(NSString *)serverID serverName:(NSString *)serverName balance:(NSString *)balance vip:(NSString *)vip partyName:(NSString *)partyName extra:(NSString *)extra;
/**
 角色升级的时候,上传角色信息
 
 @param roleID 角色ID --必传
 @param roleName 角色名称 --必传
 @param roleLevel 角色等级 --必传
 @param serverID 服务器ID --必传
 @param serverName 服务器名称 --必传
 @param balance 账号余额 --非必传 传默认值0
 @param vip vip等级 --非必传 传默认值0
 @param partyName 公会名称 --非必传 传默认值“无”
 @param extra 扩展字段 --非必传 传默认值“0”
 */

-(void)uploadUp_levelInfoRoleID:(NSString *)roleID roleName:(NSString *)roleName roleLevel:(NSString *)roleLevel serverID:(NSString *)serverID serverName:(NSString *)serverName balance:(NSString *)balance vip:(NSString *)vip partyName:(NSString *)partyName extra:(NSString *)extra;
/**
 角色改名的时候,上传角色信息(如果游戏没有这一功能,可不调用)
 
 @param roleID 角色ID --必传
 @param roleName 角色名称 --修改之后角色名称
 @param roleLevel 角色等级 --必传
 @param serverID 服务器ID --必传
 @param serverName 服务器名称 --必传
 @param balance 账号余额 --非必传 传默认值0
 @param vip vip等级 --非必传 传默认值0
 @param partyName 公会名称 --非必传 传默认值“无”
 @param extra 扩展字段 --旧的角色名称
 */

-(void)uploadUpdateInfoRoleID:(NSString *)roleID roleName:(NSString *)roleName roleLevel:(NSString *)roleLevel serverID:(NSString *)serverID serverName:(NSString *)serverName balance:(NSString *)balance vip:(NSString *)vip partyName:(NSString *)partyName extra:(NSString *)extra;

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

+(ChuKaiCommon *)sharedInstance;

/**
 设置登录模块背景图层颜色
 */
-(void)loginViewBackgroundColor:(UIColor *)color alpha:(CGFloat)alpha;



@end

//1,初始化
@protocol ChuKaiInitDelegate<NSObject>
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

@protocol ChuKaiLoginDelegate<NSObject>
/**
 登录成功

 @param lwToken 登录成功后的token值
 */
-(void)onCommonLoginSuccess:(NSString *)lwToken;
/**
 登录失败
 */
-(void)onCommonLoginFail;
/**
 注销登录
 */
-(void)onCommonLoginOut;
@end

@protocol ChuKaiPayDelegate<NSObject>
/**
 支付状态
 
 @param RSPaycode 支付状态说明
 */
-(void)onPayState:(ChukaiCode)RSPaycode;
@end
