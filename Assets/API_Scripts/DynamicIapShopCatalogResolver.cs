using System.Collections.Generic;

public enum DynamicIapShopCatalogSource
{
    None,
    LocalCatalog,
    ServerCatalog
}

public readonly struct DynamicIapShopCatalogDecision
{
    public DynamicIapShopCatalogDecision(DynamicIapShopCatalogSource source, int itemCount, bool canShowShopUi)
    {
        Source = source;
        ItemCount = itemCount;
        CanShowShopUi = canShowShopUi;
    }

    public DynamicIapShopCatalogSource Source { get; }
    public int ItemCount { get; }
    public bool CanShowShopUi { get; }
}

public static class DynamicIapShopCatalogResolver
{
    public static DynamicIapShopCatalogDecision Resolve(
        bool isServerConfigured,
        bool isServerOn,
        int runtimeCatalogCount,
        IList<string> serverProductIds)
    {
        if (!isServerConfigured)
        {
            return new DynamicIapShopCatalogDecision(
                DynamicIapShopCatalogSource.LocalCatalog,
                runtimeCatalogCount,
                runtimeCatalogCount > 0);
        }

        int usableServerCount = serverProductIds?.Count ?? 0;
        if (isServerOn && usableServerCount > 0)
        {
            return new DynamicIapShopCatalogDecision(
                DynamicIapShopCatalogSource.ServerCatalog,
                usableServerCount,
                true);
        }

        return new DynamicIapShopCatalogDecision(
            DynamicIapShopCatalogSource.None,
            0,
            false);
    }
}
