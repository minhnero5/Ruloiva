using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using JellyQuestUnity;


public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    [SerializeField] int requestTimeout = 5;

    public UserData currentUserData;
    public StoredPurchaseInfo currentStoredPurchaseInfo;

    private string rsaPublicKeyPem;

    public bool isLoggedIn = false;
    public bool isServerOn = false;
    public bool IsBootFetchSkusDone { get; private set; } = false;

    // Separate local coin storage (offline coins)
    private int localCoinBackup = 0;
    private const string LOCAL_COIN_BACKUP_KEY = "LocalCoinBackup";
    private const string SERVER_COIN_OVERRIDE_ACTIVE_KEY = "ServerCoinOverrideActive";

    public SkuManager skuManager = new SkuManager();

    public static Action OnUserLogoutAction;
    public static Action<bool> OnGetUserStatusAction;
    public static Action<bool> OnShowLoadingPopupAction;
    public static Action OnLoginSuccessAction;
    public static Action OnSignUpSuccessAction;
    public static Action OnBuyInappAction;
    public static Action OnServerOffAction;

    private ApiClient _api;
    private FlowRunner _flows;
    private IFlowUI _ui;
    private bool _isSaveCoinSyncSubscribed;
    private SaveService _subscribedSave;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance != null) return;

        Instance = this;
        DontDestroyOnLoad(gameObject);

        rsaPublicKeyPem = Resources.Load<TextAsset>("public_key").text;

        // Ensure not null
        if (currentUserData == null) currentUserData = new UserData();

        SetupPipeline();
    }

    private void Start()
    {
        InitializeLocalCoin();
        SubscribeSaveCoinSync();
        // Boot fetch skus (ServerUrl.APP)
        BootFetchSkus();

    }

    private void OnDestroy()
    {
        UnsubscribeSaveCoinSync();
    }

    private void SetupPipeline()
    {
        _ui = new DefaultFlowUI();

        _api = new ApiClient(
            timeout: requestTimeout,
            rsaPublicKeyPem: rsaPublicKeyPem,
            getToken: () => currentUserData != null ? currentUserData.token : null
        );

        _flows = new FlowRunner(
            host: this,
            ui: _ui,
            specOf: (id) => FlowUiRegistry.Specs.TryGetValue(id, out var s) ? s : new FlowUiSpec()
        );
    }

    private void InitializeLocalCoin()
    {
        if (currentUserData == null)
        {
            currentUserData = new UserData();
        }

        var runtimeCoins = GetRuntimeCoins();
        localCoinBackup = PlayerPrefs.GetInt(LOCAL_COIN_BACKUP_KEY, runtimeCoins);

        if (IsServerCoinOverrideActive())
        {
            ApplyRuntimeCoins(localCoinBackup);
            ClearServerCoinOverrideFlag();
            runtimeCoins = GetRuntimeCoins();
        }
        else
        {
            localCoinBackup = runtimeCoins;
            PersistLocalCoinBackup();
        }

        currentUserData.coin = runtimeCoins;
        RefreshRuntimeScreen();
        Debug.Log("Local coin loaded from runtime save: " + runtimeCoins);
    }

    private void SubscribeSaveCoinSync()
    {
        if (_isSaveCoinSyncSubscribed)
        {
            return;
        }

        _subscribedSave = RuntimeSave;
        if (_subscribedSave == null)
        {
            return;
        }

        _subscribedSave.CoinsChanged += HandleSaveCoinsChanged;
        _isSaveCoinSyncSubscribed = true;
    }

    private void UnsubscribeSaveCoinSync()
    {
        if (!_isSaveCoinSyncSubscribed || _subscribedSave == null)
        {
            return;
        }

        _subscribedSave.CoinsChanged -= HandleSaveCoinsChanged;
        _subscribedSave = null;
        _isSaveCoinSyncSubscribed = false;
    }

    private void HandleSaveCoinsChanged(int coins)
    {
        if (currentUserData == null)
        {
            currentUserData = new UserData();
        }

        currentUserData.coin = coins;

        if (!isLoggedIn && localCoinBackup != coins)
        {
            localCoinBackup = coins;
            PersistLocalCoinBackup();
        }

        RefreshRuntimeScreen();
    }

    private SaveService RuntimeSave => JellyQuestApp.Instance != null ? JellyQuestApp.Instance.Save : null;

    private int GetRuntimeCoins()
    {
        var save = RuntimeSave;
        return save != null && save.Profile != null
            ? save.Profile.coins
            : Mathf.Max(0, localCoinBackup);
    }

    private void ApplyRuntimeCoins(int coins)
    {
        var save = RuntimeSave;
        if (save == null)
        {
            if (currentUserData == null)
            {
                currentUserData = new UserData();
            }

            currentUserData.coin = Mathf.Max(0, coins);
            return;
        }

        var previousCoins = save.Profile != null ? (int?)save.Profile.coins : null;
        save.SetCoins(coins);

        if (currentUserData == null)
        {
            currentUserData = new UserData();
        }

        var appliedCoins = save.Profile != null ? save.Profile.coins : Mathf.Max(0, coins);
        currentUserData.coin = appliedCoins;

        if (!previousCoins.HasValue || previousCoins.Value == appliedCoins)
        {
            RefreshRuntimeScreen();
        }
    }

    private void RefreshRuntimeScreen()
    {
        if (JellyQuestApp.Instance == null)
        {
            return;
        }

        JellyQuestApp.Instance.RefreshActiveScreenFromSave();
    }

    private bool IsServerCoinOverrideActive()
    {
        return PlayerPrefs.GetInt(SERVER_COIN_OVERRIDE_ACTIVE_KEY, 0) == 1;
    }

    private void SetServerCoinOverrideActive(bool isActive)
    {
        PlayerPrefs.SetInt(SERVER_COIN_OVERRIDE_ACTIVE_KEY, isActive ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ClearServerCoinOverrideFlag()
    {
        if (!PlayerPrefs.HasKey(SERVER_COIN_OVERRIDE_ACTIVE_KEY))
        {
            return;
        }

        PlayerPrefs.DeleteKey(SERVER_COIN_OVERRIDE_ACTIVE_KEY);
        PlayerPrefs.Save();
    }

    private void PersistLocalCoinBackup()
    {
        PlayerPrefs.SetInt(LOCAL_COIN_BACKUP_KEY, localCoinBackup);
        PlayerPrefs.Save();
    }

    private void CacheLocalCoinsBeforeServerOverride()
    {
        if (IsServerCoinOverrideActive())
        {
            return;
        }

        localCoinBackup = GetRuntimeCoins();
        PersistLocalCoinBackup();
        SetServerCoinOverrideActive(true);
    }

    // =========================
    // Helpers
    // =========================
    private IEnumerator EnsureServerOnOrFail(ApiFlowId flowId)
    {
        if (isServerOn) yield break;

        ApiResult ping = null;
        yield return _api.GetEncrypted(ServerUrl.APP, r => ping = r);

        if (!ping.Ok)
        {
            bool hasInternet = false;
            yield return _api.CheckInternet(ok => hasInternet = ok);

            if (!hasInternet)
            {
                _flows.HandleResult(flowId, ApiResult.Fail(ApiErrorKind.NoInternet, "No internet connection!"));
                yield break;
            }

            isServerOn = false;
            // Nếu đang logged in mà server tắt -> event riêng (giữ behavior cũ)
            if (isLoggedIn) OnServerOffAction?.Invoke();
            _flows.HandleResult(flowId, ApiResult.Fail(ApiErrorKind.ServerOff, "Server is not available!"));
            yield break;
        }

        // Parse sku luôn (tiện)
        try
        {
            var json = JObject.Parse(ping.Raw);
            skuManager.skuInapps = json["skus"]?.ToObject<List<SKU_INAPP>>();
        }
        catch { /* ignore */ }

        isServerOn = true;
    }

    private static string TryParseMessage(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        try
        {
            var j = JObject.Parse(raw);
            return j["message"]?.ToString();
        }
        catch { return null; }
    }

    // =========================
    // Boot: Fetch APP/SKUs
    // =========================
    public void GetDataFromServer()
    {
        // giữ API cũ để nơi khác gọi không vỡ
        BootFetchSkus();
    }

    public void BootFetchSkus()
    {
          IsBootFetchSkusDone = false;
        _flows.Run(ApiFlowId.BootFetchSkus, BootFetchSkusFlow);
    }

    private IEnumerator BootFetchSkusFlow()
    {
        ApiResult res = null;
        yield return _api.GetEncrypted(ServerUrl.APP, r => res = r);

        _flows.HandleResult(ApiFlowId.BootFetchSkus, res,
            onSuccess: () =>
            {
                var json = JObject.Parse(res.Raw);
                skuManager.skuInapps = json["skus"]?.ToObject<List<SKU_INAPP>>();
                Debug.Log("SKU In-App List Count: " + (skuManager.skuInapps != null ? skuManager.skuInapps.Count : 0));
                isServerOn = true;
                 IsBootFetchSkusDone = true;
            },
            onFail: () =>
            {
                isServerOn = false;
                IsBootFetchSkusDone = true;
            }
        );
    }

    // =========================
    // Login
    // =========================
    public void Login(string username, string password)
    {
        _flows.Run(ApiFlowId.Login, () => LoginFlow(username, password));
    }

    private IEnumerator LoginFlow(string username, string password)
    {
        if (!ApiClient.HasInternet())
        {
            _flows.HandleResult(ApiFlowId.Login, ApiResult.Fail(ApiErrorKind.NoInternet, "Internet is not available!"));
            yield break;
        }

        // ensure server
        yield return EnsureServerOnOrFail(ApiFlowId.Login);
        if (!isServerOn) yield break;

        var loginData = JsonConvert.SerializeObject(new { username, password });

        ApiResult res = null;
        yield return _api.PostEncrypted(ServerUrl.LOGIN, loginData, r => res = r);

        // Nếu server die (connection) mà đang login, bạn có thể coi là ServerOff
        if (!res.Ok && res.ErrorKind == ApiErrorKind.Connection)
        {
            res.ErrorMessage = TryParseMessage(res.ErrorRaw) ?? res.ErrorMessage;
        }

        _flows.HandleResult(ApiFlowId.Login, res,
            onSuccess: () =>
            {
                ApplyLoginPayload(res.Raw);

                // Nested flow: sync coin
                GetUserCoin();
            },
            onFail: () =>
            {
                // show message server trả về nếu có
                var msg = TryParseMessage(res.ErrorRaw);
                if (!string.IsNullOrEmpty(msg)) NoticePopUp.Instance.ShowMessage(msg);
            }
        );
    }

    private void ApplyLoginPayload(string decrypted)
    {
        var jsonResponse = JObject.Parse(decrypted);
        var user = jsonResponse["user"];
        currentUserData = user.ToObject<UserData>();

        var token = jsonResponse["accessToken"]?.ToString();
        currentUserData.token = token;

        CacheLocalCoinsBeforeServerOverride();
        isLoggedIn = true;
        OnLoginSuccessAction?.Invoke();

        // Apply server coin
        ApplyRuntimeCoins(currentUserData.coin);

        // (giữ behavior cũ) status tạm ACTIVE
        OnGetUserStatusAction?.Invoke(false);
    }

    // =========================
    // SignUp
    // =========================
    public void SignUp(string username, string password, string team = "", string email = "")
    {
        _flows.Run(ApiFlowId.SignUp, () => SignUpFlow(username, password, team, email));
    }

    private IEnumerator SignUpFlow(string username, string password, string team, string email)
    {
        if (!ApiClient.HasInternet())
        {
            _flows.HandleResult(ApiFlowId.SignUp, ApiResult.Fail(ApiErrorKind.NoInternet, "Internet is not available!"));
            yield break;
        }

        yield return EnsureServerOnOrFail(ApiFlowId.SignUp);
        if (!isServerOn) yield break;

        var signUpData = JsonConvert.SerializeObject(new { username, password, team, email });

        ApiResult res = null;
        yield return _api.PostEncrypted(ServerUrl.SIGN_UP, signUpData, r => res = r);

        _flows.HandleResult(ApiFlowId.SignUp, res,
            onSuccess: () =>
            {
                // nếu server trả user
                try
                {
                    var json = JObject.Parse(res.Raw);
                    var user = json["user"];
                    if (user != null) currentUserData = user.ToObject<UserData>();
                }
                catch { }

                OnSignUpSuccessAction?.Invoke();
                Login(username, password); // auto login after sign up
            },
            onFail: () =>
            {
                var msg = TryParseMessage(res.ErrorRaw);
                if (!string.IsNullOrEmpty(msg)) NoticePopUp.Instance.ShowMessage(msg);
            }
        );
    }

    // =========================
    // Profile: coin
    // =========================
    public void GetUserCoin()
    {
        _flows.Run(ApiFlowId.GetProfileCoin, GetUserCoinFlow);
    }

    private IEnumerator GetUserCoinFlow()
    {
        if (!ApiClient.HasInternet())
        {
            _flows.HandleResult(ApiFlowId.GetProfileCoin, ApiResult.Fail(ApiErrorKind.NoInternet, "Internet is not available!"));
            yield break;
        }

        yield return EnsureServerOnOrFail(ApiFlowId.GetProfileCoin);
        if (!isServerOn) yield break;

        ApiResult res = null;
        yield return _api.GetEncrypted(ServerUrl.PROFILE, r => res = r);

        _flows.HandleResult(ApiFlowId.GetProfileCoin, res,
            onSuccess: () =>
            {
                var jsonResponse = JObject.Parse(res.Raw);
                var user = jsonResponse["user"];
                if (user != null)
                {
                    currentUserData.coin = user["coin"]?.ToObject<int>() ?? currentUserData.coin;
                    ApplyRuntimeCoins(currentUserData.coin);
                }
            }
        );
    }

    // =========================
    // Profile: status (flow riêng cho IAP)
    // =========================
    public void GetUserStatus()
    {
        _flows.Run(ApiFlowId.GetProfileStatus, GetUserStatusFlow);
    }

    private IEnumerator GetUserStatusFlow()
    {
        if (!ApiClient.HasInternet())
        {
            _flows.HandleResult(ApiFlowId.GetProfileStatus, ApiResult.Fail(ApiErrorKind.NoInternet, "Internet is not available!"));
            yield break;
        }

        yield return EnsureServerOnOrFail(ApiFlowId.GetProfileStatus);
        if (!isServerOn) yield break;

        ApiResult res = null;
        yield return _api.GetEncrypted(ServerUrl.PROFILE, r => res = r);

        _flows.HandleResult(ApiFlowId.GetProfileStatus, res,
            onSuccess: () =>
            {
                var jsonResponse = JObject.Parse(res.Raw);
                var user = jsonResponse["user"];
                var status = user?["status"]?.ToString() ?? "unknown";
                bool isRestricted = status != "ACTIVE";

                OnGetUserStatusAction?.Invoke(isRestricted);

                if (!isRestricted)
                    OnBuyInappAction?.Invoke();
            }
        );
    }

    // =========================
    // Coins: Spend/Add (online/offline)
    // =========================
    public void SpendCoins(int amount)
    {
        if (amount <= 0) return;

        if (!isLoggedIn)
        {
            ApplyRuntimeCoins(GetRuntimeCoins() - amount);
            return;
        }

        _flows.Run(ApiFlowId.SpendCoins, () => SpendCoinsFlow(amount));
    }

    private IEnumerator SpendCoinsFlow(int amount)
    {
        if (!ApiClient.HasInternet())
        {
            _flows.HandleResult(ApiFlowId.SpendCoins, ApiResult.Fail(ApiErrorKind.NoInternet, "Internet is not available!"));
            yield break;
        }

        yield return EnsureServerOnOrFail(ApiFlowId.SpendCoins);
        if (!isServerOn) yield break;

        var spendData = JsonConvert.SerializeObject(new { amount = amount, reason = "Buy Item" });

        ApiResult res = null;
        yield return _api.PostEncrypted(ServerUrl.SPEND_COINS, spendData, r => res = r);

        _flows.HandleResult(ApiFlowId.SpendCoins, res,
            onSuccess: () =>
            {
                var jsonResponse = JObject.Parse(res.Raw);
                currentUserData.coin = jsonResponse["currentCoin"]?.ToObject<int>() ?? currentUserData.coin;
                ApplyRuntimeCoins(currentUserData.coin);
            }
        );
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        if (!isLoggedIn)
        {
            ApplyRuntimeCoins(GetRuntimeCoins() + amount);
            return;
        }

        _flows.Run(ApiFlowId.AddCoins, () => AddCoinsFlow(amount));
    }

    private IEnumerator AddCoinsFlow(int amount)
    {
        if (!ApiClient.HasInternet())
        {
            _flows.HandleResult(ApiFlowId.AddCoins, ApiResult.Fail(ApiErrorKind.NoInternet, "Internet is not available!"));
            yield break;
        }

        yield return EnsureServerOnOrFail(ApiFlowId.AddCoins);
        if (!isServerOn) yield break;

        var addData = JsonConvert.SerializeObject(new { amount = amount, reason = "Completed Level Reward" });

        ApiResult res = null;
        yield return _api.PostEncrypted(ServerUrl.ADD_COINS, addData, r => res = r);

        _flows.HandleResult(ApiFlowId.AddCoins, res,
            onSuccess: () =>
            {
                var jsonResponse = JObject.Parse(res.Raw);
                currentUserData.coin = jsonResponse["currentCoin"]?.ToObject<int>() ?? currentUserData.coin;
                ApplyRuntimeCoins(currentUserData.coin);
            }
        );
    }

    // =========================
    // Purchase
    // =========================
    public void SetOrderInfo(Order order)
    {
#if UNITY_EDITOR
        var productId = order.CartOrdered.Items().FirstOrDefault()?.Product.definition.id ?? "unknown_product";
        string randomCode = Guid.NewGuid().ToString("N").Substring(0, 12);
        currentStoredPurchaseInfo.productId = productId;
        currentStoredPurchaseInfo.purchaseToken = randomCode;
        currentStoredPurchaseInfo.orderId = order.Info.TransactionID;
        currentStoredPurchaseInfo.priceAmount = order.CartOrdered.Items().First()?.Product.metadata.localizedPrice ?? 0;
        currentStoredPurchaseInfo.curency = order.CartOrdered.Items().First()?.Product.metadata.isoCurrencyCode ?? "unknown_currency";
#else
        var root = JObject.Parse(order.Info.Receipt);
        var gp = JObject.Parse(root["Payload"].ToString());
        var json = JObject.Parse(gp["json"].ToString());
        currentStoredPurchaseInfo.productId = json["productId"]?.ToString() ?? "unknown_product";
        currentStoredPurchaseInfo.purchaseToken = json["purchaseToken"]?.ToString() ?? "temporary_token";
        currentStoredPurchaseInfo.orderId = order.Info.TransactionID;
        currentStoredPurchaseInfo.priceAmount =  order.CartOrdered.Items().First()?.Product.metadata.localizedPrice ?? 0;
        currentStoredPurchaseInfo.curency = order.CartOrdered.Items().First()?.Product.metadata.isoCurrencyCode ?? "unknown_currency";
#endif
    }

    public void VerifyPurchaseInfo()
    {
        if (!isLoggedIn)
        {
            _flows.Run(ApiFlowId.VerifyPurchase, GuessVerifyPurchaseFlow);
            return;
        }
        _flows.Run(ApiFlowId.VerifyPurchase, VerifyPurchaseFlow);
    }


    private IEnumerator GuessVerifyPurchaseFlow()
    {
        if (!ApiClient.HasInternet())
        {
            Debug.LogWarning("Guest purchase: No internet, skipping verify");
            yield break;
        }

        // Ping server nhưng không hiện popup nếu fail
        ApiResult ping = null;
        yield return _api.GetEncrypted(ServerUrl.APP, r => ping = r);

        if (!ping.Ok)
        {
            Debug.LogWarning("Guest purchase: Server off, skipping verify");
            yield break;
        }

        // Server OK → verify như bình thường
        var jsonData = JsonConvert.SerializeObject(currentStoredPurchaseInfo);
        ApiResult res = null;
        yield return _api.PostEncrypted(ServerUrl.VERIFY_PURCHASE, jsonData, r => res = r);

        if (res.Ok)
        {
            Debug.Log("Guest purchase verified successfully");
        }
        else
        {
            Debug.LogWarning($"Guest purchase verify failed: {res.ErrorMessage}");
        }
    }

    private IEnumerator VerifyPurchaseFlow()
    {
        if (!ApiClient.HasInternet())
        {
            _flows.HandleResult(ApiFlowId.VerifyPurchase, ApiResult.Fail(ApiErrorKind.NoInternet, "Internet is not available!"));
            yield break;
        }

        yield return EnsureServerOnOrFail(ApiFlowId.VerifyPurchase);
        if (!isServerOn) yield break;

        var jsonData = JsonConvert.SerializeObject(currentStoredPurchaseInfo);

        ApiResult res = null;
        yield return _api.PostEncrypted(ServerUrl.VERIFY_PURCHASE, jsonData, r => res = r);

        _flows.HandleResult(ApiFlowId.VerifyPurchase, res,
            onSuccess: () =>
            {
                if (isLoggedIn)
                {
                    var jsonResponse = JObject.Parse(res.Raw);
                    currentUserData.coin = jsonResponse["currentCoin"]?.ToObject<int>() ?? currentUserData.coin;
                    ApplyRuntimeCoins(currentUserData.coin);
                }
            },

            onFail: () =>
            {
                int amount = skuManager.GetAmountProduct(currentStoredPurchaseInfo.productId);
                AddCoins(amount);
            }
        );
    }

    // =========================
    // Logout
    // =========================
    public void Logout(Action action = null)
    {
        isLoggedIn = false;
        currentUserData = new UserData();

        // Restore local coins from backup (not from server)
        ApplyRuntimeCoins(localCoinBackup);
        ClearServerCoinOverrideFlag();

        OnUserLogoutAction?.Invoke();
        action?.Invoke();
    }
}

[System.Serializable]
public class UserData
{
    public string id;
    public string username;
    public string team;
    public string role;
    public int coin;
    public string token;
}

[System.Serializable]
public class StoredPurchaseInfo
{
    public string orderId;
    public string productId;
    public string purchaseToken;
    public decimal priceAmount;
    public string curency;
}
