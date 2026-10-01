using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class DynamicIapShopView : MonoBehaviour
{
    public readonly struct LocalCatalogDisplayProduct
    {
        public LocalCatalogDisplayProduct(
            string productId,
            decimal localizedPrice,
            int rawOrder,
            bool availableToPurchase,
            bool hasValidLocalizedPrice)
        {
            ProductId = productId;
            LocalizedPrice = localizedPrice;
            RawOrder = rawOrder;
            AvailableToPurchase = availableToPurchase;
            HasValidLocalizedPrice = hasValidLocalizedPrice;
        }

        public string ProductId { get; }
        public decimal LocalizedPrice { get; }
        public int RawOrder { get; }
        public bool AvailableToPurchase { get; }
        public bool HasValidLocalizedPrice { get; }
    }

    public static DynamicIapShopView Instance { get; private set; }

    public bool IsBuildResolved { get; private set; }
    public bool CanShowShopUi { get; private set; }

    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private IAPButtonView template;
    private readonly List<IAPButtonView> runtimeItems = new List<IAPButtonView>();
    private bool buildStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[IAP] Multiple DynamicIapShopView instances found. Destroying duplicate.");
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (!buildStarted)
        {
            StartCoroutine(BuildWhenReady());
        }
    }


    private IEnumerator BuildWhenReady()
    {
        buildStarted = true;

        while (IAPManager.Instance == null || !IAPManager.Instance.IsCatalogReady)
        {
            yield return null;
        }

        if (ServerUrl.IsServerConfigured)
        {
            while (UserDataManager.Instance == null || !UserDataManager.Instance.IsBootFetchSkusDone)
            {
                yield return null;
            }
        }

        while (IAPManager.Instance != null && !IAPManager.Instance.isProductsFetched)
        {
            yield return null;
        }

        if (contentRoot == null || template == null)
        {
            Debug.LogError("[IAP] Dynamic shop could not find Shop content root or template IAP card.");
            CanShowShopUi = false;
            IsBuildResolved = true;
            gameObject.SetActive(false);
            yield break;
        }

        template.gameObject.SetActive(false);

        List<SKU_INAPP> usableServerSkus = GetUsableServerSkus();
        DynamicIapShopCatalogDecision decision = ResolveCatalogDecision(usableServerSkus);
        ClearRuntimeItems();

        CanShowShopUi = decision.CanShowShopUi;
        if (!CanShowShopUi || decision.ItemCount <= 0)
        {
            contentRoot.gameObject.SetActive(false);
            IsBuildResolved = true;
            yield break;
        }

        int builtCount = 0;
        if (decision.Source == DynamicIapShopCatalogSource.ServerCatalog)
        {
            builtCount = BuildRuntimeItemsFromServerSkus(SortSkusForDisplay(usableServerSkus));
        }
        else if (decision.Source == DynamicIapShopCatalogSource.LocalCatalog)
        {
            builtCount = BuildRuntimeItemsFromLocalCatalog();
        }

        CanShowShopUi = CanShowShopUi && builtCount > 0;
        contentRoot.gameObject.SetActive(CanShowShopUi);
        IsBuildResolved = true;
    }

    public static List<SKU_INAPP> SortSkusForDisplay(IList<SKU_INAPP> usableSkus)
    {
        if (usableSkus == null || usableSkus.Count == 0)
        {
            return new List<SKU_INAPP>();
        }

        return usableSkus
            .Select((item, index) => new KeyValuePair<int, SKU_INAPP>(index, item))
            .OrderBy(entry => entry.Value.price)
            .ThenBy(entry => entry.Key)
            .Select(entry => entry.Value)
            .ToList();
    }

    public static List<LocalCatalogDisplayProduct> SortLocalCatalogForDisplay(IList<LocalCatalogDisplayProduct> localProducts)
    {
        if (localProducts == null || localProducts.Count == 0)
        {
            return new List<LocalCatalogDisplayProduct>();
        }

        return localProducts
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.ProductId) &&
                item.AvailableToPurchase &&
                item.HasValidLocalizedPrice)
            .OrderBy(item => item.LocalizedPrice)
            .ThenBy(item => item.RawOrder)
            .ToList();
    }

    private List<SKU_INAPP> GetUsableServerSkus()
    {
        if (UserDataManager.Instance == null || UserDataManager.Instance.skuManager == null)
        {
            return new List<SKU_INAPP>();
        }

        return UserDataManager.Instance.skuManager.GetNormalizedActiveSkusForIap();
    }

    private DynamicIapShopCatalogDecision ResolveCatalogDecision(IList<SKU_INAPP> usableServerSkus)
    {
        return DynamicIapShopCatalogResolver.Resolve(
            ServerUrl.IsServerConfigured,
            UserDataManager.Instance != null && UserDataManager.Instance.isServerOn,
            IAPManager.Instance.GetProductCount(),
            usableServerSkus?.Select(item => item.id).ToList());
    }

    private int BuildRuntimeItemsFromLocalCatalog()
    {
        List<string> sortedProductIds = GetSortedLocalProductIdsForDisplay();
        if (sortedProductIds.Count == 0)
        {
            return 0;
        }

        int builtCount = 0;
        foreach (string productId in sortedProductIds)
        {
            IAPButtonView item = Instantiate(template, contentRoot);
            item.gameObject.name = $"{template.name} Runtime ({productId})";
            item.Configure(productId);
            item.gameObject.SetActive(true);
            runtimeItems.Add(item);
            builtCount++;
        }

        return builtCount;
    }

    private List<string> GetSortedLocalProductIdsForDisplay()
    {
        var localProducts = new List<LocalCatalogDisplayProduct>();
        int productCount = IAPManager.Instance.GetProductCount();
        for (int i = 0; i < productCount; i++)
        {
            string productId = IAPManager.Instance.GetProductId(i);
            if (string.IsNullOrWhiteSpace(productId))
            {
                continue;
            }

            Product product = IAPManager.Instance.GetProductById(productId);
            bool hasValidLocalizedPrice = TryGetValidLocalizedPrice(product, out decimal localizedPrice);

            localProducts.Add(new LocalCatalogDisplayProduct(
                productId,
                localizedPrice,
                i,
                product != null && product.availableToPurchase,
                hasValidLocalizedPrice));
        }

        return SortLocalCatalogForDisplay(localProducts)
            .Select(item => item.ProductId)
            .ToList();
    }

    private static bool TryGetValidLocalizedPrice(Product product, out decimal localizedPrice)
    {
        localizedPrice = 0m;
        if (product == null || product.metadata == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(product.metadata.localizedPriceString))
        {
            return false;
        }

        localizedPrice = product.metadata.localizedPrice;
        return localizedPrice > 0m;
    }

    private int BuildRuntimeItemsFromServerSkus(IList<SKU_INAPP> sortedUsableSkus)
    {
        if (sortedUsableSkus == null || sortedUsableSkus.Count == 0)
        {
            return 0;
        }

        int builtCount = 0;
        foreach (var sku in sortedUsableSkus)
        {
            if (sku == null || string.IsNullOrWhiteSpace(sku.id))
            {
                continue;
            }

            if (IAPManager.Instance.GetProductById(sku.id) == null)
            {
                continue;
            }

            IAPButtonView item = Instantiate(template, contentRoot);
            item.gameObject.name = $"{template.name} Runtime ({sku.id})";
            item.Configure(sku.id);
            item.gameObject.SetActive(true);
            runtimeItems.Add(item);
            builtCount++;
        }

        return builtCount;
    }

    private void ClearRuntimeItems()
    {
        foreach (IAPButtonView item in runtimeItems)
        {
            if (item == null)
            {
                continue;
            }

            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
        }

        runtimeItems.Clear();

        foreach (IAPButtonView item in contentRoot.GetComponentsInChildren<IAPButtonView>(true))
        {
            if (item == null || item == template)
            {
                continue;
            }

            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
        }
    }
}
