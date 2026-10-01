using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using System;
using System.Collections;
using JellyQuestUnity;
using TMPro;

public class IAPButtonView : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    [SerializeField] int inappIndex = 0;

    Button buyBtn;
    int coin = 500;
    private string productId = string.Empty;
    private Coroutine bindAndFetchCoroutine;

    private void Awake()
    {
        buyBtn = GetComponent<Button>();
        if (buyBtn != null)
        {
            buyBtn.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    public void Configure(int index)
    {
        ResetBindingState();
        inappIndex = index;
        RestartBindingCoroutineIfNeeded();
    }

    public void Configure(string configuredProductId)
    {
        ResetBindingState();
        productId = configuredProductId;
        inappIndex = -1;
        RestartBindingCoroutineIfNeeded();
    }

    private void ResetBindingState()
    {
        productId = string.Empty;
        coin = 500;
        if (bindAndFetchCoroutine != null)
        {
            StopCoroutine(bindAndFetchCoroutine);
            bindAndFetchCoroutine = null;
        }
    }

    private void RestartBindingCoroutineIfNeeded()
    {
        if (isActiveAndEnabled)
        {
            bindAndFetchCoroutine = StartCoroutine(BindAndFetchInapp());
        }
    }

    void SetActiveBuyButton(bool state)
    {
        if (buyBtn != null)
        {
            buyBtn.interactable = state;
        }
    }

    void OnEnable()
    {
        if (bindAndFetchCoroutine != null)
        {
            StopCoroutine(bindAndFetchCoroutine);
        }
        bindAndFetchCoroutine = StartCoroutine(BindAndFetchInapp());    
    }

    void OnDisable()
    {
        if (bindAndFetchCoroutine != null)
        {
            StopCoroutine(bindAndFetchCoroutine);
            bindAndFetchCoroutine = null;
        }
    }

    private IEnumerator BindAndFetchInapp()
    {
        while (IAPManager.Instance == null || !IAPManager.Instance.IsCatalogReady)
        {
            yield return null;
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            productId = IAPManager.Instance.GetProductId(inappIndex);
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            Debug.LogWarning($"[IAP] Product index {inappIndex} is out of catalog range. Hiding button.");
            gameObject.SetActive(false);
            yield break;
        }

        FetchedInapp();
    }

    private void FetchedInapp()
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            gameObject.SetActive(false);
            return;
        }

        if (!ServerUrl.IsServerConfigured)
        {
            Debug.Log("Server URL not configured, skipping product fetch on Server.");
#if !UNITY_EDITOR
            StartCoroutine(WaitingProductFetch());
#endif
            return;
        }

        if (UserDataManager.Instance == null)
        {
#if !UNITY_EDITOR
            StartCoroutine(WaitingProductFetch());
#endif
            return;
        }

        Debug.Log("[IAP] Fetching product: " + productId);
        var skuItems = UserDataManager.Instance.skuManager?.skuInapps;
        if (UserDataManager.Instance.isServerOn && skuItems != null && skuItems.Count > 0)
        {
            if (UserDataManager.Instance.skuManager.IsSkuAvailable(productId))
            {
#if UNITY_EDITOR
                FectchProductFromSKU();
                Debug.Log("[IAP] Editor Mode: Fetching product from SKU manager.");
                return;
#endif

                if (IAPManager.Instance.GetProductById(productId) != null)
                {
                    FectchProductFromSKU();
                }
                else
                {
                    gameObject.SetActive(false);
                    Debug.Log("Product " + productId + " is not available for purchase.");
                }
            }
            else
            {
                Debug.Log("Cant fetching product" + productId + " from SKU Manager");
                gameObject.SetActive(false);
            }
        }
        else
        {
#if !UNITY_EDITOR
        StartCoroutine(WaitingProductFetch());
#endif
        }
    }

    private void OnBuyButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(productId) || IAPManager.Instance == null)
        {
            Debug.LogWarning("[IAP] Purchase blocked: product ID is invalid or IAP manager is not ready.");
            return;
        }

        if (!ServerUrl.IsServerConfigured)
        {
            IAPManager.Instance.BuyProduct(this.productId, (quantity) =>
                {
                    OnPurchaseSuccess(quantity);
                });
            return;
        }

        if (UserDataManager.Instance.isLoggedIn)
        {
            UserDataManager.OnBuyInappAction = () =>
            {
                Debug.Log($"[IAP] Attempting to purchase product: {productId}");
                IAPManager.Instance.BuyProduct(this.productId, (quantity) =>
                {
                    OnPurchaseSuccess(quantity);
                });
            };

            UserDataManager.Instance.GetUserStatus();
        }
        else
        {
            Action purchaseAction = () =>
            {
                IAPManager.Instance.BuyProduct(this.productId, (quantity) =>
                {
                    OnPurchaseSuccess(quantity);
                });
            };

            ApiPopupManager.Instance.ShowAskLoginPopup(purchaseAction, purchaseAction);
        }
    }

    public void FectchProductFromSKU()
    {
        coin = UserDataManager.Instance.skuManager.GetAmountProduct(productId);
        titleText.text = coin.ToString();
        priceText.text = UserDataManager.Instance.skuManager.GetPriceProduct(productId).ToString() + "$";
    }

    public void OnProductFetched(Product product)
    {
        if (product == null)
        {
            Debug.LogError($"[IAP] Product with ID {productId} not found.");
            gameObject.SetActive(false);
            return;
        }

        if (product.availableToPurchase)
        {
            string tile = product.metadata.localizedTitle;
            coin = int.Parse(tile.Split(' ')[0]);
            titleText.text = coin.ToString();
            priceText.text = product.metadata.localizedPriceString;
        }
    }

    public void OnPurchaseSuccess(int quantity = 1)
    {
        UserDataManager userDataManager = UserDataManager.Instance;
        var runtimeSave = JellyQuestApp.Instance != null ? JellyQuestApp.Instance.Save : null;
        if (userDataManager == null)
        {
            if (runtimeSave != null)
            {
                runtimeSave.AddCoins(coin * quantity);
            }
            else
            {
                Debug.LogWarning("[IAP] Purchase succeeded but JellyQuestApp.Save is unavailable.");
            }

            return;
        }

        if (!userDataManager.isLoggedIn)
        {
            userDataManager.AddCoins(coin * quantity);
            NoticePopUp.Instance.ShowMessage("Purchase Successful");
        }
        else
        {
            if (runtimeSave != null)
            {
                runtimeSave.AddCoins(coin * quantity);
            }
            else
            {
                Debug.LogWarning("[IAP] Purchase succeeded but JellyQuestApp.Save is unavailable.");
            }
        }
    }

    IEnumerator WaitingProductFetch()
    {
        while (IAPManager.Instance == null || !IAPManager.Instance.isProductsFetched)
        {
            yield return null;
        }
        OnProductFetched(IAPManager.Instance.GetProductById(productId));
    }
}
