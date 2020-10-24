//
//  BaiduAction.h
//  ocpc
//
//  Created by Bao,Shiwei on 2019/1/19.
//  Copyright © 2019年 Baidu. All rights reserved.
//

#import <Foundation/Foundation.h>

extern NSString *const BaiduSDKActionNamePageView;        /**<  页面访问  */
extern NSString *const BaiduSDKActionNameRegister;        /**<  注册  */
extern NSString *const BaiduSDKActionNameViewContent;     /**<  内容浏览  */
extern NSString *const BaiduSDKActionNameConsult;         /**<  咨询  */
extern NSString *const BaiduSDKActionNameAddToCart;       /**<  加入购物车 */
extern NSString *const BaiduSDKActionNamePurchase;        /**<  购买 */
extern NSString *const BaiduSDKActionNameSearch;          /**<  搜索 */
extern NSString *const BaiduSDKActionNameAddToWishList;   /**<  收藏 */
extern NSString *const BaiduSDKActionNameInitiateCheckOut;/**<  开始结算 */
extern NSString *const BaiduSDKActionNameCompleteOrder;   /**<  下单 */
extern NSString *const BaiduSDKActionNameDownloadApp;     /**<  下载应用 */
extern NSString *const BaiduSDKActionNameRate;            /**<  评分 */
extern NSString *const BaiduSDKActionNameReservation;     /**<  预约 */
extern NSString *const BaiduSDKActionNameShare;           /**<  分享 */
extern NSString *const BaiduSDKActionNameApply;           /**<  申请 */
extern NSString *const BaiduSDKActionNameClaimOffer;      /**<  领取卡券 */
extern NSString *const BaiduSDKActionNameNavigate;        /**<  导航 */
extern NSString *const BaiduSDKActionNameProductRecommend;/**<  商品推荐 */
extern NSString *const BaiduSDKActionNameLogin;           /**<  登录  */
extern NSString *const BaiduSDKActionNameBindSocialAccount; /**<  绑定社交h账号  */
extern NSString *const BaiduSDKActionNameCreateRole;        /**<  创建角色  */
extern NSString *const BaiduSDKActionNameUpgrade;           /**<  升级  */
extern NSString *const BaiduSDKActionNameCompleteTeachingTask;  /**<  完成教学任务  */
extern NSString *const BaiduSDKActionNameAuthorizationTrust;    /**<  授权信任  */
extern NSString *const BaiduSDKActionNameCashOut;               /**< 提现  */

/**
百度oCPC SDK
当前版本 1.0.2
*/
@interface BaiduAction : NSObject

extern NSString *const BaiduSDKActionParamKeyOuterActionId; /**< 自定义去重Id */
extern NSString *const BaiduSDKActionParamKeyAudienceType;  /**<  标示客户类型 */
extern NSString *const BaiduSDKActionParamKeyPurchaseMoney; /**<  购买付费金额 */

typedef enum BaiduActionParamAudienceType{
    
    BaiduActionParamAudienceType_NewAudience = 0,     // 新客户
    BaiduActionParamAudienceType_UsedAudience = 1,    // 老客户
    
} BaiduActionParamAudienceType;

/**
 在接入OCPC行为数据SDK时，请在App启动的时候调用初始化方法。初始化方法调用时请传入数据源UserActionSetId和在后台看到的secretKey密钥串。
 
 @param actionSetId 数据源id，在DMP系统后台可以看见创建的数据源id
 @param secretKey 密钥串，在DMP系统后台可以看见分配的密钥串
 */
+ (void)init:(NSString *)actionSetId secretKey:(NSString *)secretKey;

/**
 在上报OCPC行为数据时，系统提供若干标准的行为类型actionName，若需要上报自定义actionName，请与OCPC联系，并在参数名中传入自定义的字符串。
 
 @param actionName 行为类型名，参见BaiduSDKActionName
 @param actionParam 行为参数，只支持单层结构，暂不支持嵌套的行为参数数据。
 */
+ (void)logAction:(NSString *)actionName actionParam:(NSDictionary *)actionParam;

/**
 设置的应用使用间隔，为新用户激活（默认30天）
 
 @param days 天数（最小1）
 */
+ (void)setActivateInterval:(NSInteger)days;

/**
 设置是否显示调试log， 需要在init方法之前调用
 该接口设置只在DEBUG打包模式下生效，能控制log是否打印
 RELEASE打包模式下sdk不会打印log，该接口无效
 
 @param showDebugLog 是否显示调试log，默认YES
 */
+ (void)setShowDebugLog:(BOOL)showDebugLog;
@end


