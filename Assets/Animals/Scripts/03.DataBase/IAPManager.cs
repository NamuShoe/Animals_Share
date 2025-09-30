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

    // puppy: 해당 변수는 구매 성공 여부를 확인하는 변수입니다.
    public Status status = Status.Waiting;

    private void Awake()
    {
        instance = this;
    }
    
    private async void Start()
    {
        storeController = UnityIAPServices.StoreController();

        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        storeController.OnCheckEntitlement += OnCheckEntitlement;
        
        status = Status.Waiting;
        await storeController.Connect();

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
        
        // puppy: IAP 5.0의 예시 스크립트에서는 이곳에서 영수증 검증을 합니다. 무조건 이곳에 하실 필요는 없습니다.
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
    
    // puppy: 구매하는 함수입니다.
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
