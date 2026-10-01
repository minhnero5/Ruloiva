using TMPro;
using UnityEngine;

public class TextDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] [TextArea] private string defaultText;

    private void Awake()
    {
        ApplyDefaultText();
    }

    private void Reset()
    {
        AssignTargetTextIfMissing();
    }

    private void OnValidate()
    {
        AssignTargetTextIfMissing();

        if (targetText != null)
        {
            targetText.text = defaultText ?? string.Empty;
        }
    }

    public void SetText(string value)
    {
        if (!EnsureTargetText(true))
        {
            return;
        }

        targetText.text = value ?? string.Empty;
    }

    public string GetText()
    {
        return EnsureTargetText(false) ? targetText.text : string.Empty;
    }

    public void ClearText()
    {
        SetText(string.Empty);
    }

    private void ApplyDefaultText()
    {
        if (EnsureTargetText(false))
        {
            targetText.text = defaultText ?? string.Empty;
        }
    }

    private void AssignTargetTextIfMissing()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }
    }

    private bool EnsureTargetText(bool warnIfMissing)
    {
        AssignTargetTextIfMissing();
        if (targetText != null)
        {
            return true;
        }

        if (warnIfMissing)
        {
            Debug.LogWarning("TextDisplay requires a TextMeshProUGUI reference.", this);
        }

        return false;
    }
}
