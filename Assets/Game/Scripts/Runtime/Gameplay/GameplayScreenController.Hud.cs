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
                private void RebuildObjectives()
                {
                    ClearObjectiveRows();
                    if (_view.objectiveRowTemplate == null || _view.objectiveContainer == null)
                    {
                        return;
                    }

                    for (var index = 0; index < _board.Objectives.Length; index++)
                    {
                        var rowView = Instantiate(_view.objectiveRowTemplate, _view.objectiveContainer);
                        rowView.gameObject.name = "ObjectiveRow" + index;
                        rowView.gameObject.SetActive(true);
                        var position = GetObjectiveRowPosition(index, _board.Objectives.Length);
                        rowView.rectTransform.anchorMin = new Vector2(0f, 1f);
                        rowView.rectTransform.anchorMax = new Vector2(0f, 1f);
                        rowView.rectTransform.pivot = new Vector2(0f, 1f);
                        rowView.rectTransform.anchoredPosition = position;

                        _spawnedObjectiveRows.Add(rowView);
                    }

                    _view.objectiveRowTemplate.gameObject.SetActive(false);
                }

                private void ClearObjectiveRows()
                {
                    foreach (var row in _spawnedObjectiveRows)
                    {
                        if (row != null)
                        {
                            Destroy(row.gameObject);
                        }
                    }

                    _spawnedObjectiveRows.Clear();
                }

                private void BuildBoardVisuals()
                {
                    _pieceVisuals.Clear();
                    _cellVisuals = new CellVisual[_board.Height, _board.Width];
                    SyncBoardVisuals(_board.CreateVisualSnapshot());
                }

                private IEnumerator TimerLoop()
                {
                    while (_board != null && _board.EndState == BoardEndState.None)
                    {
                        yield return new WaitForSecondsRealtime(1f);
                        if (_inputLocked || IsBlockingOverlayVisible())
                        {
                            continue;
                        }

                        _board.TickSecond();
                        if (_board.TimeRemaining == 10)
                        {
                            App.Audio.PlayClip("JellyQuest/Raw/assets/music/end_timer", 0.6f);
                        }

                        UpdateHud();
                        HandleEndState(_board.EndState);
                    }
                }

                private void HandleEndState(BoardEndState endState)
                {
                    if (endState == BoardEndState.None || _sessionPersisted)
                    {
                        return;
                    }

                    if (endState == BoardEndState.WinPending || endState == BoardEndState.LosePending)
                    {
                        if (_pendingEndCoroutine == null)
                        {
                            _pendingEndCoroutine = StartCoroutine(FinalizePendingEndStateRoutine(_board.PendingEndReason));
                        }

                        return;
                    }

                    _sessionPersisted = true;
                    if (endState == BoardEndState.Win)
                    {
                        App.Audio.PlayClip("JellyQuest/Raw/assets/music/win", 1f);
                        App.Save.AddCoins(_board.EarnedCoins);
                        App.Save.SetStars(_board.Definition.levelNumber, _board.CurrentStars);
                        App.Save.UnlockNextLevel(_board.Definition.levelNumber);
                        UpdateHud();
                        ShowWinPopup();
                    }
                    else
                    {
                        App.Platform.ShowInterstitial();
                        ShowLosePopup();
                    }
                }

                private IEnumerator FinalizePendingEndStateRoutine(BoardPendingEndReason pendingReason)
                {
                    switch (pendingReason)
                    {
                        case BoardPendingEndReason.ObjectivesCompleted:
                            ShowToast("Goals completed!", 2.3f);
                            yield return new WaitForSecondsRealtime(3f);
                            break;
                        case BoardPendingEndReason.TimeExpired:
                            ShowToast("TIME'S UP", 2.3f);
                            yield return new WaitForSecondsRealtime(3f);
                            break;
                        case BoardPendingEndReason.NoMovesRemaining:
                            yield return new WaitForSecondsRealtime(0.5f);
                            break;
                        default:
                            yield return new WaitForSecondsRealtime(0.2f);
                            break;
                    }

                    _pendingEndCoroutine = null;
                    if (_board != null && _board.FinalizePendingEndState())
                    {
                        _inputLocked = false;
                        UpdateHud();
                        HandleEndState(_board.EndState);
                    }
                }

                private void UpdateHud(BoardHudSnapshot hud = null)
                {
                    if (_board == null)
                    {
                        return;
                    }

                    hud = hud ?? _board.CreateHudSnapshot();
                    _view.movesText.text = hud.movesRemaining.ToString();
                    _view.scoreText.text = hud.score.ToString();
                    _view.timerText.text = hud.timeRemaining.ToString();
                    _view.coinsText.text = App.Save.Profile.coins.ToString();
                    _view.levelText.text = "level " + _board.Definition.levelNumber;

                    if (_view.legacyTimerBarFillRect != null)
                    {
                        if (_legacyTimerFillBaseWidth <= 0f)
                        {
                            _legacyTimerFillBaseWidth = ResolveLegacyTimerFillBaseWidth(_view.legacyTimerBarFillRect, LegacyTimerFillFallbackWidth);
                        }

                        var size = _view.legacyTimerBarFillRect.sizeDelta;
                        size.x = CalculateLegacyTimerFillWidth(_legacyTimerFillBaseWidth, hud.timeRemaining, _board.Definition.timerSeconds);
                        _view.legacyTimerBarFillRect.sizeDelta = size;
                    }

                    for (var index = 0; index < _spawnedObjectiveRows.Count; index++)
                    {
                        if (index >= hud.objectives.Length)
                        {
                            continue;
                        }

                        var objective = hud.objectives[index];
                        var row = _spawnedObjectiveRows[index];
                        row.iconImage.sprite = GetObjectiveSprite(objective.type);
                        row.progressText.text = objective.currentCount + "/" + objective.targetCount;
                    }

                    foreach (var entry in _boosterBindings)
                    {
                        var count = App.Save.GetBoosterCount(entry.Key);
                        if (entry.Value.countText != null)
                        {
                            entry.Value.countText.text = count.ToString();
                            entry.Value.countText.enabled = true;
                        }

                        if (entry.Value.image != null)
                        {
                            var isActive = _activeBooster == entry.Key;
                            entry.Value.image.sprite = GetBoosterSprite(entry.Key, true);
                            entry.Value.image.color = isActive ? GetActiveBoosterButtonColor() : Color.white;
                        }

                        if (entry.Value.button != null)
                        {
                            entry.Value.button.transform.localScale = _activeBooster == entry.Key
                                ? ActiveBoosterButtonScale
                                : Vector3.one;
                        }
                    }

                    if (_view.musicButtonImage != null)
                    {
                        ApplyMusicButtonState(App.Save.Profile.musicEnabled);
                    }
                }

                private void ApplyHudSnapshot(BoardHudSnapshot hud)
                {
                    UpdateHud(hud);
                }

                private Sprite GetObjectiveSprite(ObjectiveType type)
                {
                    return LegacyUiSpriteLookup.GetObjectiveSprite(App.UiSprites, App.Sprites, type);
                }

                private Sprite GetBoosterSprite(BoosterType boosterType, bool unlocked)
                {
                    return LegacyUiSpriteLookup.GetBoosterSprite(App.UiSprites, App.Sprites, boosterType, unlocked);
                }

                private void ApplyMusicButtonState(bool enabled)
                {
                    if (_view.musicButtonImage == null)
                    {
                        return;
                    }

                    LegacyUiSpriteSceneApplier.ApplyMusicButtonState(
                        _view.musicButtonImage,
                        App.UiSprites,
                        App.Sprites,
                        enabled);
                }

                private Sprite GetGameplayMovesPanelSprite()
                {
                    return LegacyUiSpriteLookup.GetGameplayMovesPanel(App.UiSprites, App.Sprites);
                }

                private Sprite GetGameplayObjectivePanelSprite()
                {
                    return LegacyUiSpriteLookup.GetGameplayObjectivePanel(App.UiSprites, App.Sprites);
                }

                private Sprite GetGameplayObjectiveStripSprite()
                {
                    return LegacyUiSpriteLookup.GetGameplayObjectiveStrip(App.UiSprites, App.Sprites);
                }

                private Sprite GetGameplayTimerBarBackgroundSprite()
                {
                    return LegacyUiSpriteLookup.GetGameplayTimerBarBackground(App.UiSprites, App.Sprites);
                }

                private Sprite GetGameplayTimerBarFillSprite()
                {
                    return LegacyUiSpriteLookup.GetGameplayTimerBarFill(App.UiSprites, App.Sprites);
                }

                private Sprite GetGameplayTimerClockSprite()
                {
                    return LegacyUiSpriteLookup.GetGameplayTimerClock(App.UiSprites, App.Sprites);
                }

                private Sprite GetPauseButtonSprite()
                {
                    return LegacyUiSpriteLookup.GetPauseButton(App.UiSprites, App.Sprites);
                }

                private Sprite GetResumeButtonSprite()
                {
                    return LegacyUiSpriteLookup.GetResumeButton(App.UiSprites, App.Sprites);
                }

                private Sprite GetReplayButtonSprite()
                {
                    return LegacyUiSpriteLookup.GetReplayButton(App.UiSprites, App.Sprites);
                }

                private Sprite GetMenuButtonSprite()
                {
                    return LegacyUiSpriteLookup.GetMenuButton(App.UiSprites, App.Sprites);
                }

                private Sprite GetBuyButtonSprite()
                {
                    return LegacyUiSpriteLookup.GetBuyButton(App.UiSprites, App.Sprites);
                }

                private Sprite GetCloseSmallButtonSprite()
                {
                    return LegacyUiSpriteLookup.GetCloseSmallButton(App.UiSprites, App.Sprites);
                }

                private static void SetButtonSprite(Button button, Sprite sprite, bool preserveAspect)
                {
                    if (button == null)
                    {
                        return;
                    }

                    var image = button.GetComponent<Image>();
                    if (image == null)
                    {
                        return;
                    }

                    SetImageSprite(image, sprite, preserveAspect);
                    button.targetGraphic = image;
                }

                private static void SetImageSprite(Image image, Sprite sprite, bool preserveAspect)
                {
                    if (image == null || sprite == null)
                    {
                        return;
                    }

                    image.sprite = sprite;
                    image.preserveAspect = preserveAspect;
                }

                private static Image FindOverlayPanelImage(GameObject overlay)
                {
                    if (overlay == null)
                    {
                        return null;
                    }

                    var panel = overlay.transform.Find("Panel");
                    return panel != null ? panel.GetComponent<Image>() : null;
                }
    }
}
