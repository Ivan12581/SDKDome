package celia.sdk;

import android.content.Intent;

import androidx.annotation.NonNull;

import com.android.billingclient.api.AcknowledgePurchaseParams;
import com.android.billingclient.api.AcknowledgePurchaseResponseListener;
import com.android.billingclient.api.BillingClient;
import com.android.billingclient.api.BillingClientStateListener;
import com.android.billingclient.api.BillingFlowParams;
import com.android.billingclient.api.BillingResult;
import com.android.billingclient.api.ConsumeParams;
import com.android.billingclient.api.ConsumeResponseListener;
import com.android.billingclient.api.Purchase;
import com.android.billingclient.api.PurchasesUpdatedListener;
import com.android.billingclient.api.SkuDetails;
import com.android.billingclient.api.SkuDetailsParams;
import com.android.billingclient.api.SkuDetailsResponseListener;
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInClient;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class GooglePay {
    PurchasesUpdatedListener purchaseUpdateListener;
    BillingClient billingClient;
    CeliaActivity mainActivity;
    private GoogleSignInClient mGoogleSignInClient;
    private static final int RC_GET_TOKEN = 9002;
    public GooglePay(CeliaActivity activity){

        mainActivity = activity;
        Init();
    }
    void Init(){
        GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
                .requestIdToken(Constant.google_server_client_id)
                .requestEmail()
                .build();
        mGoogleSignInClient = GoogleSignIn.getClient(mainActivity, gso);

    }
    public void Login(){
        Intent signInIntent = mGoogleSignInClient.getSignInIntent();
        mainActivity.startActivityForResult(signInIntent, RC_GET_TOKEN);
    }
    public void onActivityResult(int requestCode, int resultCode, Intent data){
        if (requestCode == RC_GET_TOKEN) {
            Task<GoogleSignInAccount> task = GoogleSignIn.getSignedInAccountFromIntent(data);
            try {
                GoogleSignInAccount account = task.getResult(ApiException.class);
                String token = account.getIdToken();
                String id = account.getId();
                mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                    {
                        put("state", "1");
                        put("uid", account.getId());
                        put("token",token);
                    }
                });

            } catch (ApiException e) {
                mainActivity.ShowLog("signInResult:failed code=" + e.getStatusCode());
                mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Login.getCode(), new HashMap<String, String>(){
                    {
                        put("state", "0");
                        put("message", e.getStatusCode() + "");
                    }
                });
            }
        }
    }
    public void Logout(){
        mGoogleSignInClient.signOut().addOnCompleteListener(mainActivity, new OnCompleteListener<Void>() {
            @Override
            public void onComplete(@NonNull Task<Void> task) {
                mainActivity.ShowLog("Google Logout onComplete");
            }
        });
    }
    void Connect()
    {
        purchaseUpdateListener = new PurchasesUpdatedListener() {
            @Override
            public void onPurchasesUpdated(BillingResult billingResult, List<Purchase> purchases) {
                OnUpdate(billingResult,purchases);
            }
        };
        billingClient = BillingClient.newBuilder(mainActivity)
                .setListener(purchaseUpdateListener)
                .enablePendingPurchases()
                .build();

        billingClient.startConnection(new BillingClientStateListener() {
            @Override
            public void onBillingSetupFinished(BillingResult billingResult) {
                if (billingResult.getResponseCode() ==  BillingClient.BillingResponseCode.OK) {
                    // The BillingClient is ready. You can query purchases here.
                    mainActivity.ShowLog("Google pay connected");
                    mainActivity.ShowLog("Google pay is ready?:" + billingClient.isReady());
                    if (ShowUnfinished())
                    {
                        CheckGoogleProduct();
                    }
                }
            }
            @Override
            public void onBillingServiceDisconnected()
            {
                mainActivity.ShowLog("Google pay connection lost");
            }
        });
    }

    boolean ShowUnfinished()
    {
        Purchase.PurchasesResult result = billingClient.queryPurchases(BillingClient.SkuType.INAPP);
        mainActivity.ShowLog("History code:" + result.getResponseCode());
		if(result.getPurchasesList()== null)
			return true;
        for (Purchase item : result.getPurchasesList()) {
            mainActivity.ShowLog("History purchase List:" + item.getOriginalJson());
            SendOrder(item,0);
        }
        if (result.getPurchasesList().size() == 0)
        {
            return true;
        }else
        {
            return false;
        }
    }

    String currentID;
    String currentOrderNo;
    public  void Purchase(String id,String orderNumber)
    {
        mainActivity.ShowLog("Tring to purchase :" + id);
        currentID = id;
        currentOrderNo = orderNumber;
        Connect();
    }

    void CheckGoogleProduct()
    {
        List<String> skuList = new ArrayList<>();
        skuList.add(currentID);
        SkuDetailsParams.Builder params = SkuDetailsParams.newBuilder();
        params.setSkusList(skuList).setType(BillingClient.SkuType.INAPP);
        billingClient.querySkuDetailsAsync(params.build(),
                new SkuDetailsResponseListener() {
                    @Override
                    public void onSkuDetailsResponse(BillingResult billingResult,
                                                     List<SkuDetails> skuDetailsList) {
                        // Process the result.
                        for (SkuDetails item: skuDetailsList) {
                            mainActivity.ShowLog("Sku detail:" + item.getTitle() + "-" + item.getPrice() + ":" + item.getOriginalJson());

                        }
                        mainActivity.ShowLog("sku result:" + billingResult.getResponseCode() + "-" + billingResult.getDebugMessage());

                        if (skuDetailsList.size()>0)
                        {
                            float priceStr = skuDetailsList.get(0).getPriceAmountMicros()/1000000;
                            Utils.getInstance().saveCacheData("price",priceStr+"");
                            Utils.getInstance().saveCacheData("CurrencyCode",skuDetailsList.get(0).getPriceCurrencyCode());
                            mainActivity.ShowLog("---saveCacheData---price-->" + priceStr + "---CurrencyCode--->" + skuDetailsList.get(0).getPriceCurrencyCode());
                            DoPurchase(skuDetailsList.get(0),currentOrderNo);
                        }else {
                            return;
                        }
                    }
                });
    }

    void DoPurchase(SkuDetails targetItem,String orderNumber)
    {
        BillingFlowParams billingFlowParams = BillingFlowParams.newBuilder()
                .setSkuDetails(targetItem)
                .setObfuscatedAccountId(orderNumber)
                .build();
        int responseCode = billingClient.launchBillingFlow(mainActivity, billingFlowParams).getResponseCode();
        if (responseCode != 0)
        {
            mainActivity.ShowLog("Failed to build the bill  code:" + responseCode);
        }
    }

    void OnUpdate(BillingResult billingResult, List<Purchase> purchases) {
        if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK
                && purchases != null) {
            for (Purchase purchase : purchases) {
                handlePurchase(purchase);
            }
        } else if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.USER_CANCELED) {
            // Handle an error caused by a user cancelling the purchase flow.
            mainActivity.ShowLog("purchase canceled");
        } else {
            // Handle any other error codes.
            mainActivity.ShowLog("purchase failed code:" + billingResult.getResponseCode());
        }
    }

    void handlePurchase(Purchase purchase)
    {
        mainActivity.ShowLog("Purcahse stste:" + purchase.getPurchaseState());
        if (purchase.getPurchaseState() == Purchase.PurchaseState.PURCHASED)
        {
            mainActivity.ShowLog( "Purchased a product:" + purchase.getOriginalJson());
            Acknowledge(purchase);
            SendOrder(purchase,1);
        }

    }

    //通知谷歌（不做3天后自动退款）
    void Acknowledge(Purchase purchase)
    {
        AcknowledgePurchaseResponseListener acknowledgePurchaseResponseListener = new AcknowledgePurchaseResponseListener()
        {
            @Override
            public  void onAcknowledgePurchaseResponse(BillingResult billingResult)
            {
                mainActivity.ShowLog("acknowledge finished");
            }
        };

        if (purchase.getPurchaseState() == Purchase.PurchaseState.PURCHASED) {
            if (!purchase.isAcknowledged()) {
                AcknowledgePurchaseParams acknowledgePurchaseParams =
                        AcknowledgePurchaseParams.newBuilder()
                                .setPurchaseToken(purchase.getPurchaseToken())
                                .build();
                mainActivity.ShowLog("acknowledging");
                billingClient.acknowledgePurchase(acknowledgePurchaseParams, acknowledgePurchaseResponseListener);
            }
        }
    }

    //消耗（不做该商品无法再次购买）
    public void Consume(String orderNumber)
    {
        Purchase purchase = orderCache.get(orderNumber);
        if (purchase == null)
        {
            return;
        }
        ConsumeParams consumeParams =
                ConsumeParams.newBuilder()
                        .setPurchaseToken(purchase.getPurchaseToken())
                        .build();

        ConsumeResponseListener listener = new ConsumeResponseListener() {
            @Override
            public void onConsumeResponse(BillingResult billingResult, String purchaseToken) {
                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                    // Handle the success of the consume operation.
                    mainActivity.ShowLog("consume finished : token:" + purchaseToken);
                    ReportConsumeFinish();
                }
            }
        };

        mainActivity.ShowLog("consuming");
        billingClient.consumeAsync(consumeParams, listener);
    }

    HashMap<String, Purchase> orderCache;

    //通知服务器校验
    public  void SendOrder(Purchase purchase,int isfinished)
    {
        if ( orderCache == null )
        {
            orderCache = new HashMap<String, Purchase>();
        }
        orderCache.put(purchase.getAccountIdentifiers().getObfuscatedAccountId(),purchase);
        mainActivity.ShowLog("purchase json : " + purchase.getOriginalJson());

        String price = Utils.getInstance().getCacheData("price");
        String CurrencyCode = Utils.getInstance().getCacheData("CurrencyCode");
        mainActivity.ShowLog("---getCacheData---price-->" + price + "---CurrencyCode--->" + CurrencyCode);
        //支付统计
        mainActivity.adjustHelper.OfficialPurchaseEvent(price,CurrencyCode,purchase.getOrderId());
        mainActivity.faceBookHelper.OfficialPurchaseEvent(price,CurrencyCode,purchase.getOrderId());

        mainActivity.SendMessageToUnity(CeliaActivity.MsgID.Pay.getCode(), new HashMap<String, String>(){
            {
                put("state",isfinished + "");
                put("sku",  purchase.getSku());
                put("token",purchase.getPurchaseToken());
                put("orderNumber",purchase.getAccountIdentifiers().getObfuscatedAccountId());
            } });
    }

    void ReportConsumeFinish()
    {
        mainActivity.SendMessageToUnity(CeliaActivity.MsgID.ConsumeGoogleOrder.getCode(), new HashMap<String, String>(){});
    }

}
