//
//  AdBrix.h
//  AdBrixLib
//
//  Created by wonje,song on 2015. 5. 21..
//  Copyright (c) 2015년 wonje,song. All rights reserved.
//

#import <Foundation/Foundation.h>

#import <UIKit/UIKit.h>

#import "AdBrixItem.h"
#import "AdBrixCommerceProductModel.h"
#import "AdBrixCommerceProductCategoryModel.h"
#import "AdBrixCommerceProductAttrModel.h"


typedef NS_ENUM(NSInteger, AdBrixCustomCohortType)
{
    AdBrixCustomCohort_1 = 1,
    AdBrixCustomCohort_2 = 2,
    AdBrixCustomCohort_3 = 3
};

typedef NS_ENUM(NSInteger, AdBrixCurrencyType)
{
    AdBrixCurrencyKRW = 1,
    AdBrixCurrencyUSD = 2,
    AdBrixCurrencyJPY = 3,
    AdBrixCurrencyEUR = 4,
    AdBrixCurrencyGBP = 5,
    AdBrixCurrencyCNY = 6,
    AdBrixCurrencyTWD = 7,
    AdBrixCurrencyHKD = 8,
    AdBrixCurrencyIDR = 9,
    AdBrixCurrencyINR = 10,
    AdBrixCurrencyRUB = 11,
    AdBrixCurrencyTHB = 12,
    AdBrixCurrencyVND = 13,
    AdBrixCurrencyMYR = 14
};

typedef NS_ENUM(NSInteger, AdbrixPaymentMethod)
{
    AdBrixPaymentCreditCard         = 1,
    AdBrixPaymentBankTransfer       = 2,
    AdBrixPaymentMobilePayment      = 3,
    AdBrixPaymentETC                = 4
};

typedef NS_ENUM(NSInteger, AdBrixSharingChannel)
{
    AdBrixSharingFacebook       = 1,
    AdBrixSharingKakaoTalk      = 2,
    AdBrixSharingKakaoStory     = 3,
    AdBrixSharingLine           = 4,
    AdBrixSharingWhatsApp       = 5,
    AdBrixSharingQQ             = 6,
    AdBrixSharingWeChat         = 7,
    AdBrixSharingSMS            = 8,
    AdBrixSharingEmail          = 9,
    AdBrixSharingCopyUrl        = 10,
    AdBrixSharingETC            = 11
};




@interface AdBrix : NSObject

/*!
 @abstract
 singleton AdBrix 객체를 반환한다.
 */
+ (AdBrix *)shared;

/*!
 @abstract
 first time experience의 Activity에 해당할때 호출한다.
 
 @param activityName              activity name.
 */
+ (void)firstTimeExperience:(NSString *)activityName;


/*!
 @abstract
 first time experience의 Activity에 해당할때 호출한다.
 
 @param activityName              activity name.
 @param param                     parameter.
 */
+ (void)firstTimeExperience:(NSString *)activityName param:(NSString *)param;

/*!
 @abstract
 retension의 Activity에 해당할때 호출한다.
 
 @param activityName              activity name.
 */
+ (void)retention:(NSString *)activityName;

/*!
 @abstract
 retension의 Activity에 해당할때 호출한다.
 
 @param activityName              activity name.
 @param param                     parameter.
 */
+ (void)retention:(NSString *)activityName param:(NSString *)param;

/*!
 @abstract
 buy의 Activity에 해당할때 호출한다.
 
 @param activityName              activity name.
 */
+ (void)buy:(NSString *)activityName  __attribute__((deprecated("use -purchase: instead")));

/*!
 @abstract
 buy의 Activity에 해당할때 호출한다.
 
 @param activityName              activity name.
 @param param                     parameter.
 */
+ (void)buy:(NSString *)activityName param:(NSString *)param  __attribute__((deprecated("use -purchase: instead")));


+ (void)showViralCPINotice:(UIViewController *)viewController;

/*!
 @abstract
 cohort 분석시 호출한다.
 
 @param customCohortType          cohort type : AdBrixCustomCohortType
 @param filterName                filter Name
 */
+ (void)setCustomCohort:(AdBrixCustomCohortType)customCohortType filterName:(NSString *)filterName;

/*!
 @abstract
 activity 바로 전송
 */
+ (void)flush;


#pragma mark - Commerce

+ (void)purchase:(NSString*)orderId productId:(NSString*)productId productName:(NSString*)productName price:(double)price quantity:(NSUInteger)quantity currencyString:(NSString *)currencyString category:(NSString*)categories __attribute__((deprecated("use -commercePurchase: instead")));

+ (void)purchaseList:(NSArray*)orderInfo __attribute__((deprecated("use -commercePurchase: instead")));

+ (void)purchase:(NSString*)purchaseDataJsonString __attribute__((deprecated("use -commercePurchase: instead")));
/*!
 @abstract
 currency를 정의한다
 @abstract
 :+:defined const:+:
 @abstract
 AdBrixCurrencyKRW,
 AdBrixCurrencyUSD,
 AdBrixCurrencyJPY,
 AdBrixCurrencyEUR,
 AdBrixCurrencyGBP,
 AdBrixCurrencyCNY,
 AdBrixCurrencyTWD,
 AdBrixCurrencyHKD,
 AdBrixCurrencyIDR,
 AdBrixCurrencyINR,
 AdBrixCurrencyRUB,
 AdBrixCurrencyTHB,
 AdBrixCurrencyVND,
 AdBrixCurrencyMYR
 
 @param integer:currency       value of currency (mandatory) code ex) [AdBrix currencyName:AdBrixCurrencyKRW]
 */
+ (NSString *)currencyName:(NSUInteger)currency;

/*!
 @abstract
 payment method를 정의한다
 @abstract
 :+:defined const:+:
 @abstract
 AdBrixPaymentCreditCard,
 AdBrixPaymentBankTransfer,
 AdBrixPaymentMobilePayment,
 AdBrixPaymentETC
 
 @param integer:method       value of payment method (mandatory) code ex) [AdBrix paymentMethod:AdBrixPaymentCreditCard]
 */
+ (NSString *)paymentMethod:(NSUInteger)method;

/*!
 @abstract
 sharing channel을 정의한다
 @abstract
 :+:defined const:+:
 @abstract
 AdBrixSharingFacebook,
 AdBrixSharingKakaoTalk,
 AdBrixSharingKakaoStory,
 AdBrixSharingLine,
 AdBrixSharingWhatsApp,
 AdBrixSharingQQ,
 AdBrixSharingWeChat,
 AdBrixSharingSMS,
 AdBrixSharingEmail,
 AdBrixSharingCopyUrl,
 AdBrixSharingETC
 
 @param integer:channel       value of sharing channel (mandatory)
 */
+ (NSString *)sharingChannel:(NSUInteger)channel;

+ (AdBrixItem*)createItemModel :(NSString*)orderId productId:(NSString*)productId productName:(NSString*)productName price:(double)price quantity:(NSUInteger)quantity currencyString:(NSString *)currencyString category:(NSString*)categories __attribute__((deprecated("use -createCommerceProductModel with commercePurchase api: instead")));
    
+ (AdBrixItem*)PurchaseItemModel :(NSString*)orderId productId:(NSString*)productId productName:(NSString*)productName price:(double)price quantity:(NSUInteger)quantity currencyString:(NSString *)currencyString category:(NSString*)categories __attribute__((deprecated("use -createCommerceProductModel with commercePurchase api: instead")));

#pragma mark - CommerceV2
/*!
 @abstract
 purchase의 CommerceProductMedel을 생성한다.
 
 @param string:productId            productId(mandatory)
 @param string:productName          product name(mandatory)
 @param double:price                value of price(mandatory)
 @param double:discount             value of discount(mandatory)
 @param int:quantity                value of quantity(mandatory)
 @param string:currencyString       currency(optional)
 @param object:categories           AdBrixCommerceProductCategoryModel object.(opnational)
 @param dictionary:atrrData         extra attrbute data(optional)
 */
+ (AdBrixCommerceProductModel*)createCommerceProductModel :(NSString*)productId productName:(NSString*)productName price:(double)price discount:(double)discount quantity:(NSUInteger)quantity currencyString:(NSString *)currencyString category:(AdBrixCommerceProductCategoryModel*)categories extraAttrsMap:(AdBrixCommerceProductAttrModel *)extraAttrs;

/*!
 @abstract
 purchase(not commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:productId            value of productId (mandatory)
 @param double:price                value of price(mandatory)
 @param string:currency             value of (optional) code ex) [AdBrix currencyName:AdBrixCurrencyKRW] or any NSString followed by ISO 4217 Currency Codes
 @param string:paymentMethod        value of paymentMethod(optional)
 @param double:price                value of products price(mandatory)
 */
+ (void)purchase:(NSString *)prodictId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod;
//__attribute__((deprecated("use + (void)purchase:(NSString *)prodictId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData - instead")));
/*!
 @abstract
 purchase(not commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param model:product               model of product(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 */
+ (void)purchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product paymentMethod:(NSString *)paymentMethod;
//__attribute__((deprecated("use + (void)purchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 purchase(not commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param array:products              array of products model(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 */
+ (void)purchase:(NSString *)orderId productsInfos:(NSArray *)productsInfos paymentMethod:(NSString *)paymentMethod;
//__attribute__((deprecated("use + (void)purchase:(NSString *)orderId productsInfos:(NSArray *)productsInfos paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 purchase(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:prodictId            value of sharing channel (mandatory)
 @param double:price                value of price(mandatory)
 @param string:currency             value of currency (optional) code ex) [AdBrix currencyName:AdBrixCurrencyKRW] or any NSString followed by ISO 4217 Currency Codes
 @param string:paymentMethod        value of paymentMethod(optional)
 */
+ (void)commercePurchase:(NSString *)productId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod;

//__attribute__((deprecated("use + (void)commercePurchase:(NSString *)prodictId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod attrData: (NSDictionary *)attrData - instead")));
/*!
 @abstract
 purchase(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              orderId(mandatory)
 @param model:product               model of product(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivery charge(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 */
+ (void)commercePurchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod;
//__attribute__((deprecated("use + (void)commercePurchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData - instead")));
/*!
 @abstract
 purchase(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              orderId(mandatory)
 @param array:product               array of products model(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivery charge(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 */
+ (void)commercePurchase:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod;
//__attribute__((deprecated("use + (void)commercePurchase:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData - instead")));
/*!
 @abstract
 deeplink open(commerce)의 Activity에 해당할때 호출한다.
 
 @param string:deeplinkUrl          deeplinkUrl(mandatory)
 */
+ (void)commerceDeeplinkOpen : (NSString *)deeplinkUrl;
//__attribute__((deprecated("use + (void)commerceDeeplinkOpen : (NSString *)deeplinkUrl atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 login(commerce)의 Activity에 해당할때 호출한다. 이 셀렉터를 통하여[IgaworksCore setUserId:[userId]]가 호출.
 
 @param string:userId               userId(mandatory)
 */
+ (void)commerceLogin : (NSString *)userId;
//__attribute__((deprecated("use + (void)commerceLogin : (NSString *)userId atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 refund(commerce)의 Activity에 해당할때 호출한다.
 
 @param string:orderId              orderId(mandatory)
 @param model:product               model of product(mandatory)
 @param string:penaltyCharge        value of penalty charge(mandatory)
 */+ (void)commerceRefund:(NSString *)orderId product:(AdBrixCommerceProductModel *)product penaltyCharge:(double)penaltyCharge;
//__attribute__((deprecated("use + (void)commerceLogin : (NSString *)userId atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 refund(not commerce)의 Activity에 해당할때 호출한다.
 
 @param string:orderId              orderId(mandatory)
 @param array:product               array of products model(mandatory)
 @param string:penaltyCharge        value of penalty charge(mandatory)
 */+ (void)commerceRefundBulk:(NSString *)orderId productsInfos:(NSArray *)productsInfos penaltyCharge:(double)penaltyCharge;
//__attribute__((deprecated("use + (void)commerceRefundBulk:(NSString *)orderId productsInfos:(NSArray *)productsInfos penaltyCharge:(double)penaltyCharge atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 add to cart(commerce)의 Activity에 해당할때 호출한다.
 
 @param model:product               model of product(mandatory)
 */
+ (void)commerceAddToCart:(AdBrixCommerceProductModel *)product;
//__attribute__((deprecated("use + (void)commerceAddToCart:(AdBrixCommerceProductModel *)product atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 add to cart(not commerce)의 Activity에 해당할때 호출한다.
 
 @param array:product               array of products model(mandatory)
 */
+ (void)commerceAddToCartBulk:(NSArray *)productsInfos;
//__attribute__((deprecated("use + (void)commerceAddToCartBulk:(NSArray *)productsInfos atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 add to wish list(commerce)의 Activity에 해당할때 호출한다.
 
 @param model:product               model of product(mandatory)
 */
+ (void)commerceAddToWishList:(AdBrixCommerceProductModel *)product;
//__attribute__((deprecated("use + (void)commerceAddToWishList:(AdBrixCommerceProductModel *)product atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 product view(commerce)의 Activity에 해당할때 호출한다.
 
 @param model:product               model of product(mandatory)
 */
+ (void)commerceProductView:(AdBrixCommerceProductModel*)product;
//__attribute__((deprecated("use + (void)commerceProductView:(AdBrixCommerceProductModel*)product atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 category view(commerce)의 Activity에 해당할때 호출한다.
 
 @param model:category              model of category(mandatory)
 */
+ (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category;
//__attribute__((deprecated("use + (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 category view(commerce)의 Activity에 해당할때 호출한다.
 
 @param model:category              model of category(mandatory)
 @param array:product               array of products model(mandatory)
 */
+ (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category productsInfos:(NSArray *)productsInfos;
//__attribute__((deprecated("use + (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category productsInfos:(NSArray *)productsInfos atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 review order(commerce)의 Activity에 해당할때 호출한다.
 
 @param string:orderId              orderId(mandatory)
 @param model:product               model of product(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivety charge(mandatory)
 */
+ (void)commerceReviewOrder:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge;
//__attribute__((deprecated("use + (void)commerceReviewOrder:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 review order(commerce)의 Activity에 해당할때 호출한다.
 
 @param string:orderId              orderId(mandatory)
 @param array:product               array of products model(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivety charge(mandatory)
 */
+ (void)commerceReviewOrderBulk:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge;
//__attribute__((deprecated("use + (void)commerceReviewOrderBulk:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 payment view(commerce)의 Activity에 해당할때 호출한다.
 
 @param string:orderId              orderId(mandatory)
 @param array:product               array of products model(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivety charge(mandatory)
 */
+ (void)commercePaymentView:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge;
//__attribute__((deprecated("use + (void)commercePaymentView:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 search(commerce)의 Activity에 해당할때 호출한다.
 
 @param array:product               array of products model(mandatory)
 @param string:keyword              value of keyword(mandatory)
 */
+ (void)commerceSearch:(NSArray *)productsInfos keyword:(NSString *) keyword;
//__attribute__((deprecated("use + (void)commerceSearch:(NSArray *)productsInfos keyword:(NSString *) keyword atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 share(commerce)의 Activity에 해당할때 호출한다.
 
 @param string:channel              value of channel(mandatory)
 @param array:product               array of products model(mandatory)
 */
+ (void)commerceShare:(NSString*)channel product:(AdBrixCommerceProductModel *)product;
//__attribute__((deprecated("use + (void)commerceShare:(NSString*)channel product:(AdBrixCommerceProductModel *)product atrrData: (NSDictionary *)attrData - instead")));

/*!
 @abstract
 view home(commerce)의 Activity에 해당할때 호출한다.
 */
+ (void)commerceViewHome;

#pragma mark - Overloaded CommerceV2

+ (void)purchase:(NSString *)prodictId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 purchase(not commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:productId            value of productId (mandatory)
 @param double:price                value of price(mandatory)
 @param string:currency             value of (optional) code ex) [AdBrix currencyName:AdBrixCurrencyKRW] or any NSString followed by ISO 4217 Currency Codes
 @param string:paymentMethod        value of paymentMethod(optional)
 @param double:price                value of product's price(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)purchase:(NSString *)productId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)purchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 purchase(not commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param model:product               model of product(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)purchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product paymentMethod:(NSString *)paymentMethod attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)purchase:(NSString *)orderId productsInfos:(NSArray *)productsInfos paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 purchase(not commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param array:product               array of product's model(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)purchase:(NSString *)orderId productsInfo:(NSArray *)productsInfo paymentMethod:(NSString *)paymentMethod attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commercePurchase:(NSString *)prodictId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData  __attribute__((deprecated("deprecated")));
/*!
 @abstract
 purchase(commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능.
 
 @param string:prodictId            value of sharing channel (mandatory)
 @param double:price                value of price(mandatory)
 @param string:currency             value of currency (optional) code ex) [AdBrix currencyName:AdBrixCurrencyKRW] or any NSString followed by ISO 4217 Currency Codes
 @param string:paymentMethod        value of paymentMethod(optional)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commercePurchase:(NSString *)productId price:(double)price currency:(NSString*)currency paymentMethod:(NSString *)paymentMethod attrData: (NSDictionary *)attrData  __attribute__((deprecated()));

+ (void)commercePurchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData __attribute__((deprecated("deprecated")));
/*!
 @abstract
 purchase(commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능.
 
 @param string:orderId              orderId(mandatory)
 @param model:product               model of product(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivery charge(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 */
+ (void)commercePurchase:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commercePurchase:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod atrrData: (NSDictionary *)attrData __attribute__((deprecated("deprecated")));
/*!
 @abstract
 purchase(commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능.
 
 @param string:orderId              orderId(mandatory)
 @param array:product               array of product's model(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivery charge(mandatory)
 @param string:paymentMethod        value of paymentMethod(optional)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commercePurchase:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge paymentMethod:(NSString *)paymentMethod attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceDeeplinkOpen : (NSString *)deeplinkUrl atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 deeplink open(commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능.
 
 @param string:deeplinkUrl          deeplinkUrl(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceDeeplinkOpen : (NSString *)deeplinkUrl attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceLogin : (NSString *)userId atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 login(commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능. 이 셀렉터를 통하여[IgaworksCore setUserId:[userId]]가 호출.
 
 @param string:userId               userId(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceLogin : (NSString *)userId attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceRefund:(NSString *)orderId product:(AdBrixCommerceProductModel *)product penaltyCharge:(double)penaltyCharge atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 refund(commerce)의 Activity에 해당할때 호출한다. optioanl 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param model:product               model of product(mandatory)
 @param string:penaltyCharge        value of penalty charge(optional)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceRefund:(NSString *)orderId product:(AdBrixCommerceProductModel *)product penaltyCharge:(double)penaltyCharge attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceRefundBulk:(NSString *)orderId productsInfos:(NSArray *)productsInfos penaltyCharge:(double)penaltyCharge atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 refund(not commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param array:product               array of product's model(mandatory)
 @param string:penaltyCharge        value of penalty charge(optional)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceRefundBulk:(NSString *)orderId productsInfo:(NSArray *)productsInfo penaltyCharge:(double)penaltyCharge attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceAddToCart:(AdBrixCommerceProductModel *)product atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 add to cart(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param model:product               model of product(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceAddToCart:(AdBrixCommerceProductModel *)product attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceAddToCartBulk:(NSArray *)productsInfos atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 add to cart(not commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param array:product               array of product's model(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceAddToCartBulk:(NSArray *)productsInfo attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceAddToWishList:(AdBrixCommerceProductModel *)product atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 add to wish list(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param model:product               model of product(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceAddToWishList:(AdBrixCommerceProductModel *)product attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceProductView:(AdBrixCommerceProductModel*)product atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 product view(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param model:product               model of product(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceProductView:(AdBrixCommerceProductModel*)product attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 category view(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param model:category              model of category(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category productsInfos:(NSArray *)productsInfos atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 category view(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param model:category              model of category(mandatory)
 @param array:product               array of product's model(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceCategoryView:(AdBrixCommerceProductCategoryModel*)category productsInfo:(NSArray *)productsInfo attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceReviewOrder:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 review order(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param model:product               model of product(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivety charge(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceReviewOrder:(NSString *)orderId product:(AdBrixCommerceProductModel *)product discount:(double)discount deliveryCharge:(double)deliveryCharge attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceReviewOrderBulk:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 review order(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param array:product               array of product's model(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivety charge(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceReviewOrderBulk:(NSString *)orderId productsInfo:(NSArray *)productsInfo discount:(double)discount deliveryCharge:(double)deliveryCharge attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commercePaymentView:(NSString *)orderId productsInfos:(NSArray *)productsInfos discount:(double)discount deliveryCharge:(double)deliveryCharge atrrData: (NSDictionary *)attrData  __attribute__((deprecated()));
/*!
 @abstract
 payment view(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:orderId              order Id(mandatory)
 @param array:product               array of product's model(mandatory)
 @param double:discount             value of discount(mandatory)
 @param double:deliveryCharge       value of delivety charge(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commercePaymentView:(NSString *)orderId productsInfo:(NSArray *)productsInfo discount:(double)discount deliveryCharge:(double)deliveryCharge attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceSearch:(NSArray *)productsInfos keyword:(NSString *) keyword atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 search(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param array:product               array of product's model(mandatory)
 @param string:keyword              value of keyword(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceSearch:(NSArray *)productsInfo keyword:(NSString *) keyword attrData: (NSDictionary *)attrData __attribute__((deprecated()));

+ (void)commerceShare:(NSString*)channel product:(AdBrixCommerceProductModel *)product atrrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 share(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param string:channel              value of channel(mandatory)
 @param array:product               array of product's model(mandatory)
 @param dictionary:attrData         value of attrData(optional)
 */
+ (void)commerceShare:(NSString*)channel product:(AdBrixCommerceProductModel *)product attrData: (NSDictionary *)attrData __attribute__((deprecated()));
/*!
 @abstract
 view home(commerce)의 Activity에 해당할때 호출한다. optional 파라미터는 nil로 대체 가능.
 
 @param dictionary:attrData         value of attrData(optional)
 */
//+ (void)commerceViewHome : (NSDictionary *)attrData;
@end
