using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
    public sealed partial class GameplayScreenController
    {
                private void ShowPausePopup()
                {
                    HideAllOverlays();
                    if (_view.pauseOverlay != null)
                    {
                        _view.pauseOverlay.SetActive(true);
                    }
                }

                private void ShowShop(BoosterType boosterType)
                {
                    HideAllOverlays();
                    if (_view.shopOverlay != null)
                    {
                        _view.shopOverlay.SetActive(true);
                    }

                    if (_view.shopIconImage != null)
                    {
                        _view.shopIconImage.sprite = GetBoosterSprite(boosterType, true);
                    }

                    RefreshShopPopup(boosterType);

                    BindButton(_view.shopBuyButton, () =>
                    {
                        var unlock = GetUnlock(boosterType);
                        if (!App.Save.TrySpendCoins(unlock.price))
                        {
                            ShowToast("Not enough coins");
                            RefreshShopPopup(boosterType);
                            return;
                        }

                        App.Audio.PlayMarker("bouton");
                        App.Save.AddBooster(boosterType, 1);
                        UpdateHud();
                        RefreshShopPopup(boosterType);
                    });
                }

                private void RefreshShopPopup(BoosterType boosterType)
                {
                    var unlock = GetUnlock(boosterType);
                    var currentCoins = App.Save.Profile.coins;
                    var ownedCount = App.Save.GetBoosterCount(boosterType);
                    if (_view.shopPriceText != null)
                    {
                        _view.shopPriceText.text = "Price: " + unlock.price;
                    }

                    if (_view.shopCoinsText != null)
                    {
                        _view.shopCoinsText.text = "Coins: " + currentCoins;
                    }

                    if (_view.shopOwnedText != null)
                    {
                        _view.shopOwnedText.text ="x"+ ownedCount.ToString();
                    }
                }

                private void ShowWinPopup()
                {
                    HideAllOverlays();
                    if (_view.winOverlay != null)
                    {
                        _view.winOverlay.SetActive(true);
                    }

                    if (_view.winScoreText != null)
                    {
                        _view.winScoreText.text = "Score: " + _board.Score + "\nCoins earned: " + _board.EarnedCoins;
                    }

                    EnsureWinPopupStars();
                    var earnedStars = Mathf.Clamp(_board.CurrentStars, 0, 3);
                    if (!HasWinStarImages())
                    {
                        if (_view.winStarsText != null)
                        {
                            _view.winStarsText.gameObject.SetActive(true);
                            _view.winStarsText.text = earnedStars > 0 ? new string('*', earnedStars) : string.Empty;
                        }

                        SetWinButtonsInteractable(true);
                        return;
                    }

                    if (_view.winStarsText != null)
                    {
                        _view.winStarsText.gameObject.SetActive(false);
                    }

                    ResetWinStarVisuals();
                    SetWinButtonsInteractable(false);
                    _winStarsCoroutine = StartCoroutine(AnimateWinStarsRoutine(earnedStars));
                }

                private IEnumerator AnimateWinStarsRoutine(int earnedStars)
                {
                    var revealSteps = BuildWinStarRevealSteps(Mathf.Clamp(earnedStars, 0, 3));
                    if (revealSteps.Length == 0)
                    {
                        yield return new WaitForSecondsRealtime(WinStarZeroRevealDelay);
                        SetWinButtonsInteractable(true);
                        _winStarsCoroutine = null;
                        yield break;
                    }

                    for (var index = 0; index < revealSteps.Length; index++)
                    {
                        yield return AnimateWinStarReveal(revealSteps[index]);
                        if (index < revealSteps.Length - 1)
                        {
                            yield return new WaitForSecondsRealtime(WinStarRevealDelay);
                        }
                    }

                    SetWinButtonsInteractable(true);
                    _winStarsCoroutine = null;
                }

                private IEnumerator AnimateWinStarReveal(WinStarRevealStep revealStep)
                {
                    if (revealStep.image == null)
                    {
                        yield break;
                    }

                    revealStep.image.gameObject.SetActive(true);
                    var elapsed = 0f;
                    while (elapsed < WinStarRevealDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        var t = Mathf.Clamp01(elapsed / WinStarRevealDuration);
                        var eased = Mathf.SmoothStep(0f, 1f, t);
                        var scale = EvaluateWinStarScale(t);
                        var anchoredPosition = Vector2.Lerp(
                            revealStep.anchoredPosition + (Vector2.up * WinStarStartOffsetY),
                            revealStep.anchoredPosition,
                            eased);
                        var angle = Mathf.Lerp(revealStep.startAngle, 0f, eased);
                        SetWinStarVisualState(revealStep.image, anchoredPosition, eased, scale, angle);
                        yield return null;
                    }

                    SetWinStarVisualState(revealStep.image, revealStep.anchoredPosition, 1f, 1f, 0f);
                }

                private void EnsureWinPopupStars()
                {
                    if (_view == null)
                    {
                        return;
                    }

                    if (_view.winStarsContainer == null)
                    {
                        if (_view.winLeftStarImage != null)
                        {
                            _view.winStarsContainer = _view.winLeftStarImage.rectTransform.parent as RectTransform;
                        }
                        else if (_view.winCenterStarImage != null)
                        {
                            _view.winStarsContainer = _view.winCenterStarImage.rectTransform.parent as RectTransform;
                        }
                        else if (_view.winRightStarImage != null)
                        {
                            _view.winStarsContainer = _view.winRightStarImage.rectTransform.parent as RectTransform;
                        }
                    }

                    if (_view.winStarsText != null && HasWinStarImages())
                    {
                        _view.winStarsText.gameObject.SetActive(false);
                    }

                    ResetWinStarVisuals();
                }

                private bool HasWinStarImages()
                {
                    return _view != null
                        && _view.winStarsContainer != null
                        && _view.winLeftStarImage != null
                        && _view.winCenterStarImage != null
                        && _view.winRightStarImage != null;
                }

                private void ResetWinStarVisuals()
                {
                    if (_view?.winStarsContainer != null)
                    {
                        _view.winStarsContainer.gameObject.SetActive(true);
                    }

                    ResetWinStarVisual(_view?.winLeftStarImage, WinLeftStarAnchoredPosition, -WinStarSideStartRotation);
                    ResetWinStarVisual(_view?.winCenterStarImage, WinCenterStarAnchoredPosition, WinStarCenterStartRotation);
                    ResetWinStarVisual(_view?.winRightStarImage, WinRightStarAnchoredPosition, WinStarSideStartRotation);
                }

                private void ResetWinStarVisual(Image image, Vector2 anchoredPosition, float startAngle)
                {
                    if (image == null)
                    {
                        return;
                    }

                    image.gameObject.SetActive(false);
                    SetWinStarVisualState(image, anchoredPosition + (Vector2.up * WinStarStartOffsetY), 0f, WinStarStartScale, startAngle);
                }

                private void SetWinStarVisualState(Image image, Vector2 anchoredPosition, float alpha, float scale, float angle)
                {
                    if (image == null)
                    {
                        return;
                    }

                    var rectTransform = image.rectTransform;
                    rectTransform.anchoredPosition = anchoredPosition;
                    rectTransform.localScale = Vector3.one * scale;
                    rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

                    var color = image.color;
                    color.a = alpha;
                    image.color = color;
                }

                private float EvaluateWinStarScale(float progress)
                {
                    if (progress < 0.72f)
                    {
                        return Mathf.Lerp(WinStarStartScale, WinStarOvershootScale, progress / 0.72f);
                    }

                    return Mathf.Lerp(WinStarOvershootScale, 1f, (progress - 0.72f) / 0.28f);
                }

                private WinStarRevealStep[] BuildWinStarRevealSteps(int earnedStars)
                {
                    switch (earnedStars)
                    {
                        case 1:
                            return new[]
                            {
                                new WinStarRevealStep
                                {
                                    image = _view.winCenterStarImage,
                                    anchoredPosition = WinCenterStarAnchoredPosition,
                                    startAngle = WinStarCenterStartRotation
                                }
                            };
                        case 2:
                            return new[]
                            {
                                new WinStarRevealStep
                                {
                                    image = _view.winLeftStarImage,
                                    anchoredPosition = WinLeftStarAnchoredPosition,
                                    startAngle = -WinStarSideStartRotation
                                },
                                new WinStarRevealStep
                                {
                                    image = _view.winRightStarImage,
                                    anchoredPosition = WinRightStarAnchoredPosition,
                                    startAngle = WinStarSideStartRotation
                                }
                            };
                        case 3:
                            return new[]
                            {
                                new WinStarRevealStep
                                {
                                    image = _view.winCenterStarImage,
                                    anchoredPosition = WinCenterStarAnchoredPosition,
                                    startAngle = WinStarCenterStartRotation
                                },
                                new WinStarRevealStep
                                {
                                    image = _view.winLeftStarImage,
                                    anchoredPosition = WinLeftStarAnchoredPosition,
                                    startAngle = -WinStarSideStartRotation
                                },
                                new WinStarRevealStep
                                {
                                    image = _view.winRightStarImage,
                                    anchoredPosition = WinRightStarAnchoredPosition,
                                    startAngle = WinStarSideStartRotation
                                }
                            };
                        default:
                            return Array.Empty<WinStarRevealStep>();
                    }
                }

                private void SetWinButtonsInteractable(bool interactable)
                {
                    if (_view.winNextButton != null)
                    {
                        _view.winNextButton.interactable = interactable;
                    }

                    if (_view.winMapButton != null)
                    {
                        _view.winMapButton.interactable = interactable;
                    }

                    if (_view.winReplayButton != null)
                    {
                        _view.winReplayButton.interactable = interactable;
                    }
                }

                private void StopWinStarsAnimation()
                {
                    if (_winStarsCoroutine != null)
                    {
                        StopCoroutine(_winStarsCoroutine);
                        _winStarsCoroutine = null;
                    }

                    SetWinButtonsInteractable(true);
                    ResetWinStarVisuals();
                }

                private void ShowLosePopup()
                {
                    HideAllOverlays();
                    if (_view.loseOverlay != null)
                    {
                        _view.loseOverlay.SetActive(true);
                    }

                    if (_view.loseInfoText != null)
                    {
                        _view.loseInfoText.text = "Moves: " + _board.MovesRemaining + "\nTime: " + _board.TimeRemaining;
                    }
                }

                private void ShowTutorial(string text)
                {
                    HideAllOverlays();
                    if (_view.tutorialOverlay != null)
                    {
                        _view.tutorialOverlay.SetActive(true);
                    }

                    if (_view.tutorialText != null)
                    {
                        _view.tutorialText.text = text;
                    }
                }

                private void ShowToast(string message)
                {
                    ShowToast(message, 1.2f);
                }

                private void ShowToast(string message, float visibleDuration)
                {
                    if (_toastCoroutine != null)
                    {
                        StopCoroutine(_toastCoroutine);
                    }

                    _toastCoroutine = StartCoroutine(ToastRoutine(message, visibleDuration));
                }

                private IEnumerator ToastRoutine(string message, float visibleDuration)
                {
                    if (_view.toastText == null || _view.toastCanvasGroup == null)
                    {
                        yield break;
                    }

                    _view.toastText.text = message;
                    _view.toastCanvasGroup.gameObject.SetActive(true);
                    _view.toastCanvasGroup.alpha = 1f;
                    yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, visibleDuration));

                    var elapsed = 0f;
                    while (elapsed < 0.3f)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        _view.toastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                        yield return null;
                    }

                    HideToastImmediate();
                    _toastCoroutine = null;
                }

                private void HideToastImmediate()
                {
                    if (_view.toastCanvasGroup == null)
                    {
                        return;
                    }

                    _view.toastCanvasGroup.alpha = 0f;
                    _view.toastCanvasGroup.gameObject.SetActive(false);
                }

                private void HideAllOverlays()
                {
                    StopWinStarsAnimation();
                    HideBoosterPreviewImmediate();
                    SetOverlayActive(_view.pauseOverlay, false);
                    SetOverlayActive(_view.shopOverlay, false);
                    SetOverlayActive(_view.winOverlay, false);
                    SetOverlayActive(_view.loseOverlay, false);
                    SetOverlayActive(_view.tutorialOverlay, false);
                }

                private static bool IsOverlayActive(GameObject overlay)
                {
                    return overlay != null && overlay.activeSelf;
                }

                private static void SetOverlayActive(GameObject overlay, bool active)
                {
                    if (overlay != null)
                    {
                        overlay.SetActive(active);
                    }
                }
    }
}
