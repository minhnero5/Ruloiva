using TMPro;
using UnityEngine;
using JellyQuestUnity;

public class CoinTextDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string prefix = string.Empty;
    [SerializeField] private string suffix = string.Empty;

    private SaveService subscribedSave;

    private void Awake()
    {
        AssignTextIfMissing();
    }

    private void OnEnable()
    {
        RefreshCoins();
        SubscribeToSave();
    }

    private void OnDisable()
    {
        UnsubscribeFromSave();
    }

    private void Reset()
    {
        AssignTextIfMissing();
    }

    private void OnValidate()
    {
        AssignTextIfMissing();

        if (!Application.isPlaying && coinText != null)
        {
            coinText.text = FormatCoins(0);
        }
    }

    public void RefreshCoins()
    {
        var save = CurrentSave;
        if (save == null || save.Profile == null)
        {
            SetCoinText(0);
            return;
        }

        SetCoinText(save.Profile.coins);
    }

    public void SetCoinAmount(int amount)
    {
        SetCoinText(Mathf.Max(0, amount));
    }

    private SaveService CurrentSave => JellyQuestApp.Instance != null ? JellyQuestApp.Instance.Save : null;

    private void SubscribeToSave()
    {
        var save = CurrentSave;
        if (save == null || subscribedSave == save)
        {
            return;
        }

        UnsubscribeFromSave();
        subscribedSave = save;
        subscribedSave.CoinsChanged += HandleCoinsChanged;
    }

    private void UnsubscribeFromSave()
    {
        if (subscribedSave == null)
        {
            return;
        }

        subscribedSave.CoinsChanged -= HandleCoinsChanged;
        subscribedSave = null;
    }

    private void HandleCoinsChanged(int coins)
    {
        SetCoinText(coins);
    }

    private void SetCoinText(int amount)
    {
        if (!EnsureTextReference())
        {
            Debug.LogWarning("CoinTextDisplay requires a TextMeshProUGUI reference.", this);
            return;
        }

        coinText.text = FormatCoins(amount);
    }

    private string FormatCoins(int amount)
    {
        return prefix + Mathf.Max(0, amount) + suffix;
    }

    private void AssignTextIfMissing()
    {
        if (coinText == null)
        {
            coinText = GetComponent<TextMeshProUGUI>();
        }
    }

    private bool EnsureTextReference()
    {
        AssignTextIfMissing();
        return coinText != null;
    }
}
