using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class IAPManager : MonoBehaviour 
{
    public static IAPManager instance;
    
    private StoreController storeController;

    public Status status = Status.Waiting;

    private void Awake()
    {
        instance = this;
    }
    
    private async void Start()
    {
        storeController = UnityIAPServices.StoreController();

        // Listen to store events
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        storeController.OnCheckEntitlement += OnCheckEntitlement;
        
        status = Status.Waiting;
        await storeController.Connect();

        // Fetch your products
        FetchProducts();
    }

    private void OnCheckEntitlement(Entitlement entitlement)
    {
        if (entitlement.Product != null) {
            string productId = entitlement.Product.definition.id;

            if (entitlement.Status == EntitlementStatus.FullyEntitled) {
                if (entitlement.Product.definition.id == ShopType.NC_NoAds_NoAds_0.ToString().ToLower()) {
                    
                    // TODO : 관련 함수 구현
                }
                else if (entitlement.Product.definition.id == ShopType.S_Subscription_Membership_0.ToString().ToLower()) {
                    
                    // TODO : 관련 함수 구현
                }
                else if (entitlement.Product.definition.id == ShopType.S_Subscription_Membership_1.ToString().ToLower()) {
                    
                    // TODO : 관련 함수 구현
                }
            }
        }
    }

    public void CheckEntitlement(ShopType shopType)
    {
        storeController.CheckEntitlement(storeController.GetProductById(shopType.ToString().ToLower())!);
    }

    public void CheckEntitlement()
    {
        storeController.CheckEntitlement(storeController.GetProductById(ShopType.C_Package_Starter_0.ToString().ToLower()));
        storeController.CheckEntitlement(storeController.GetProductById(ShopType.S_Subscription_Membership_0.ToString().ToLower()));
        storeController.CheckEntitlement(storeController.GetProductById(ShopType.NC_NoAds_NoAds_0.ToString().ToLower()));
    }

    private void FetchProducts()
    {
        var products = new List<ProductDefinition>();

        foreach (var shopType in Enum.GetValues(typeof(ShopType))) {
            var shopTypeString = shopType.ToString().ToLower();
            
            if(shopTypeString.StartsWith("c_"))
                products.Add(new ProductDefinition(shopTypeString, ProductType.Consumable));
            else if (shopTypeString.StartsWith("nc_"))
                products.Add(new ProductDefinition(shopTypeString, ProductType.NonConsumable));
            else if(shopTypeString.StartsWith("s_"))
                products.Add(new ProductDefinition(shopTypeString, ProductType.Subscription));
        }
        
        storeController.FetchProducts(products);
    }
    
    private void OnPurchasePending(PendingOrder order)
    {
        var product = order.CartOrdered.Items().First()?.Product;
        Debug.Log($"Pending purchase: {product.definition.id}");
        
        // Grant reward now if you want immediate effect
        // But for consumables, best practice is to wait until confirmed

        bool validPurchase = true;
        
        if (Application.platform == RuntimePlatform.Android) {
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);

            try {
                //영수증 검사
                /*서명 검증을 통해 영수증 유효성을 검사합니다.
                영수증의 애플리케이션 번들 식별자를 애플리케이션의 식별자와 비교합니다.
                이 둘이 일치하지 않으면 InvalidBundleId 예외 오류가 발생합니다.*/
                var result = validator.Validate(order.Info.Receipt);

                //영수증 내용 출력
                foreach (IPurchaseReceipt purchaseReceipt in result) {
                    Debug.Log(purchaseReceipt.productID);
                    Debug.Log(purchaseReceipt.purchaseDate);
                    Debug.Log(purchaseReceipt.transactionID);
                }
            }
            catch (IAPSecurityException) {
                Debug.Log("Invalid receipt");
                validPurchase = false;
            }
        }
        
        // Confirm purchase so the transaction is completed
        if(validPurchase)
            storeController.ConfirmPurchase(order);
        else
            Debug.Log("Failed Purchase");
    }

    private void OnPurchaseConfirmed(Order order)
    {
        var product = order.CartOrdered.Items().First()?.Product;
        Debug.Log($"Confirmed purchase: {product.definition.id}");
        
        status = Status.Success;
    }

    private void OnPurchaseFailed(FailedOrder order)
    {
        var product = order.CartOrdered.Items().First()?.Product;
        if(order.FailureReason != PurchaseFailureReason.UserCancelled)
            Debug.Log($"Purchase failed for {product?.definition.id}, reason: {order.FailureReason}");

        status = Status.Fail;
    }

    private void OnPurchaseDeferred(DeferredOrder order)
    {
        var product = order.CartOrdered.Items().First()?.Product;
        Debug.Log($"Deferred purchase: {product.definition.id}");
    }
    
    public void InitiatePurchase(string productId)
    {
        var product = storeController?.GetProducts().FirstOrDefault(product => product.definition.id == productId);

        if (product != null)
        {
            storeController?.PurchaseProduct(product);
            status = Status.Waiting;
        }
        else
        {
            Debug.Log($"The product service has no product with the ID {productId}");
        }
    }
    
    public void RestorePurchases()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // iOS and macOS require explicit restoration
            storeController.RestoreTransactions((result, error) =>
            {
                if (result)
                {
                    Debug.Log("Restore subscription succeeded");
                    // ProcessRestoredSubscriptions();
                }
                else
                {
                    Debug.LogError($"Restore subscription failed: {error}");
                }
            });
        }
        else
        {
            // Android handles restoration automatically
            Debug.Log("No restore needed on this platform");
            // ProcessRestoredSubscriptions();
        }
    }
}
