using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
    public sealed partial class BoardModel
    {
                internal bool CanTargetBoosterAt(Vector2Int target)
                {
                    return EndState == BoardEndState.None && CanSelect(target);
                }

                internal List<Vector2Int> GetBoosterTargets(Vector2Int target, BoosterType boosterType)
                {
                    var targets = new List<Vector2Int>();
                    if (!CanTargetBoosterAt(target))
                    {
                        return targets;
                    }

                    var seen = new HashSet<Vector2Int>();
                    switch (boosterType)
                    {
                        case BoosterType.Single:
                            TryAddBoosterTarget(targets, seen, target.x, target.y);
                            break;
                        case BoosterType.Column:
                            for (var row = 0; row < _height; row++)
                            {
                                TryAddBoosterTarget(targets, seen, target.x, row);
                            }

                            break;
                        case BoosterType.Row:
                            for (var column = 0; column < _width; column++)
                            {
                                TryAddBoosterTarget(targets, seen, column, target.y);
                            }

                            break;
                        case BoosterType.Cross:
                            for (var row = 0; row < _height; row++)
                            {
                                TryAddBoosterTarget(targets, seen, target.x, row);
                            }

                            for (var column = 0; column < _width; column++)
                            {
                                TryAddBoosterTarget(targets, seen, column, target.y);
                            }

                            break;
                        case BoosterType.Area:
                            for (var row = target.y - 1; row <= target.y + 1; row++)
                            {
                                for (var column = target.x - 1; column <= target.x + 1; column++)
                                {
                                    TryAddBoosterTarget(targets, seen, column, row);
                                }
                            }

                            break;
                    }

                    return targets;
                }

                public BoardActionReport TrySwap(Vector2Int from, Vector2Int to)
                {
                    var report = new BoardActionReport();
                    if (EndState != BoardEndState.None || !AreAdjacent(from, to) || !CanSelect(from) || !CanSelect(to))
                    {
                        return report;
                    }

                    SwapPieces(from, to);
                    var initialMatches = FindMatches();
                    if (initialMatches.Count == 0)
                    {
                        SwapPieces(from, to);
                        return report;
                    }

                    report.valid = true;
                    SpendMove();
                    AddPhase(report, BoardVisualPhaseKind.Swap, 0.12f);
                    ResolveBoard(report, initialMatches);
                    FinalizeReport(report);
                    return report;
                }

                public BoardActionReport ApplyBooster(Vector2Int target, BoosterType boosterType)
                {
                    var report = new BoardActionReport();
                    var targets = GetBoosterTargets(target, boosterType);
                    if (targets.Count == 0)
                    {
                        return report;
                    }

                    var marks = new HashSet<Vector2Int>(targets);

                    report.valid = true;
                    SpendMove();
                    ResolveBoard(report, marks);
                    FinalizeReport(report);
                    return report;
                }

                private void TryAddBoosterTarget(List<Vector2Int> targets, HashSet<Vector2Int> seen, int column, int row)
                {
                    if (!IsInside(row, column))
                    {
                        return;
                    }

                    var cell = _cells[row, column];
                    if (!cell.exists || cell.piece == null)
                    {
                        return;
                    }

                    var coordinate = new Vector2Int(column, row);
                    if (seen.Add(coordinate))
                    {
                        targets.Add(coordinate);
                    }
                }

                private void ResolveBoard(BoardActionReport report, HashSet<Vector2Int> initialMarks)
                {
                    PendingEndReason = BoardPendingEndReason.None;

                    var marks = new HashSet<Vector2Int>(initialMarks);
                    while (marks.Count > 0)
                    {
                        ExpandBlockerDamage(marks);
                        var scorePopups = new List<BoardScorePopupInfo>();
                        var destroyedThisWave = ApplyMarks(marks, report, scorePopups);
                        if (destroyedThisWave > 3)
                        {
                            AddRandomDoubleBonuses();
                        }

                        AddPhase(report, BoardVisualPhaseKind.Clear, 0.12f, scorePopups);
                        if (!CollapseAndFill(report))
                        {
                            return;
                        }

                        marks = FindMatches();
                    }

                    if (AllObjectivesComplete())
                    {
                        SetPendingEndState(BoardEndState.WinPending, BoardPendingEndReason.ObjectivesCompleted);
                    }
                    else if (MovesRemaining <= 0)
                    {
                        SetPendingEndState(BoardEndState.LosePending, BoardPendingEndReason.NoMovesRemaining);
                    }
                    else if (TimeRemaining <= 0)
                    {
                        SetPendingEndState(BoardEndState.LosePending, BoardPendingEndReason.TimeExpired);
                    }
                    else if (!HasAvailableMoves())
                    {
                        report.shuffled = ShuffleUntilPlayable();
                        if (report.shuffled)
                        {
                            AddPhase(report, BoardVisualPhaseKind.Shuffle, 0.22f);
                        }
                    }
                }

                private int ApplyMarks(HashSet<Vector2Int> marks, BoardActionReport report, List<BoardScorePopupInfo> scorePopups)
                {
                    var destroyedCount = 0;
                    var touchedCoordinates = new List<Vector2Int>(marks);
                    foreach (var coordinate in touchedCoordinates)
                    {
                        if (!IsInside(coordinate.y, coordinate.x))
                        {
                            continue;
                        }

                        var cell = _cells[coordinate.y, coordinate.x];
                        if (!cell.exists || cell.piece == null)
                        {
                            continue;
                        }

                        report.boardChanged = true;
                        var piece = cell.piece;
                        if (piece.IsColored && piece.durability > 0)
                        {
                            piece.durability -= 1;
                            if (piece.durability <= 0)
                            {
                                piece.movable = true;
                            }

                            continue;
                        }

                        if (piece.IsBlocker)
                        {
                            piece.durability += 1;
                            if (piece.durability < 0)
                            {
                                continue;
                            }
                        }

                        destroyedCount += 1;
                        report.destroyedCount += 1;

                        if (piece.IsColored)
                        {
                            var multiplier = 1 + Mathf.Max(0, piece.doubleCount);
                            var scoreDelta = 15 * multiplier;
                            Score += scoreDelta;
                            report.scoreGained += scoreDelta;
                            if (scorePopups != null)
                            {
                                scorePopups.Add(new BoardScorePopupInfo
                                {
                                    runtimeId = piece.runtimeId,
                                    scoreDelta = scoreDelta
                                });
                            }

                            IncrementObjective((ObjectiveType)piece.color, multiplier);
                        }

                        EarnedCoins += 2;
                        report.coinsGained += 2;

                        if (cell.hasIce)
                        {
                            cell.hasIce = false;
                            IncrementObjective(ObjectiveType.Ice, 1);
                        }

                        cell.piece = null;
                    }

                    return destroyedCount;
                }
    }
}
