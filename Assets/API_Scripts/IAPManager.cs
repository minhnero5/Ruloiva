using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance { get; private set; }
    StoreController m_StoreController;
    private Dictionary<string, Action<int>> purchaseCallbacks = new();
    public static Action<Product> OnProductFetched;
    public bool canShowShop = false;
    public bool isProductsFetched = false;
    public bool IsCatalogReady { get; private set; } = false;

    private readonly List<string> defaultProductIds = new List<string>
    {
        "greatwallys_0.29","greatwallys_0.49", "greatwallys_0.99", "greatwallys_1.99", "greatwallys_2.99", "greatwallys_3.99",
        "greatwallys_4.99", "greatwallys_5.99", "greatwallys_7.99", "greatwallys_9.99", "greatwallys_19.99",
        "greatwallys_29.99", "greatwallys_49.99","greatwallys_99.99","greatwallys_15","greatwallys_16","greatwallys_17"
    };
    private List<string> runtimeProductIds = new List<string>();

    void Awake()
    {
        if (Instance != null) return;

        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(WaitForSkuAndInitializeIap());
    }

    private IEnumerator WaitForSkuAndInitializeIap()
    {
        if (ServerUrl.IsServerConfigured)
        {
            while (UserDataManager.Instance == null)
            {
                yield return null;
            }

            while (!UserDataManager.Instance.IsBootFetchSkusDone)
            {
                yield return null;
            }
        }

        ResolveRuntimeProductIds();
        IsCatalogReady = true;
        InitializeIAP();
    }

    private void ResolveRuntimeProductIds()
    {
        if (ServerUrl.IsServerConfigured && UserDataManager.Instance != null && UserDataManager.Instance.skuManager != null)
        {
            var serverProductIds = UserDataManager.Instance.skuManager
                .GetNormalizedActiveSkusForIap()
                .Select(item => item.id)
                .ToList();
            if (serverProductIds.Count > 0)
            {
                runtimeProductIds = serverProductIds;
                Debug.Log($"[IAP] Using {runtimeProductIds.Count} product IDs from server SKU config.");
                return;
            }

            Debug.LogWarning("[IAP] Server configured but no usable SKU IDs found. Falling back to default product IDs.");
        }

        runtimeProductIds = new List<string>(defaultProductIds);
        Debug.Log($"[IAP] Using default product IDs. Count: {runtimeProductIds.Count}");
    }

    public string GetProductId(int index)
    {
        if (index < 0 || index >= runtimeProductIds.Count)
        {
            Debug.LogError($"[IAP] Invalid product index: {index}");
            return null;
        }
        return runtimeProductIds[index];
    }

    public int GetProductCount()
    {
        return runtimeProductIds.Count;
    }

    public Product GetProductById(string productId)
    {
        if (m_StoreController == null || string.IsNullOrEmpty(productId))
        {
            return null;
        }
        return m_StoreController.GetProductById(productId);
    }

    async void InitializeIAP()
    {
        isProductsFetched = false;
        canShowShop = false;

        m_StoreController = UnityIAPServices.StoreController();

        m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
        m_StoreController.OnPurchasePending += OnPurchasePending;
        m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        m_StoreController.OnStoreDisconnected += OnStoreDisconnected;
        Debug.Log("Connecting to store.");

        await m_StoreController.Connect();
        m_StoreController.OnProductsFetchFailed += OnProductsFetchedFailed;
        m_StoreController.OnProductsFetched += OnProductsFetched;
        m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
        m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
        FetchProducts();

        m_StoreController.FetchPurchases();
    }

    public void BuyProduct(string productId, Action<int> onSuccess = null)
    {
        if (m_StoreController == null || string.IsNullOrWhiteSpace(productId))
        {
            Debug.LogWarning($"[IAP] Cannot buy product. Store not ready or invalid product ID: '{productId}'.");
            return;
        }
        if (purchaseCallbacks.ContainsKey(productId))
        {
            purchaseCallbacks[productId] = onSuccess;
        }
        else
        {
            purchaseCallbacks.Add(productId, onSuccess);
        }
        m_StoreController.PurchaseProduct(productId);
    }

    void FetchProducts()
    {
        List<ProductDefinition> initialProductsToFetch = new List<ProductDefinition>();
        foreach (var item in runtimeProductIds)
        {
            var a = new ProductDefinition(item, ProductType.Consumable);
            initialProductsToFetch.Add(a);
        }

        if (initialProductsToFetch.Count == 0)
        {
            Debug.LogWarning("[IAP] No product IDs available to fetch.");
            isProductsFetched = true;
            canShowShop = false;
            return;
        }

        m_StoreController.FetchProducts(initialProductsToFetch);
    }

    void OnPurchasePending(PendingOrder order)
    {
        var buyId = order.CartOrdered.Items().First()?.Product.definition.id;
        purchaseCallbacks.TryGetValue(buyId, out var callback);
        int quantity = order.CartOrdered.Items().First()?.Quantity ?? 1;
        if (callback != null)
        {
            callback.Invoke(quantity);

            // Debug.Log("PendingOrder.Info.Receipt: " + order.Info.Receipt);
            Debug.Log($"[IAP] Purchase success for product: {buyId}");
        }
        else
        {
            Debug.LogWarning($"[IAP] No callback found for pending purchase: {buyId}");
        }

        m_StoreController.ConfirmPurchase(order);

        try
        {
            UserDataManager.Instance.SetOrderInfo(order);
            UserDataManager.Instance.VerifyPurchaseInfo();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[IAP] Error processing purchase: {ex.Message}");
            // Có thể thêm xử lý retry hoặc thông báo người dùng
        }
    }

    void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"[IAP] Fetched {products.Count} products.");
        canShowShop = products.Any(p => p.availableToPurchase);
        isProductsFetched = true;
    }

    void OnPurchaseFailed(FailedOrder order)
    {

        var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
        var productId = product?.definition?.id;

        Debug.Log($"[IAP] Purchase failed - {productId} | Reason={order.FailureReason} | Details={order.Details}");

        // Detect "already owned"
        bool alreadyOwned = order.FailureReason == PurchaseFailureReason.DuplicateTransaction;

        if (!alreadyOwned && order.Details != null)
        {
            var d = order.Details.ToString();
            alreadyOwned =
                d.IndexOf("ITEM_ALREADY_OWNED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("BillingResponseCode: 7", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("already own", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        if (alreadyOwned)
        {
            m_StoreController.FetchPurchases();
        }
    }
    void OnPurchaseConfirmed(Order order)
    {
        switch (order)
        {
            case ConfirmedOrder confirmedOrder:
                OnPurchaseConfirmed(confirmedOrder);
                break;
            case FailedOrder failedOrder:
                OnPurchaseConfirmationFailed(failedOrder);
                break;
            default:
                Debug.Log("Unknown OnPurchaseConfirmed result.");
                break;
        }

    }
    void OnPurchaseConfirmed(ConfirmedOrder order)
    {
        var product = GetFirstProductInOrder(order);
        if (product == null)
        {
            Debug.Log("Could not find product in purchase confirmation.");
        }

        Debug.Log($"Purchase confirmed- Product: {product?.definition.id}");
    }

    void OnPurchaseConfirmationFailed(FailedOrder order)
    {
        var product = GetFirstProductInOrder(order);
        if (product == null)
        {
            Debug.Log("Could not find product in failed confirmation.");
        }

        Debug.Log($"Confirmation failed - Product: '{product?.definition.id}'," +
                  $"PurchaseFailureReason: {order.FailureReason.ToString()},"
                  + $"Confirmation Failure Details: {order.Details}");

    }

    Product GetFirstProductInOrder(Order order)
    {
        return order.CartOrdered.Items().First()?.Product;
    }

    void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.Log($"Store disconnected details: {description.message}");
    }

    void OnProductsFetchedFailed(ProductFetchFailed failure)
    {
        isProductsFetched = true;
        Debug.Log($"Products fetch failed for {failure.FailedFetchProducts.Count} products: {failure.FailureReason}");
    }

    void OnPurchasesFetched(Orders orders)
    {
        Debug.Log($"[IAP] PurchasesFetched - Pending:{orders.PendingOrders.Count}, Confirmed:{orders.ConfirmedOrders.Count}, Deferred:{orders.DeferredOrders.Count}");
        foreach (var pending in orders.PendingOrders)
        {
            var pid = pending.CartOrdered.Items().First()?.Product.definition.id;

            // TODO: grant item nếu cần (hoặc kiểm tra đã grant chưa)
            m_StoreController.ConfirmPurchase(pending);
        }
        // Trên Google Play: các Pending chưa xử lý thường sẽ được đẩy qua OnPurchasePending để bạn xử lý/Confirm. :contentReference[oaicite:1]{index=1}
    }

    void OnPurchasesFetchFailed(PurchasesFetchFailureDescription desc)
    {
        Debug.LogWarning($"[IAP] PurchasesFetchFailed - {desc.FailureReason} | {desc.Message}");
    }

}
