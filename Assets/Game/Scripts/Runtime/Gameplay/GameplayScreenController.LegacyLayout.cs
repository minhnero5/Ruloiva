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
                private void EnsureLegacyGameplayHud()
                {
                    if (_view == null)
                    {
                        return;
                    }

                    if (_view.legacyTimerBarFillRect == null && _view.legacyTimerBarFillImage != null)
                    {
                        _view.legacyTimerBarFillRect = _view.legacyTimerBarFillImage.rectTransform;
                    }

                    _legacyTimerFillBaseWidth = ResolveLegacyTimerFillBaseWidth(_view.legacyTimerBarFillRect, LegacyTimerFillFallbackWidth);
                }

                internal static float ResolveLegacyTimerFillBaseWidth(RectTransform legacyTimerBarFillRect, float fallbackWidth)
                {
                    if (legacyTimerBarFillRect == null)
                    {
                        return Mathf.Max(0f, fallbackWidth);
                    }

                    var width = legacyTimerBarFillRect.sizeDelta.x;
                    return width > 0f ? width : Mathf.Max(0f, fallbackWidth);
                }

                internal static float CalculateLegacyTimerFillWidth(float baseWidth, int timeRemaining, int timerSeconds)
                {
                    var safeBaseWidth = Mathf.Max(0f, baseWidth);
                    var ratio = timerSeconds > 0 ? Mathf.Clamp01((float)timeRemaining / timerSeconds) : 0f;
                    return safeBaseWidth * ratio;
                }

                private Vector2 GetObjectiveRowPosition(int index, int count)
                {
                    if (count <= 1)
                    {
                        return new Vector2(10f, -18f);
                    }

                    if (count == 2)
                    {
                        return index == 0 ? new Vector2(10f, -18f) : new Vector2(132f, -18f);
                    }

                    if (count == 3)
                    {
                        return index switch
                        {
                            0 => new Vector2(10f, -18f),
                            1 => new Vector2(132f, -18f),
                            _ => new Vector2(10f, -46f)
                        };
                    }

                    return index switch
                    {
                        0 => new Vector2(10f, -18f),
                        1 => new Vector2(132f, -18f),
                        2 => new Vector2(10f, -46f),
                        3 => new Vector2(132f, -46f),
                        _ => new Vector2(10f, -74f)
                    };
                }

                private static void SetAnchoredRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
                {
                    if (rectTransform == null)
                    {
                        return;
                    }

                    rectTransform.anchorMin = anchorMin;
                    rectTransform.anchorMax = anchorMax;
                    rectTransform.pivot = pivot;
                    rectTransform.anchoredPosition = anchoredPosition;
                    rectTransform.sizeDelta = size;
                }

    }
}
