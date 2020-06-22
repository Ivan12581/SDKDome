//
//  PayInApple.m
//  Unity-iPhone
//
//  Created by mini on 6/18/20.
//
#import "UnityAppController.h"
#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>
#import <Foundation/Foundation.h>

@interface PayInApple : UnityAppController<SKProductsRequestDelegate,SKPaymentTransactionObserver>
   
@end

//苹果服务器验证地址:
//测试：https://sandbox.itunes.apple.com/verifyReceipt
//已上线Appstore：https://buy.itunes.apple.com/verifyReceipt


@implementation PayInApple{
    NSString *goodID;   //商品ID
    NSInteger *goodNum; //商品数量
}



//收到产品返回信息
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response NS_AVAILABLE(10_7, 3_0) {
    NSArray *myProduct = response.products;

    if([myProduct count] == 0){
        NSLog(@"查找不到商品信息");
        return;
    }
   SKProduct *requestProduct = nil;
    for (SKProduct *product in myProduct) {
        NSLog(@"product info");
        NSLog(@"  基本描述: %@", [product description]);
        NSLog(@"  IAP的id: %@", product.productIdentifier);
        NSLog(@"  地区编码: %@", product.priceLocale.localeIdentifier);
        NSLog(@"  本地价格: %@", product.price);
        NSLog(@"  语言代码: %@", [product.priceLocale objectForKey:NSLocaleLanguageCode]);
        NSLog(@"  国家代码: %@", [product.priceLocale objectForKey:NSLocaleCountryCode]);
        NSLog(@"  货币代码: %@", [product.priceLocale objectForKey:NSLocaleCurrencyCode]);
        NSLog(@"  货币符号: %@", [product.priceLocale objectForKey:NSLocaleCurrencySymbol]);
        NSLog(@"  本地标题: %@", product.localizedTitle);
        NSLog(@"  本地描述: %@", product.localizedDescription);
        
        //如果后台消费条目的ID与我这里需要请求的一样（用于确保订单的正确性）
        if([product.productIdentifier isEqualToString:goodID]){
            requestProduct = product;
//                break;
        }

    }
    //发送购买请求
    NSLog(@"发送购买请求");
    SKMutablePayment *payment = [SKMutablePayment paymentWithProduct:requestProduct];

//    @property(nonatomic, readonly) NSInteger quantity;
//    The default value is 1, the minimum value is 1, and the maximum value is 10.
    payment.quantity = goodNum;
        //可记录一个字符串，用于帮助苹果检测不规则支付活动 可以是userId，也可以是订单id，跟你自己需要而定
        //payment.applicationUsername = userId;
    [[SKPaymentQueue defaultQueue] addPayment:payment];

}

#pragma mark - SKRequestDelegate
//请求失败
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error {
    // 购买失败
    NSLog(@"error:%@", error);
}
//请求结束
- (void)requestDidFinish:(SKRequest *)request
{
    NSLog(@"请求结束");
}


#pragma mark - SKPaymentTransactionObserver
//监听购买结果
 //监听SKPaymentTransactionObserver处理购买结果
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray<SKPaymentTransaction *> *)transactions {
    for (SKPaymentTransaction *tran in transactions) {
        switch (tran.transactionState) {
            case SKPaymentTransactionStatePurchased: // 交易完成
                // 发送自己服务器验证凭证
                NSLog(@"交易完成");
                [self completeTransaction:tran];
                //    等服务器验证完后再finish 这样每次启动app这个接口会再调用
//                [[SKPaymentQueue defaultQueue]finishTransaction:tran];
                break;
            case SKPaymentTransactionStatePurchasing: // 购买中
                NSLog(@"商品添加进列表");
                break;
            case SKPaymentTransactionStateRestored: // 购买过 消耗型商品不用写
                NSLog(@"已经购买过商品");
                // 恢复逻辑
                //[[SKPaymentQueue defaultQueue] finishTransaction:tran];
                break;
            case SKPaymentTransactionStateFailed: // 交易失败
                 NSLog(@"交易失败");
                [[SKPaymentQueue defaultQueue]finishTransaction:tran];
                break;
            default:
                break;
        }
    }
}

//交易结束,验证支付信息是否都正确。
- (void)completeTransaction:(SKPaymentTransaction *)transaction {
    // 验证凭据，获取到苹果返回的交易凭据
    // appStoreReceiptURL iOS7.0增加的，购买交易完成后，会将凭据存放在该地址
    NSURL *receiptURL = [[NSBundle mainBundle] appStoreReceiptURL];
    // 从沙盒中获取到购买凭据
    NSData *receiptData = [NSData dataWithContentsOfURL:receiptURL];
    /**
        BASE64 常用的编码方案，通常用于数据传输，以及加密算法的基础算法，传输过程中能够保证数据传输的稳定性
        BASE64是可以编码和解码的
        关于验证：https:blog.csdn.net/qq_22080737/article/details/79786500?utm_medium=distribute.pc_relevant.none-task-blog-baidujs-2
    */
    NSString *encodeStr = [receiptData base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
    
    //发给自己服务器
//    [self sendMineServer:encodeStr];
//    [self sendMineServer:transaction];

}


//In-App Purchase入口
- (void)buyProductsWithId:(NSString *)productsId andQuantity:(NSInteger)quantity {
    goodNum = quantity;

    [self addListener];
    //允许程序内付费购买
    if ([SKPaymentQueue canMakePayments]) {
        //请求对应的产品信息
        NSArray *productArr = [[NSArray alloc] initWithObjects:productsId,nil];
        NSSet *nsset = [NSSet setWithArray:productArr];
        SKProductsRequest *request = [[SKProductsRequest alloc] initWithProductIdentifiers:nsset];
        request.delegate = self;
        [request start];
    } else {
        //您的手机没有打开程序内付费购买
        NSLog(@"用户不允许内购");
    }
}

-(void)Init{
    
}
#pragma mark -- 支付
-(void)Pay: (const char *) jsonString{
        NSLog(@"---支付---");
}

//监听购买结果
-(void)addListener{
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
}
//移除监听购买结果
-(void)delListener{
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
}


@end

IMPL_APP_CONTROLLER_SUBCLASS (PayInApple)

extern "C"
{

    void cPay(const char* jsonString){
        [(PayInApple*)[UIApplication sharedApplication].delegate Pay:jsonString];
    }

}

