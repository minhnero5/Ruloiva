using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
    public sealed partial class GameplayScreenController
    {
                private IEnumerator AnimateBoardReport(BoardActionReport report, Vector2Int? attemptedFrom, Vector2Int? attemptedTo)
                {
                    if (_board == null)
                    {
                        yield break;
                    }

                    if (!report.valid)
                    {
                        if (attemptedFrom.HasValue && attemptedTo.HasValue)
                        {
                            yield return AnimateInvalidSwap(attemptedFrom.Value, attemptedTo.Value);
                        }

                        UpdateHud();
                        yield break;
                    }

                    if (report.phases.Count == 0)
                    {
                        SyncBoardVisuals(_board.CreateVisualSnapshot());
                        UpdateHud();
                        yield break;
                    }

                    for (var index = 0; index < report.phases.Count; index++)
                    {
                        var phase = report.phases[index];
                        switch (phase.kind)
                        {
                            case BoardVisualPhaseKind.Swap:
                            case BoardVisualPhaseKind.Settle:
                            case BoardVisualPhaseKind.Shuffle:
                                yield return AnimateMovePhase(phase.snapshot, phase.suggestedDuration);
                                break;
                            case BoardVisualPhaseKind.Clear:
                                yield return AnimateClearPhase(phase);
                                break;
                            case BoardVisualPhaseKind.Fill:
                                yield return AnimateFillPhase(phase.snapshot, phase.suggestedDuration);
                                break;
                            default:
                                SyncBoardVisuals(phase.snapshot);
                                break;
                        }

                        ApplyHudSnapshot(phase.hud);
                        if (index < report.phases.Count - 1)
                        {
                            yield return new WaitForSecondsRealtime(CascadePauseDuration);
                        }
                    }

                    SyncBoardVisuals(_board.CreateVisualSnapshot());
                    UpdateHud();
                }

                private IEnumerator AnimateInvalidSwap(Vector2Int from, Vector2Int to)
                {
                    if (!TryGetPieceVisualAtCell(from, out var sourceVisual))
                    {
                        yield break;
                    }

                    var sourceStart = sourceVisual.transform.localPosition;
                    if (IsBoardCoordinate(to) && TryGetPieceVisualAtCell(to, out var targetVisual))
                    {
                        var targetCell = _board.GetCell(to.y, to.x);
                        if (targetCell != null && targetCell.piece != null && targetCell.piece.CanBeSelected)
                        {
                            var targetStart = targetVisual.transform.localPosition;
                            var forwardMoves = new List<PieceMoveAnimation>
                            {
                                new PieceMoveAnimation { visual = sourceVisual, from = sourceStart, to = targetStart },
                                new PieceMoveAnimation { visual = targetVisual, from = targetStart, to = sourceStart }
                            };
                            yield return AnimatePieceMoves(forwardMoves, InvalidSwapDuration);

                            var backwardMoves = new List<PieceMoveAnimation>
                            {
                                new PieceMoveAnimation { visual = sourceVisual, from = targetStart, to = sourceStart },
                                new PieceMoveAnimation { visual = targetVisual, from = sourceStart, to = targetStart }
                            };
                            yield return AnimatePieceMoves(backwardMoves, InvalidSwapDuration);
                            sourceVisual.transform.localPosition = sourceStart;
                            targetVisual.transform.localPosition = targetStart;
                            yield break;
                        }
                    }

                    var offset = GetInvalidSwapOffset(from, to);
                    var forward = new List<PieceMoveAnimation>
                    {
                        new PieceMoveAnimation { visual = sourceVisual, from = sourceStart, to = sourceStart + offset }
                    };
                    yield return AnimatePieceMoves(forward, InvalidSwapDuration);

                    var backward = new List<PieceMoveAnimation>
                    {
                        new PieceMoveAnimation { visual = sourceVisual, from = sourceStart + offset, to = sourceStart }
                    };
                    yield return AnimatePieceMoves(backward, InvalidSwapDuration);
                    sourceVisual.transform.localPosition = sourceStart;
                }

                private IEnumerator AnimateMovePhase(BoardVisualSnapshot nextSnapshot, float duration)
                {
                    if (_currentSnapshot == null || nextSnapshot == null)
                    {
                        SyncBoardVisuals(nextSnapshot);
                        yield break;
                    }

                    UpdateCellVisuals(nextSnapshot);
                    var moves = new List<PieceMoveAnimation>();
                    for (var row = 0; row < nextSnapshot.Height; row++)
                    {
                        for (var column = 0; column < nextSnapshot.Width; column++)
                        {
                            var cell = nextSnapshot.GetCell(row, column);
                            if (cell == null || cell.pieceRuntimeId == 0)
                            {
                                continue;
                            }

                            if (!_pieceVisuals.TryGetValue(cell.pieceRuntimeId, out var pieceVisual))
                            {
                                SyncBoardVisuals(nextSnapshot);
                                yield break;
                            }

                            var coordinate = new Vector2Int(column, row);
                            pieceVisual.coordinate = coordinate;
                            UpdatePieceVisualState(pieceVisual, cell);
                            var targetPosition = GetBoardPosition(coordinate, PieceSortingOrder);
                            if ((pieceVisual.transform.localPosition - targetPosition).sqrMagnitude > 0.0001f)
                            {
                                moves.Add(new PieceMoveAnimation
                                {
                                    visual = pieceVisual,
                                    from = pieceVisual.transform.localPosition,
                                    to = targetPosition
                                });
                            }
                        }
                    }

                    yield return AnimatePieceMoves(moves, duration);
                    SyncBoardVisuals(nextSnapshot);
                }

                private IEnumerator AnimateClearPhase(BoardVisualPhase phase)
                {
                    var nextSnapshot = phase != null ? phase.snapshot : null;
                    var duration = phase != null ? phase.suggestedDuration : 0f;
                    if (_currentSnapshot == null || nextSnapshot == null)
                    {
                        SyncBoardVisuals(nextSnapshot);
                        yield break;
                    }

                    StartScorePopups(phase.scorePopups);
                    UpdateCellVisuals(nextSnapshot);
                    var removedIds = new HashSet<int>();
                    for (var row = 0; row < _currentSnapshot.Height; row++)
                    {
                        for (var column = 0; column < _currentSnapshot.Width; column++)
                        {
                            var previousCell = _currentSnapshot.GetCell(row, column);
                            if (previousCell == null || previousCell.pieceRuntimeId == 0)
                            {
                                continue;
                            }

                            if (!nextSnapshot.TryFindPiece(previousCell.pieceRuntimeId, out _, out _))
                            {
                                removedIds.Add(previousCell.pieceRuntimeId);
                            }
                        }
                    }

                    for (var row = 0; row < nextSnapshot.Height; row++)
                    {
                        for (var column = 0; column < nextSnapshot.Width; column++)
                        {
                            var nextCell = nextSnapshot.GetCell(row, column);
                            if (nextCell == null || nextCell.pieceRuntimeId == 0)
                            {
                                continue;
                            }

                            if (!_pieceVisuals.TryGetValue(nextCell.pieceRuntimeId, out var pieceVisual))
                            {
                                SyncBoardVisuals(nextSnapshot);
                                yield break;
                            }

                            pieceVisual.coordinate = new Vector2Int(column, row);
                            UpdatePieceVisualState(pieceVisual, nextCell);
                        }
                    }

                    if (removedIds.Count == 0)
                    {
                        if (duration > 0f)
                        {
                            yield return new WaitForSecondsRealtime(duration);
                        }

                        SyncBoardVisuals(nextSnapshot);
                        yield break;
                    }

                    var removedVisuals = new List<PieceVisual>();
                    foreach (var runtimeId in removedIds)
                    {
                        if (_pieceVisuals.TryGetValue(runtimeId, out var pieceVisual))
                        {
                            removedVisuals.Add(pieceVisual);
                        }
                    }

                    if (removedVisuals.Count == 0)
                    {
                        SyncBoardVisuals(nextSnapshot);
                        yield break;
                    }

                    var destroyEffects = new List<DestroyEffectVisual>(removedVisuals.Count);
                    foreach (var pieceVisual in removedVisuals)
                    {
                        var effectVisual = CreateDestroyEffectVisual(pieceVisual.transform.localPosition);
                        if (effectVisual != null)
                        {
                            destroyEffects.Add(effectVisual);
                        }
                    }

                    duration = Mathf.Max(0.01f, Mathf.Max(duration, DestroyEffectDuration));
                    var elapsed = 0f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        var t = Mathf.Clamp01(elapsed / duration);
                        var eased = Mathf.SmoothStep(0f, 1f, t);
                        var alpha = 1f - t;
                        var scale = Vector3.Lerp(Vector3.one, Vector3.one * 1.18f, eased);
                        var effectProgress = Mathf.Clamp01(elapsed / DestroyEffectDuration);
                        var effectFrame = Mathf.Min(DestroyEffectFrameCount - 1, Mathf.FloorToInt(effectProgress * DestroyEffectFrameCount));
                        foreach (var pieceVisual in removedVisuals)
                        {
                            pieceVisual.transform.localScale = Vector3.Scale(pieceVisual.baseLocalScale, scale);
                            SetPieceVisualAlpha(pieceVisual, alpha);
                        }

                        foreach (var effectVisual in destroyEffects)
                        {
                            SetDestroyEffectFrame(effectVisual, effectFrame);
                        }

                        yield return null;
                    }

                    foreach (var effectVisual in destroyEffects)
                    {
                        DestroyDestroyEffectVisual(effectVisual);
                    }

                    foreach (var pieceVisual in removedVisuals)
                    {
                        DestroyPieceVisual(pieceVisual.runtimeId);
                    }

                    SyncBoardVisuals(nextSnapshot);
                }

                private IEnumerator AnimateFillPhase(BoardVisualSnapshot nextSnapshot, float duration)
                {
                    if (_currentSnapshot == null || nextSnapshot == null)
                    {
                        SyncBoardVisuals(nextSnapshot);
                        yield break;
                    }

                    UpdateCellVisuals(nextSnapshot);
                    var newPiecesByColumn = new Dictionary<int, List<Vector2Int>>();
                    for (var row = 0; row < nextSnapshot.Height; row++)
                    {
                        for (var column = 0; column < nextSnapshot.Width; column++)
                        {
                            var cell = nextSnapshot.GetCell(row, column);
                            if (cell == null || cell.pieceRuntimeId == 0)
                            {
                                continue;
                            }

                            var coordinate = new Vector2Int(column, row);
                            if (_currentSnapshot.TryFindPiece(cell.pieceRuntimeId, out _, out _))
                            {
                                if (!_pieceVisuals.TryGetValue(cell.pieceRuntimeId, out var existingVisual))
                                {
                                    SyncBoardVisuals(nextSnapshot);
                                    yield break;
                                }

                                existingVisual.coordinate = coordinate;
                                UpdatePieceVisualState(existingVisual, cell);

                                continue;
                            }

                            if (!newPiecesByColumn.TryGetValue(column, out var list))
                            {
                                list = new List<Vector2Int>();
                                newPiecesByColumn[column] = list;
                            }

                            list.Add(coordinate);
                        }
                    }

                    var moves = new List<PieceMoveAnimation>();
                    foreach (var entry in newPiecesByColumn)
                    {
                        entry.Value.Sort((left, right) => left.y.CompareTo(right.y));
                        for (var index = 0; index < entry.Value.Count; index++)
                        {
                            var coordinate = entry.Value[index];
                            var cell = nextSnapshot.GetCell(coordinate.y, coordinate.x);
                            var spawnPosition = GetBoardPosition(entry.Key, -(index + 1), PieceSortingOrder);
                            var targetPosition = GetBoardPosition(coordinate, PieceSortingOrder);
                            var pieceVisual = CreatePieceVisual(cell.pieceRuntimeId, cell, coordinate, spawnPosition);
                            moves.Add(new PieceMoveAnimation
                            {
                                visual = pieceVisual,
                                from = spawnPosition,
                                to = targetPosition
                            });
                        }
                    }

                    yield return AnimatePieceMoves(moves, duration);
                    SyncBoardVisuals(nextSnapshot);
                }

                private IEnumerator AnimatePieceMoves(List<PieceMoveAnimation> moves, float duration)
                {
                    if (moves == null || moves.Count == 0)
                    {
                        yield break;
                    }

                    duration = Mathf.Max(0.01f, duration);
                    var elapsed = 0f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        var t = Mathf.Clamp01(elapsed / duration);
                        var eased = Mathf.SmoothStep(0f, 1f, t);
                        foreach (var move in moves)
                        {
                            move.visual.transform.localPosition = Vector3.LerpUnclamped(move.from, move.to, eased);
                        }

                        yield return null;
                    }

                    foreach (var move in moves)
                    {
                        move.visual.transform.localPosition = move.to;
                    }
                }

                private void SyncBoardVisuals(BoardVisualSnapshot snapshot)
                {
                    if (snapshot == null)
                    {
                        return;
                    }

                    EnsureCellVisualStorage();
                    UpdateCellVisuals(snapshot);
                    var activeRuntimeIds = new HashSet<int>();
                    for (var row = 0; row < snapshot.Height; row++)
                    {
                        for (var column = 0; column < snapshot.Width; column++)
                        {
                            var cell = snapshot.GetCell(row, column);
                            if (cell == null || cell.pieceRuntimeId == 0)
                            {
                                continue;
                            }

                            activeRuntimeIds.Add(cell.pieceRuntimeId);
                            var coordinate = new Vector2Int(column, row);
                            if (!_pieceVisuals.TryGetValue(cell.pieceRuntimeId, out var pieceVisual))
                            {
                                pieceVisual = CreatePieceVisual(cell.pieceRuntimeId, cell, coordinate, GetBoardPosition(coordinate, PieceSortingOrder));
                            }

                            pieceVisual.coordinate = coordinate;
                            pieceVisual.transform.localPosition = GetBoardPosition(coordinate, PieceSortingOrder);
                            pieceVisual.transform.localScale = pieceVisual.baseLocalScale;
                            UpdatePieceVisualState(pieceVisual, cell);
                            SetPieceVisualAlpha(pieceVisual, 1f);
                        }
                    }

                    var staleRuntimeIds = new List<int>();
                    foreach (var entry in _pieceVisuals)
                    {
                        if (!activeRuntimeIds.Contains(entry.Key))
                        {
                            staleRuntimeIds.Add(entry.Key);
                        }
                    }

                    foreach (var runtimeId in staleRuntimeIds)
                    {
                        DestroyPieceVisual(runtimeId);
                    }

                    _currentSnapshot = snapshot;
                }

                private void EnsureCellVisualStorage()
                {
                    if (_board == null)
                    {
                        return;
                    }

                    if (_cellVisuals == null || _cellVisuals.GetLength(0) != _board.Height || _cellVisuals.GetLength(1) != _board.Width)
                    {
                        _cellVisuals = new CellVisual[_board.Height, _board.Width];
                    }
                }

                private void UpdateCellVisuals(BoardVisualSnapshot snapshot)
                {
                    EnsureCellVisualStorage();
                    for (var row = 0; row < snapshot.Height; row++)
                    {
                        for (var column = 0; column < snapshot.Width; column++)
                        {
                            var cell = snapshot.GetCell(row, column);
                            var isPlayable = cell != null && cell.exists;
                            var hasIce = cell != null && cell.exists && cell.hasIce;
                            SetCellGridVisual(row, column, isPlayable);
                            SetCellIceVisual(row, column, hasIce);
                        }
                    }
                }

                private void SetCellGridVisual(int row, int column, bool isPlayable)
                {
                    if (_cellVisuals[row, column] == null)
                    {
                        _cellVisuals[row, column] = new CellVisual();
                    }

                    var cellVisual = _cellVisuals[row, column];
                    var gridSprite = isPlayable ? GetGridSpriteForCell(row, column) : null;
                    if (!isPlayable || gridSprite == null)
                    {
                        if (cellVisual.gridRenderer != null)
                        {
                            Destroy(cellVisual.gridRenderer.gameObject);
                            cellVisual.gridRenderer = null;
                        }

                        return;
                    }

                    if (cellVisual.gridRenderer == null)
                    {
                        if (_gridTileTemplate == null)
                        {
                            throw new InvalidOperationException("Gameplay grid tile template is not available.");
                        }

                        var gridObject = Instantiate(_gridTileTemplate, _boardLayer.transform, false);
                        gridObject.name = "Grid";
                        cellVisual.gridRenderer = gridObject.GetComponent<SpriteRenderer>();
                    }

                    cellVisual.gridRenderer.sprite = gridSprite;
                    cellVisual.gridRenderer.sortingOrder = GridSortingOrder;
                    cellVisual.gridRenderer.transform.localPosition = GetBoardPosition(column, row, GridSortingOrder);
                    cellVisual.gridRenderer.transform.localScale = Vector3.one;
                    cellVisual.gridRenderer.color = Color.white;
                }

                private Sprite GetGridSpriteForCell(int row, int column)
                {
                    var maskValue = _board != null && _board.Definition != null
                        ? _board.Definition.GetBackgroundMaskValue(row, column)
                        : 0;
                    return LegacyUiSpriteLookup.GetGameplayBoardTile(App.UiSprites, App.Sprites, maskValue);
                }

                private Sprite GetBoosterPreviewSpriteForCell(int row, int column)
                {
                    var maskValue = _board != null && _board.Definition != null
                        ? _board.Definition.GetBackgroundMaskValue(row, column)
                        : 0;
                    return LegacyUiSpriteLookup.GetGameplayBoosterPreviewSprite(App.UiSprites, App.Sprites, maskValue);
                }

                private void SetCellIceVisual(int row, int column, bool hasIce)
                {
                    if (_cellVisuals[row, column] == null)
                    {
                        _cellVisuals[row, column] = new CellVisual();
                    }

                    var cellVisual = _cellVisuals[row, column];
                    if (!hasIce || _iceSprite == null)
                    {
                        if (cellVisual.iceRenderer != null)
                        {
                            Destroy(cellVisual.iceRenderer.gameObject);
                            cellVisual.iceRenderer = null;
                        }

                        return;
                    }

                    if (cellVisual.iceRenderer == null)
                    {
                        var iceObject = new GameObject("Ice", typeof(SpriteRenderer));
                        iceObject.transform.SetParent(_boardLayer.transform, false);
                        cellVisual.iceRenderer = iceObject.GetComponent<SpriteRenderer>();
                        cellVisual.iceRenderer.sortingOrder = IceSortingOrder;
                    }

                    cellVisual.iceRenderer.sprite = _iceSprite;
                    cellVisual.iceRenderer.transform.localPosition = GetBoardPosition(column, row, IceSortingOrder);
                    cellVisual.iceRenderer.transform.localScale = IceScale;
                }

                private void UpdateBoosterPreviewVisuals()
                {
                    if (_board == null || !_activeBooster.HasValue)
                    {
                        HideBoosterPreviewImmediate();
                        return;
                    }

                    EnsureCellVisualStorage();
                    HashSet<Vector2Int> targetedCells = null;
                    if (_boosterPreviewCell.HasValue)
                    {
                        targetedCells = new HashSet<Vector2Int>(_board.GetBoosterTargets(_boosterPreviewCell.Value, _activeBooster.Value));
                    }

                    var pulse = 0.5f + (0.5f * Mathf.Sin(Time.unscaledTime * BoosterPreviewPulseSpeed));
                    for (var row = 0; row < _board.Height; row++)
                    {
                        for (var column = 0; column < _board.Width; column++)
                        {
                            var coordinate = new Vector2Int(column, row);
                            var showHint = _board.CanTargetBoosterAt(coordinate);
                            var isTargeted = targetedCells != null && targetedCells.Contains(coordinate);
                            SetCellBoosterPreviewVisual(row, column, showHint, isTargeted, pulse);
                        }
                    }
                }

                private void ClearBoosterPreviewState()
                {
                    _boosterPreviewCell = null;
                    HideBoosterPreviewImmediate();
                }

                private void HideBoosterPreviewImmediate()
                {
                    if (_cellVisuals == null)
                    {
                        return;
                    }

                    for (var row = 0; row < _cellVisuals.GetLength(0); row++)
                    {
                        for (var column = 0; column < _cellVisuals.GetLength(1); column++)
                        {
                            var cellVisual = _cellVisuals[row, column];
                            if (cellVisual?.boosterPreviewRenderer == null)
                            {
                                continue;
                            }

                            cellVisual.boosterPreviewRenderer.enabled = false;
                            cellVisual.boosterPreviewRenderer.transform.localScale = Vector3.one;
                        }
                    }
                }

                private void SetCellBoosterPreviewVisual(int row, int column, bool showHint, bool isTargeted, float pulse)
                {
                    if (_cellVisuals[row, column] == null)
                    {
                        _cellVisuals[row, column] = new CellVisual();
                    }

                    var cellVisual = _cellVisuals[row, column];
                    if (!showHint && !isTargeted)
                    {
                        if (cellVisual.boosterPreviewRenderer != null)
                        {
                            cellVisual.boosterPreviewRenderer.enabled = false;
                        }

                        return;
                    }

                    var previewSprite = GetBoosterPreviewSpriteForCell(row, column);
                    if (previewSprite == null)
                    {
                        if (cellVisual.boosterPreviewRenderer != null)
                        {
                            cellVisual.boosterPreviewRenderer.enabled = false;
                        }

                        return;
                    }

                    if (cellVisual.boosterPreviewRenderer == null)
                    {
                        if (_gridTileTemplate == null)
                        {
                            throw new InvalidOperationException("Gameplay grid tile template is not available.");
                        }

                        var previewParent = _boosterPreviewLayer != null ? _boosterPreviewLayer.transform : _boardLayer.transform;
                        var previewObject = Instantiate(_gridTileTemplate, previewParent, false);
                        previewObject.name = "BoosterPreview";
                        cellVisual.boosterPreviewRenderer = previewObject.GetComponent<SpriteRenderer>();
                    }

                    var renderer = cellVisual.boosterPreviewRenderer;
                    renderer.enabled = true;
                    renderer.sprite = previewSprite;
                    renderer.sortingOrder = BoosterPreviewSortingOrder;
                    renderer.transform.localPosition = GetBoardPosition(column, row, BoosterPreviewSortingOrder);

                    var alpha = isTargeted
                        ? Mathf.Lerp(0.58f, 0.78f, pulse)
                        : Mathf.Lerp(0.2f, 0.34f, pulse);
                    var color = isTargeted ? GetBoosterPreviewTargetColor() : GetBoosterPreviewHintColor();
                    color.a = alpha;
                    renderer.color = color;
                    renderer.transform.localScale = Vector3.one * (isTargeted
                        ? Mathf.Lerp(1.04f, 1.1f, pulse)
                        : Mathf.Lerp(1.01f, 1.04f, pulse));
                }

                private PieceVisual CreatePieceVisual(int runtimeId, BoardVisualCellSnapshot cell, Vector2Int coordinate, Vector3 localPosition)
                {
                    if (_pieceTemplate == null)
                    {
                        throw new InvalidOperationException("Gameplay piece template is not available.");
                    }

                    var pieceObject = Instantiate(_pieceTemplate, _boardLayer.transform, false);
                    pieceObject.name = "Piece" + runtimeId;
                    pieceObject.transform.localPosition = localPosition;
                    pieceObject.SetActive(true);

                    var pieceView = pieceObject.GetComponent<GameplayPieceView>();
                    if (pieceView == null
                        || pieceView.rootRenderer == null
                        || pieceView.label == null
                        || pieceView.labelRenderer == null
                        || pieceView.lockBars == null
                        || pieceView.lockBars.Length != 3)
                    {
                        Destroy(pieceObject);
                        throw new InvalidOperationException("Gameplay piece template instance is missing required components.");
                    }

                    var pieceVisual = new PieceVisual
                    {
                        runtimeId = runtimeId,
                        coordinate = coordinate,
                        gameObject = pieceObject,
                        transform = pieceObject.transform,
                        baseLocalScale = pieceObject.transform.localScale,
                        renderer = pieceView.rootRenderer,
                        lockBars = pieceView.lockBars,
                        label = pieceView.label,
                        labelRenderer = pieceView.labelRenderer
                    };

                    _pieceVisuals[runtimeId] = pieceVisual;
                    UpdatePieceVisualState(pieceVisual, cell);
                    return pieceVisual;
                }

                private DestroyEffectVisual CreateDestroyEffectVisual(Vector3 localPosition)
                {
                    var sprite = App.Sprites.GetDestroyEffectSprite(0);
                    if (sprite == null)
                    {
                        return null;
                    }

                    var effectObject = new GameObject(DestroyEffectResourceName, typeof(SpriteRenderer));
                    effectObject.transform.SetParent(_boardLayer.transform, false);
                    effectObject.transform.localPosition = localPosition;
                    effectObject.transform.localScale = DestroyEffectScale;

                    var renderer = effectObject.GetComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = DestroyEffectSortingOrder;

                    return new DestroyEffectVisual
                    {
                        gameObject = effectObject,
                        renderer = renderer
                    };
                }

                private void StartScorePopups(List<BoardScorePopupInfo> scorePopups)
                {
                    if (scorePopups == null || scorePopups.Count == 0)
                    {
                        return;
                    }

                    foreach (var popupInfo in scorePopups)
                    {
                        if (popupInfo == null || popupInfo.scoreDelta <= 0)
                        {
                            continue;
                        }

                        if (!_pieceVisuals.TryGetValue(popupInfo.runtimeId, out var pieceVisual) || pieceVisual?.transform == null)
                        {
                            continue;
                        }

                        var popupVisual = CreateScorePopupVisual("+" + popupInfo.scoreDelta, pieceVisual.transform.localPosition);
                        if (popupVisual != null)
                        {
                            StartCoroutine(AnimateScorePopup(popupVisual));
                        }
                    }
                }

                private ScorePopupVisual CreateScorePopupVisual(string text, Vector3 localPosition)
                {
                    if (_boardLayer == null || _pieceTemplate == null)
                    {
                        return null;
                    }

                    var labelTemplate = _pieceTemplate.GetComponentInChildren<TextMeshPro>(true);
                    var templateRenderer = labelTemplate != null ? labelTemplate.GetComponent<MeshRenderer>() : null;
                    if (labelTemplate == null || templateRenderer == null)
                    {
                        return null;
                    }

                    var popupObject = new GameObject("ScorePopup");
                    popupObject.transform.SetParent(_boardLayer.transform, false);
                    popupObject.transform.localPosition = localPosition;
                    popupObject.transform.localScale = ScorePopupStartScale;

                    var mainLabelObject = Instantiate(labelTemplate.gameObject, popupObject.transform, false);
                    mainLabelObject.name = "Main";
                    mainLabelObject.transform.localPosition = Vector3.zero;
                    mainLabelObject.transform.localRotation = Quaternion.identity;
                    mainLabelObject.transform.localScale = labelTemplate.transform.localScale;

                    var shadowLabelObject = Instantiate(labelTemplate.gameObject, popupObject.transform, false);
                    shadowLabelObject.name = "Shadow";
                    shadowLabelObject.transform.localPosition = ScorePopupShadowOffset;
                    shadowLabelObject.transform.localRotation = Quaternion.identity;
                    shadowLabelObject.transform.localScale = labelTemplate.transform.localScale;

                    var mainLabel = mainLabelObject.GetComponent<TextMeshPro>();
                    var mainRenderer = mainLabelObject.GetComponent<MeshRenderer>();
                    var shadowLabel = shadowLabelObject.GetComponent<TextMeshPro>();
                    var shadowRenderer = shadowLabelObject.GetComponent<MeshRenderer>();
                    if (mainLabel == null || mainRenderer == null || shadowLabel == null || shadowRenderer == null)
                    {
                        Destroy(popupObject);
                        return null;
                    }

                    ConfigureScorePopupLabel(mainLabel, mainRenderer, text, ScorePopupTextColor, ScorePopupSortingOrder);
                    ConfigureScorePopupLabel(shadowLabel, shadowRenderer, text, ScorePopupShadowColor, ScorePopupShadowSortingOrder);

                    return new ScorePopupVisual
                    {
                        root = popupObject,
                        transform = popupObject.transform,
                        baseLocalScale = popupObject.transform.localScale,
                        mainLabel = mainLabel,
                        mainRenderer = mainRenderer,
                        shadowLabel = shadowLabel,
                        shadowRenderer = shadowRenderer
                    };
                }

                private static void ConfigureScorePopupLabel(TextMeshPro label, MeshRenderer renderer, string text, Color color, int sortingOrder)
                {
                    label.text = text;
                    label.alignment = TextAlignmentOptions.Center;
                    label.color = color;
                    label.gameObject.SetActive(true);
                    renderer.sortingOrder = sortingOrder;
                    renderer.enabled = true;
                }

                private IEnumerator AnimateScorePopup(ScorePopupVisual popup)
                {
                    if (popup == null || popup.root == null || popup.transform == null)
                    {
                        yield break;
                    }

                    var startPosition = popup.transform.localPosition;
                    var endPosition = startPosition + (Vector3.up * ScorePopupTravelUnits);
                    var elapsed = 0f;
                    while (elapsed < ScorePopupDuration)
                    {
                        if (popup.root == null || popup.transform == null)
                        {
                            yield break;
                        }

                        elapsed += Time.unscaledDeltaTime;
                        var t = Mathf.Clamp01(elapsed / ScorePopupDuration);
                        var positionT = Mathf.SmoothStep(0f, 1f, t);
                        popup.transform.localPosition = Vector3.LerpUnclamped(startPosition, endPosition, positionT);
                        popup.transform.localScale = EvaluateScorePopupScale(t);
                        SetScorePopupAlpha(popup, 1f - t);
                        yield return null;
                    }

                    DestroyScorePopupVisual(popup);
                }

                private Vector3 EvaluateScorePopupScale(float normalizedTime)
                {
                    if (normalizedTime <= 0.55f)
                    {
                        var firstHalfT = Mathf.Clamp01(normalizedTime / 0.55f);
                        return Vector3.LerpUnclamped(ScorePopupStartScale, ScorePopupOvershootScale, Mathf.SmoothStep(0f, 1f, firstHalfT));
                    }

                    var secondHalfT = Mathf.Clamp01((normalizedTime - 0.55f) / 0.45f);
                    return Vector3.LerpUnclamped(ScorePopupOvershootScale, ScorePopupEndScale, Mathf.SmoothStep(0f, 1f, secondHalfT));
                }

                private void SetScorePopupAlpha(ScorePopupVisual popup, float alpha)
                {
                    if (popup == null)
                    {
                        return;
                    }

                    alpha = Mathf.Clamp01(alpha);
                    if (popup.mainLabel != null)
                    {
                        var mainColor = ScorePopupTextColor;
                        mainColor.a *= alpha;
                        popup.mainLabel.color = mainColor;
                    }

                    if (popup.shadowLabel != null)
                    {
                        var shadowColor = ScorePopupShadowColor;
                        shadowColor.a *= alpha;
                        popup.shadowLabel.color = shadowColor;
                    }
                }

                private void DestroyScorePopupVisual(ScorePopupVisual popup)
                {
                    if (popup == null || popup.root == null)
                    {
                        return;
                    }

                    Destroy(popup.root);
                }

                private void UpdatePieceVisualState(PieceVisual pieceVisual, BoardVisualCellSnapshot cell)
                {
                    if (pieceVisual == null || cell == null)
                    {
                        return;
                    }

                    pieceVisual.renderer.sprite = cell.frameIndex >= 0
                        ? LegacyUiSpriteLookup.GetGameplayPieceSprite(App.UiSprites, App.Sprites, cell.frameIndex)
                        : null;
                    var lockBarSprite = LegacyUiSpriteLookup.GetGameplayPieceLockBar(App.UiSprites, App.Sprites);
                    if (pieceVisual.lockBars != null)
                    {
                        foreach (var lockBar in pieceVisual.lockBars)
                        {
                            if (lockBar == null)
                            {
                                continue;
                            }

                            lockBar.sprite = lockBarSprite;
                        }
                    }

                    SetPieceLockBars(pieceVisual, cell.lockBarCount);
                    if (pieceVisual.label != null)
                    {
                        pieceVisual.label.text = "+" + cell.doubleCount;
                        pieceVisual.label.gameObject.SetActive(cell.doubleCount > 0);
                    }
                }

                private void SetPieceVisualAlpha(PieceVisual pieceVisual, float alpha)
                {
                    if (pieceVisual == null)
                    {
                        return;
                    }

                    var spriteColor = pieceVisual.renderer.color;
                    spriteColor.a = alpha;
                    pieceVisual.renderer.color = spriteColor;

                    if (pieceVisual.lockBars != null)
                    {
                        foreach (var lockBar in pieceVisual.lockBars)
                        {
                            if (lockBar == null)
                            {
                                continue;
                            }

                            var lockBarColor = lockBar.color;
                            lockBarColor.a = alpha;
                            lockBar.color = lockBarColor;
                        }
                    }

                    if (pieceVisual.label != null)
                    {
                        var labelColor = pieceVisual.label.color;
                        labelColor.a = alpha;
                        pieceVisual.label.color = labelColor;
                    }
                }

                private static void SetPieceLockBars(PieceVisual pieceVisual, int lockBarCount)
                {
                    if (pieceVisual == null || pieceVisual.lockBars == null)
                    {
                        return;
                    }

                    lockBarCount = Mathf.Clamp(lockBarCount, 0, 3);
                    for (var index = 0; index < pieceVisual.lockBars.Length; index++)
                    {
                        var lockBar = pieceVisual.lockBars[index];
                        if (lockBar == null)
                        {
                            continue;
                        }

                        lockBar.gameObject.SetActive(ShouldShowLockBar(index, lockBarCount));
                    }
                }

                private static bool ShouldShowLockBar(int index, int lockBarCount)
                {
                    switch (lockBarCount)
                    {
                        case 1:
                            return index == 1;
                        case 2:
                            return index == 0 || index == 2;
                        case 3:
                            return true;
                        default:
                            return false;
                    }
                }

                private void SetDestroyEffectFrame(DestroyEffectVisual effectVisual, int frameIndex)
                {
                    if (effectVisual == null || effectVisual.renderer == null)
                    {
                        return;
                    }

                    effectVisual.renderer.sprite = App.Sprites.GetDestroyEffectSprite(frameIndex);
                }

                private void DestroyDestroyEffectVisual(DestroyEffectVisual effectVisual)
                {
                    if (effectVisual == null || effectVisual.gameObject == null)
                    {
                        return;
                    }

                    Destroy(effectVisual.gameObject);
                }

                private void DestroyPieceVisual(int runtimeId)
                {
                    if (!_pieceVisuals.TryGetValue(runtimeId, out var pieceVisual))
                    {
                        return;
                    }

                    if (pieceVisual.gameObject != null)
                    {
                        Destroy(pieceVisual.gameObject);
                    }

                    _pieceVisuals.Remove(runtimeId);
                }

                private bool TryGetPieceVisualAtCell(Vector2Int coordinate, out PieceVisual pieceVisual)
                {
                    pieceVisual = null;
                    if (_currentSnapshot == null || !IsBoardCoordinate(coordinate))
                    {
                        return false;
                    }

                    var cell = _currentSnapshot.GetCell(coordinate.y, coordinate.x);
                    if (cell == null || cell.pieceRuntimeId == 0)
                    {
                        return false;
                    }

                    return _pieceVisuals.TryGetValue(cell.pieceRuntimeId, out pieceVisual);
                }

                private Vector3 GetInvalidSwapOffset(Vector2Int from, Vector2Int to)
                {
                    var delta = new Vector3(to.x - from.x, -(to.y - from.y), 0f);
                    if (delta.sqrMagnitude <= 0.0001f)
                    {
                        return Vector3.zero;
                    }

                    return delta.normalized * InvalidSwapTravelUnits;
                }

                private Vector3 GetBoardPosition(Vector2Int coordinate, int sortingOrder)
                {
                    return GetBoardPosition(coordinate.x, coordinate.y, sortingOrder);
                }

                private Vector3 GetBoardPosition(int column, int row, int sortingOrder)
                {
                    var pixelX = BoardLeftPixels + (column * BoardCellSizePixels);
                    var pixelY = BoardTopPixels + (row * BoardCellSizePixels);
                    return PixelToWorld(pixelX, pixelY, sortingOrder);
                }

                private bool IsBoardCoordinate(Vector2Int coordinate)
                {
                    return _board != null
                        && coordinate.x >= 0
                        && coordinate.x < _board.Width
                        && coordinate.y >= 0
                        && coordinate.y < _board.Height;
                }
    }
}
