using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SkuManager
{
    public List<SKU_INAPP> skuInapps = new List<SKU_INAPP>();

    public int GetTotalSku()
    {
        return skuInapps?.Count ?? 0;
    }

    public bool IsSkuAvailable(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        foreach (var item in SafeSkuList())
        {
            if (item != null &&
                item.id == id &&
                string.Equals(item.status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public string GetTileProduct(string id)
    {
        return FindById(id)?.title ?? string.Empty;
    }

    public string GetDescriptionProduct(string id)
    {
        return FindById(id)?.description ?? string.Empty;
    }

    public float GetPriceProduct(string id)
    {
        return FindById(id)?.price ?? 0f;
    }

    public int GetAmountProduct(string id)
    {
        return FindById(id)?.amount ?? 0;
    }

    public List<SKU_INAPP> GetNormalizedActiveSkusForIap()
    {
        var items = SafeSkuList();
        if (items.Count == 0)
        {
            return new List<SKU_INAPP>();
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var uniqueActiveItems = new List<SKU_INAPP>();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item == null || string.IsNullOrWhiteSpace(item.id))
            {
                continue;
            }

            if (!string.Equals(item.status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!seenIds.Add(item.id))
            {
                continue;
            }

            uniqueActiveItems.Add(item);
        }

        return uniqueActiveItems;
    }

    public List<string> GetNormalizedActiveProductIdsForIap()
    {
        return GetNormalizedActiveSkusForIap()
            .Select(entry => entry.id)
            .ToList();
    }

    private SKU_INAPP FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        foreach (var item in SafeSkuList())
        {
            if (item != null && item.id == id)
            {
                return item;
            }
        }

        return null;
    }

    private List<SKU_INAPP> SafeSkuList()
    {
        return skuInapps ?? new List<SKU_INAPP>();
    }
}

[System.Serializable]
public class SKU_INAPP
{
    public string sku;
    public string id;
    public string status;
    public int amount;
    public float price;
    public string title;
    public string description;
}
