//
//  QiLinTransSDK_IOS.h
//  QiLinTransSDK-IOS
//
//  Copyright © 2019 iqiyi. All rights reserved.
//

#import <Foundation/Foundation.h>

//! Project version number for QiLinTransSDK_IOS.
FOUNDATION_EXPORT double QiLinTransSDK_IOSVersionNumber;

//! Project version string for QiLinTransSDK_IOS.
FOUNDATION_EXPORT const unsigned char QiLinTransSDK_IOSVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <QiLinTransSDK_IOS/PublicHeader.h>

// 预定义事件
extern const NSString * _Nonnull const QL_REGISTER;
extern const NSString * _Nonnull const QL_LOGIN;
extern const NSString * _Nonnull const QL_LOGOUT;
extern const NSString * _Nonnull const QL_CONSULT;
extern const NSString * _Nonnull const QL_SHARE;
extern const NSString * _Nonnull const QL_SEARCH;
extern const NSString * _Nonnull const QL_COLLECT;
extern const NSString * _Nonnull const QL_SCORE;
extern const NSString * _Nonnull const QL_CREATE_ROLE;
extern const NSString * _Nonnull const QL_UPGRADE;
extern const NSString * _Nonnull const QL_PURCHASE;
extern const NSString * _Nonnull const QL_SHOPPING_CART;
extern const NSString * _Nonnull const QL_PLACE_ORDER;
extern const NSString * _Nonnull const QL_APPLY;
extern const NSString * _Nonnull const QL_GRANT;
extern const NSString * _Nonnull const QL_VISIT;

extern const NSString * _Nonnull const QL_PARAM_KEY_ACCOUNT;
extern const NSString * _Nonnull const QL_PARAM_KEY_DESTINATION;
extern const NSString * _Nonnull const QL_PARAM_KEY_CONTENT;
extern const NSString * _Nonnull const QL_PARAM_KEY_SOURCEID;
extern const NSString * _Nonnull const QL_PARAM_KEY_SCORE;
extern const NSString * _Nonnull const QL_PARAM_KEY_ROLENAME;
extern const NSString * _Nonnull const QL_PARAM_KEY_ROLELEVEL;
extern const NSString * _Nonnull const QL_PARAM_KEY_GOODSID;
extern const NSString * _Nonnull const QL_PARAM_KEY_BILLID;
extern const NSString * _Nonnull const QL_PARAM_KEY_APPLYTYPE;
extern const NSString * _Nonnull const QL_PARAM_KEY_MONEY;
extern const NSString * _Nonnull const QL_PARAM_KEY_COLDSTART;
extern const NSString * _Nonnull const QL_PARAM_KEY_SOURCE_NAME;
extern const NSString * _Nonnull const QL_PARAM_KEY_STATUS;
extern const NSString * _Nonnull const QL_PARAM_KEY_EXTRA;
extern const NSString * _Nonnull const QL_PARAM_KEY_DUR_INTERVAL;
extern const NSString * _Nonnull const QL_PARAM_KEY_DUR_TOTAL;

#define LOG_NONE      0
#define LOG_ERROR     3
#define LOG_INFO      2
#define LOG_DEBUG     1

@interface QiLinTrans : NSObject
+ (void) startsWithAppid:(NSString * _Nonnull)appid andAppStoreId:(NSString * _Nullable)storeId;
+ (BOOL) uploadTrans:(NSString * _Nonnull)action withParams:(NSDictionary * _Nullable)params;
+ (void) logLevel:(int)level isWriteToFile:(BOOL)toFile withPath:(NSString * _Nullable)logPath;
+ (void) onDestroy;
+ (void) onResume;
+ (NSString * _Nullable) getSdkVersion;
@end


