//
//  ApplePurchase.m
//  Unity-iPhone
//
//  Created by mini on 7/21/20.
//

#import "ApplePurchase.h"

@implementation ApplePurchase{
    NSString *curServiceName;   //保存的名称集
    NSString *goodID;   //商品ID
    NSInteger goodNum; //商品数量
    NSInteger  payModel;//支付类型
    SKPaymentTransaction *order; //所有订单
    NSArray *trans; //当前订单
    NSString *extra;   //透传字段
}
static ApplePurchase *ApplePurchaseIns = nil;
+(ApplePurchase*)sharedInstance{
    if (ApplePurchaseIns == nil) {
        if (self == [ApplePurchase class]) {
            ApplePurchaseIns = [[self alloc] init];
        }
    }
    return ApplePurchaseIns;
}
-(void)setDelegate:(id<cDelegate>)delegate{
    self.CbDelegate = delegate;
}
//这里定义支付类型 0为初始化 打开支付监听 1去Apple为支付 2为删除订单
typedef NS_ENUM(NSInteger, PayType)
{
    cInit,
    cPay,
    cDelOrder,
};

typedef NS_ENUM(NSInteger, PayState)
{
    Success,
    Fail,
    Cancel,
    NotFound,
    NotAllow,
};
//开启监听购买端口
-(void)InitApplePay{
    curServiceName = @"com.elex.girlsthrone.tw";
        //允许程序内付费购买
    if ([SKPaymentQueue canMakePayments]) {
         [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
//         [self CommonCallback:cInit AndPayState:Success];
    } else {
         NSLog(@"用户不允许内购");
        [self CommonCallback:cInit AndPayState:NotAllow];
    }
}
#pragma mark -- Apple In-App Purchase 入口
-(void)Pay: (const char *) jsonString{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    NSLog(@" ---ios pay---: %@", dict);
    payModel = [[dict valueForKey:@"PayType"] integerValue];
    switch (payModel) {
        case cInit:
             [self InitApplePay];
            break;
        case cPay:
              [self buyIAP:dict];
                break;
        case cDelOrder:
//              [self CheckTrans:dict];
        default:

            break;
    }

}
-(void)PurchaseCallback:(NSMutableDictionary *) dict{
    [dict setValue: [NSNumber numberWithInt:(int)payModel] forKey: @"PayType"];
    [dict setValue: [NSNumber numberWithInt:(int)Success] forKey: @"PayState"];
    [self.CbDelegate PayCallBack:dict];
}
//
-(void)CommonCallback:(int)PayType AndPayState:(int)PayState{
    NSNumber *type = [NSNumber numberWithInt:PayType];
    NSNumber *state = [NSNumber numberWithInt:PayState];
    [self.CbDelegate PayCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",type,@"PayType",state,@"PayState",nil]];
}
-(void)DelCallback:(int)PayState AndOrder:(NSString *)ordre{
    NSNumber *type = [NSNumber numberWithInt:(int)payModel];
    NSNumber *state = [NSNumber numberWithInt:PayState];
    [self.CbDelegate PayCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",type,@"PayType",state,@"PayState",ordre,@"Order",nil]];
}
#pragma mark - In-App Purchase入口
- (void)buyIAP:(NSMutableDictionary *) dict{
    goodID = [dict valueForKey:@"GoodID"];
    goodNum = [[dict valueForKey:@"GoodNum"] integerValue];

     NSLog(@"--IOS--AppleHelper--buyIAP--goodID-->%@", goodID);
    //请求对应的产品信息
    NSArray *productArr = [[NSArray alloc] initWithObjects:goodID,nil];
    NSSet *nsset = [NSSet setWithArray:productArr];
    SKProductsRequest *request = [[SKProductsRequest alloc] initWithProductIdentifiers:nsset];
    request.delegate = self;
    //查询商品开始
    [request start];
}
//收到产品返回信息
- (void)productsRequest:(nonnull SKProductsRequest *)request didReceiveResponse:(nonnull SKProductsResponse *)response {
     NSArray *myProduct = response.products;
        if([myProduct count] == 0){
             [self CommonCallback:cPay AndPayState:NotFound];
            NSLog(@"查找不到商品信息");
            return;
        }
       SKProduct *requestProduct = nil;
        for (SKProduct *product in myProduct) {
            if([product.productIdentifier isEqualToString:goodID]){
                requestProduct = product;
                    break;
            }
        }
        if (requestProduct == nil) {
             NSLog(@"****requestProduct == nil*****");
             [self CommonCallback:cPay AndPayState:NotFound];
            return;
        }
        //发送购买请求
        NSLog(@"发送购买请求");
        SKMutablePayment *payment = [SKMutablePayment paymentWithProduct:requestProduct];
    //    The default value is 1, the minimum value is 1, and the maximum value is 10.
        payment.quantity = goodNum;
        //applicationUsername 可透传 但是不用apple账号登录会为空 故舍弃
//        payment.applicationUsername = Extra;
        [[SKPaymentQueue defaultQueue] addPayment:payment];
}
#pragma mark - SKRequestDelegate
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error {
    NSLog(@"购买请求失败error:%@", error);
}
- (void)requestDidFinish:(SKRequest *)request{
    NSLog(@"购买请求结束");
    //保存服务器订单号和UID
    [self saveExtraWithPID:goodID AndExtra:extra];
}
#pragma mark - 监听购买结果委托
- (void)paymentQueue:(nonnull SKPaymentQueue *)queue updatedTransactions:(nonnull NSArray<SKPaymentTransaction *> *)transactions {
        //缓存订单信息 为了送达服务器后后可以删除订单
        trans = transactions;
        int count = 0;
        for (SKPaymentTransaction *tran in transactions) {
            switch (tran.transactionState) {
                case SKPaymentTransactionStatePurchased: // 交易完成
                     NSLog(@"交易完成");
                    // 发送自己服务器验证凭证
                    count = count + 1;
                   [self HandleAppleOrder:tran];
                    break;
                case SKPaymentTransactionStatePurchasing: // 购买中
                    NSLog(@"购买中 ...... 商品已经添加进列表");
                    break;
                case SKPaymentTransactionStateRestored: // 购买过 消耗型商品不用写
                    NSLog(@"购买过 消耗型商品不用处理");
                    // 恢复逻辑
                    [[SKPaymentQueue defaultQueue] finishTransaction:tran];
                    break;
                case SKPaymentTransactionStateFailed: // 交易失败
                     NSLog(@"交易失败");
                    //apple删除订单
                    [[SKPaymentQueue defaultQueue]finishTransaction:tran];
                    //删除钥匙串中的存储
                    [self deleteExtraWithPID:tran.payment.productIdentifier];
                     //回调至游戏
                    [self CommonCallback:cPay AndPayState:Fail];
                    break;
                default:
                    break;
            }
        }
    //确保有成功的订单
    if (count > 0) {
            [self HandleReceipt];
        }
}
//预处理并统计Apple订单
-(void)HandleAppleOrder:(SKPaymentTransaction *)transaction{
     NSLog(@"--预处理并统计Apple订单---");
    //apple订单号
    NSString * transaction_id = transaction.transactionIdentifier;
    //商品PID
    NSString *product_id = transaction.payment.productIdentifier;
    
    NSString *extra = [self getExtraWithPID:product_id];
    if (extra) {
           [self PurchaseCallback:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",product_id,@"product_id",transaction_id,@"transaction_id",extra,@"Extra",nil]];
    }else{
         NSLog(@"--钥匙串中没有该PID的存储--%@",product_id);
    }


}
// 从沙盒中获取到购买凭据
-(void)HandleReceipt{
    NSLog(@"--HandleReceipt---");
    NSURL *receiptURL = [[NSBundle mainBundle] appStoreReceiptURL];
    NSData *receiptData = [NSData dataWithContentsOfURL:receiptURL];
    NSString *encodeStr = [receiptData base64EncodedStringWithOptions:0];
    [self PurchaseCallback:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",encodeStr,@"encodeStr",nil]];
    //发给自己服务器
}

//从服务器返回 通过订单号解除交易状态
-(void)CheckTrans:(NSMutableDictionary *) dict{
    NSString *needDelOrder = [dict valueForKey:(@"Order")];
    NSLog(@"---从服务器返回 通过订单号解除交易状态---: %@", needDelOrder);
    if(trans != nil && trans.count > 0){
        SKPaymentTransaction *curtran = nil;
        for (SKPaymentTransaction *tran in trans) {
            if ([tran.transactionIdentifier  isEqual: needDelOrder]) {
                curtran = tran;
                break;
            }
        }
        if (curtran != nil) {
              NSLog(@"---找到并删除钥匙串存储、删除Apple交易订单---");
            [self deleteExtraWithPID:curtran.payment.productIdentifier];
            [[SKPaymentQueue defaultQueue] finishTransaction:curtran];
            [self DelCallback:Success AndOrder:needDelOrder];

        }else{
             NSLog(@"---未找到该定单---");
            [self DelCallback:NotFound AndOrder:needDelOrder];

        }
    }else{
          [self DelCallback:Fail AndOrder:needDelOrder];
         NSLog(@"---Error:Apple Order is nil---");
    }

}


#pragma mark -取出
-(NSString *)getExtraWithPID:(NSString *)PID{
    NSString *password = nil;
    //读取
    if (![SSKeychain passwordForService:curServiceName account:PID]) {
        NSLog(@ "没有 getPasswordInkeychain--");
    }
    else{
        password = [SSKeychain passwordForService:curServiceName account:PID];
    }
    return password;
}
#pragma mark - 保存
-(void)saveExtraWithPID:(NSString *)PID AndExtra:(NSString *)Extra{
//    NSLog(@ "--saveUserInKeychain--->%@",userIdentifier);
    //写入
    [SSKeychain setPassword:Extra forService:curServiceName account:PID];
}
#pragma mark -删除
-(void)deleteExtraWithPID:(NSString *)PID{
    [SSKeychain deletePasswordForService:curServiceName account:PID];
//    NSError *error = nil;
//    [SSKeychain deletePasswordForService:accountName account:PID error:&error];
//    if ([error code] == SSKeychainErrorNone) {
//
//    }
    
}
@end
