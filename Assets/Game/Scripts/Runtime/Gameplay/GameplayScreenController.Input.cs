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
                private void BindStaticButtons()
                {
                    BindButton(_view.musicButton, ToggleMusic);
                    BindButton(_view.pauseButton, () =>
                    {
                        if (_board == null || _board.EndState != BoardEndState.None)
                        {
                            return;
                        }

                        App.Audio.PlayMarker("bouton");
                        ShowPausePopup();
                        App.Platform.ShowInterstitial();
                    });

                    BindButton(_view.pauseResumeButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        HideAllOverlays();
                    });
                    BindButton(_view.pauseReplayButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadGameplay(_board.Definition.levelNumber);
                    });
                    BindButton(_view.pauseMenuButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadMap();
                    });
                    BindButton(_view.shopCloseButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        HideAllOverlays();
                    });
                    BindButton(_view.winNextButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadGameplay(Mathf.Min(App.Catalog.Levels.Length, _board.Definition.levelNumber + 1));
                    });
                    BindButton(_view.winMapButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadMap();
                    });
                    BindButton(_view.winReplayButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadGameplay(_board.Definition.levelNumber);
                    });
                    BindButton(_view.loseReplayButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadGameplay(_board.Definition.levelNumber);
                    });
                    BindButton(_view.loseMapButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadMap();
                    });
                    BindButton(_view.tutorialOkButton, () =>
                    {
                        App.Audio.PlayMarker("bouton");
                        HideAllOverlays();
                    });

                    foreach (var entry in _boosterBindings)
                    {
                        var boosterType = entry.Key;
                        BindButton(entry.Value.button, () => HandleBoosterPressed(boosterType));
                    }
                }

                private void ToggleMusic()
                {
                    if (_board != null && _board.EndState != BoardEndState.None)
                    {
                        return;
                    }

                    App.Audio.PlayMarker("bouton");
                    var enabled = !App.Save.Profile.musicEnabled;
                    App.Save.SetMusicEnabled(enabled);
                    App.Audio.SetMuted(!enabled);
                    if (enabled)
                    {
                        App.Audio.PlayMusicLoop();
                    }
                    else
                    {
                        App.Audio.StopMusic();
                    }

                    ApplyMusicButtonState(enabled);
                }

                private void HandleBoosterPressed(BoosterType type)
                {
                    if (_board.EndState != BoardEndState.None)
                    {
                        return;
                    }

                    App.Audio.PlayMarker("bouton");
                    if (App.Save.GetBoosterCount(type) <= 0)
                    {
                        ShowShop(type);
                        return;
                    }

                    _activeBooster = _activeBooster == type ? (BoosterType?)null : type;
                    UpdateHud();
                    if (_activeBooster.HasValue)
                    {
                        RefreshBoosterPreviewFromPointer();
                    }
                    else
                    {
                        ClearBoosterPreviewState();
                    }
                }

                private void HandlePointerInput()
                {
                    GetPrimaryPointerState(out var pointerPosition, out var pointerDown, out var pointerHeld, out var pointerUp);

                    if (pointerDown)
                    {
                        if (IsPointerOverUi())
                        {
                            _dragStartCell = null;
                            return;
                        }

                        if (_activeBooster.HasValue)
                        {
                            if (TryGetBoosterTargetCell(pointerPosition, out var boosterCell))
                            {
                                _dragStartCell = boosterCell;
                                _dragStartScreen = pointerPosition;
                                _boosterPreviewCell = boosterCell;
                            }
                            else
                            {
                                _dragStartCell = null;
                                _boosterPreviewCell = null;
                            }

                            UpdateBoosterPreviewVisuals();
                        }
                        else if (TryGetBoardCell(pointerPosition, out var cell))
                        {
                            _dragStartCell = cell;
                            _dragStartScreen = pointerPosition;
                        }
                        else
                        {
                            _dragStartCell = null;
                        }
                    }

                    if (pointerHeld && _dragStartCell.HasValue && _activeBooster == null)
                    {
                        var delta = pointerPosition - _dragStartScreen;
                        if (delta.magnitude > 24f)
                        {
                            var direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                                ? new Vector2Int(delta.x > 0f ? 1 : -1, 0)
                                : new Vector2Int(0, delta.y > 0f ? -1 : 1);
                            var target = _dragStartCell.Value + direction;
                            StartCoroutine(PerformSwap(_dragStartCell.Value, target));
                            _dragStartCell = null;
                        }
                    }

                    if (pointerUp && _dragStartCell.HasValue)
                    {
                        if (_activeBooster.HasValue && _boosterPreviewCell.HasValue)
                        {
                            StartCoroutine(UseBooster(_boosterPreviewCell.Value, _activeBooster.Value));
                        }

                        _dragStartCell = null;
                    }
                }

                private void RefreshBoosterPreviewFromPointer()
                {
                    if (_board == null || !_activeBooster.HasValue)
                    {
                        ClearBoosterPreviewState();
                        return;
                    }

                    GetPrimaryPointerState(out var pointerPosition, out var pointerDown, out var pointerHeld, out _);
                    var pointerOverUi = IsPointerOverUi();
                    var hoveredCell = default(Vector2Int);
                    var hasHoveredCell = !pointerOverUi && TryGetBoosterTargetCell(pointerPosition, out hoveredCell);

                    _boosterPreviewCell = hasHoveredCell ? hoveredCell : (Vector2Int?)null;

                    UpdateBoosterPreviewVisuals();
                }

                private static bool IsPointerOverUi()
                {
                    if (EventSystem.current == null)
                    {
                        return false;
                    }

                    if (Input.touchCount > 0)
                    {
                        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    }

                    return EventSystem.current.IsPointerOverGameObject();
                }

                private static void GetPrimaryPointerState(out Vector2 position, out bool down, out bool held, out bool up)
                {
                    if (Input.touchCount > 0)
                    {
                        var touch = Input.GetTouch(0);
                        position = touch.position;
                        down = touch.phase == TouchPhase.Began;
                        held = touch.phase == TouchPhase.Began
                            || touch.phase == TouchPhase.Moved
                            || touch.phase == TouchPhase.Stationary;
                        up = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                        return;
                    }

                    position = Input.mousePosition;
                    down = Input.GetMouseButtonDown(0);
                    held = Input.GetMouseButton(0);
                    up = Input.GetMouseButtonUp(0);
                }

                private IEnumerator PerformSwap(Vector2Int from, Vector2Int to)
                {
                    if (_inputLocked)
                    {
                        yield break;
                    }

                    _inputLocked = true;
                    var report = _board.TrySwap(from, to);
                    if (report.valid)
                    {
                        if (_board.Definition.levelNumber == 1 && !_levelOneSecondTutorialShown)
                        {
                            _levelOneSecondTutorialShown = true;
                            ShowTutorial("Complete all the objectives\nquickly before you\nrun out of time or moves.");
                        }

                        App.Audio.PlayMarker(report.destroyedCount > 3 ? "great" : "correct");
                    }

                    yield return AnimateBoardReport(report, from, to);
                    HandleEndState(report.endState);
                    if (report.settleGuardTriggered)
                    {
                        yield return new WaitForSecondsRealtime(0.18f);
                    }

                    _inputLocked = _board == null || _board.EndState != BoardEndState.None;
                }

                private IEnumerator UseBooster(Vector2Int cell, BoosterType boosterType)
                {
                    if (_inputLocked)
                    {
                        yield break;
                    }

                    if (!App.Save.TryConsumeBooster(boosterType))
                    {
                        _activeBooster = null;
                        ClearBoosterPreviewState();
                        UpdateHud();
                        yield break;
                    }

                    ClearBoosterPreviewState();
                    _inputLocked = true;
                    App.Audio.PlayMarker("power_up");
                    var report = _board.ApplyBooster(cell, boosterType);
                    if (!report.valid)
                    {
                        App.Save.AddBooster(boosterType, 1);
                    }

                    _activeBooster = null;
                    UpdateHud();
                    yield return AnimateBoardReport(report, null, null);
                    HandleEndState(report.endState);
                    if (report.settleGuardTriggered)
                    {
                        yield return new WaitForSecondsRealtime(0.18f);
                    }

                    _inputLocked = _board == null || _board.EndState != BoardEndState.None;
                }

                private bool TryGetBoardCell(Vector2 screenPosition, out Vector2Int cell)
                {
                    cell = default;
                    var world = Camera.main.ScreenToWorldPoint(screenPosition);
                    var pixelX = (world.x * 100f) + (ReferenceWidth * 0.5f);
                    var pixelY = (ReferenceHeight * 0.5f) - (world.y * 100f);

                    var boardMinX = BoardLeftPixels - (BoardCellSizePixels * 0.5f);
                    var boardMinY = BoardTopPixels - (BoardCellSizePixels * 0.5f);
                    var column = Mathf.FloorToInt((pixelX - boardMinX) / BoardCellSizePixels);
                    var row = Mathf.FloorToInt((pixelY - boardMinY) / BoardCellSizePixels);
                    if (column < 0 || column >= _board.Width || row < 0 || row >= _board.Height)
                    {
                        return false;
                    }

                    var boardCell = _board.GetCell(row, column);
                    if (boardCell == null || !boardCell.exists || boardCell.piece == null)
                    {
                        return false;
                    }

                    cell = new Vector2Int(column, row);
                    return true;
                }

                private bool TryGetBoosterTargetCell(Vector2 screenPosition, out Vector2Int cell)
                {
                    if (!TryGetBoardCell(screenPosition, out cell))
                    {
                        return false;
                    }

                    return _board != null && _board.CanTargetBoosterAt(cell);
                }

                private bool IsBlockingOverlayVisible()
                {
                    return IsOverlayActive(_view.pauseOverlay)
                        || IsOverlayActive(_view.shopOverlay)
                        || IsOverlayActive(_view.winOverlay)
                        || IsOverlayActive(_view.loseOverlay)
                        || IsOverlayActive(_view.tutorialOverlay);
                }

                private BoosterUnlockDefinition GetUnlock(BoosterType boosterType)
                {
                    switch (boosterType)
                    {
                        case BoosterType.Single:
                            return App.Catalog.BoosterUnlocks.single;
                        case BoosterType.Column:
                            return App.Catalog.BoosterUnlocks.column;
                        case BoosterType.Row:
                            return App.Catalog.BoosterUnlocks.row;
                        case BoosterType.Cross:
                            return App.Catalog.BoosterUnlocks.cross;
                        case BoosterType.Area:
                            return App.Catalog.BoosterUnlocks.area;
                        default:
                            return App.Catalog.BoosterUnlocks.single;
                    }
                }
    }
}
