using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
    public sealed partial class BoardModel
    {
                private bool CollapseAndFill(BoardActionReport report)
                {
                    var seenStates = new HashSet<string>();
                    var maxPassCount = Mathf.Max(8, _width * _height * 4);
                    for (var passIndex = 0; passIndex < maxPassCount; passIndex++)
                    {
                        var snapshot = BuildBoardSnapshot();
                        if (!seenStates.Add(snapshot))
                        {
                            TriggerSettleGuard(report, "Detected repeated settle state", passIndex, snapshot);
                            return false;
                        }

                        var moves = BuildSettlePassMoves();
                        if (moves.Count == 0)
                        {
                            var fillCount = FillEmptyCellsFromTop();
                            if (fillCount > 0)
                            {
                                AddPhase(report, BoardVisualPhaseKind.Fill, CalculateMotionDuration(fillCount));
                            }

                            return true;
                        }

                        ApplySettleMoves(moves);
                        AddPhase(report, BoardVisualPhaseKind.Settle, CalculateMotionDuration(moves.Count));
                    }

                    TriggerSettleGuard(report, "Exceeded settle pass limit", maxPassCount, BuildBoardSnapshot());
                    return false;
                }

                private List<SettleMove> BuildSettlePassMoves()
                {
                    var moves = new List<SettleMove>();
                    var reservedSources = new HashSet<int>();
                    for (var row = _height - 1; row >= 0; row--)
                    {
                        for (var column = 0; column < _width; column++)
                        {
                            if (!_cells[row, column].exists || _cells[row, column].piece != null)
                            {
                                continue;
                            }

                            if (!TryFindSettleSource(row, column, reservedSources, out var source))
                            {
                                continue;
                            }

                            reservedSources.Add(ToIndex(source.y, source.x));
                            moves.Add(new SettleMove
                            {
                                from = source,
                                to = new Vector2Int(column, row)
                            });
                        }
                    }

                    return moves;
                }

                private bool TryFindSettleSource(int targetRow, int targetColumn, HashSet<int> reservedSources, out Vector2Int source)
                {
                    if (TryFindVerticalSource(targetRow, targetColumn, reservedSources, out source, out var blockedByGap))
                    {
                        return true;
                    }

                    if (blockedByGap && TryFindDiagonalSource(targetRow, targetColumn, reservedSources, out source))
                    {
                        return true;
                    }

                    source = default;
                    return false;
                }

                private bool TryFindVerticalSource(int targetRow, int targetColumn, HashSet<int> reservedSources, out Vector2Int source, out bool blockedByGap)
                {
                    blockedByGap = false;
                    for (var row = targetRow - 1; row >= 0; row--)
                    {
                        if (!_cells[row, targetColumn].exists)
                        {
                            blockedByGap = true;
                            break;
                        }

                        var cell = _cells[row, targetColumn];
                        if (cell.piece == null)
                        {
                            continue;
                        }

                        if (reservedSources.Contains(ToIndex(row, targetColumn)))
                        {
                            break;
                        }

                        if (CanCollapse(cell.piece))
                        {
                            source = new Vector2Int(targetColumn, row);
                            return true;
                        }

                        break;
                    }

                    source = default;
                    return false;
                }

                private bool TryFindDiagonalSource(int targetRow, int targetColumn, HashSet<int> reservedSources, out Vector2Int source)
                {
                    if (targetRow <= 0)
                    {
                        source = default;
                        return false;
                    }

                    var candidateColumns = new[] { targetColumn + 1, targetColumn - 1 };
                    var candidateRow = targetRow - 1;
                    foreach (var candidateColumn in candidateColumns)
                    {
                        if (!IsInside(candidateRow, candidateColumn))
                        {
                            continue;
                        }

                        var cell = _cells[candidateRow, candidateColumn];
                        if (!cell.exists || cell.piece == null)
                        {
                            continue;
                        }

                        if (reservedSources.Contains(ToIndex(candidateRow, candidateColumn)) || !CanCollapse(cell.piece))
                        {
                            continue;
                        }

                        source = new Vector2Int(candidateColumn, candidateRow);
                        return true;
                    }

                    source = default;
                    return false;
                }

                private void ApplySettleMoves(List<SettleMove> moves)
                {
                    foreach (var move in moves)
                    {
                        _cells[move.to.y, move.to.x].piece = _cells[move.from.y, move.from.x].piece;
                        _cells[move.from.y, move.from.x].piece = null;
                    }
                }

                private int FillEmptyCellsFromTop()
                {
                    var fillCount = 0;
                    for (var column = 0; column < _width; column++)
                    {
                        for (var row = 0; row < _height; row++)
                        {
                            if (_cells[row, column].exists && _cells[row, column].piece == null)
                            {
                                _cells[row, column].piece = CreateRandomPiece();
                                fillCount += 1;
                            }
                        }
                    }

                    return fillCount;
                }

                private void TriggerSettleGuard(BoardActionReport report, string reason, int passIndex, string snapshot)
                {
                    report.settleGuardTriggered = true;
                    report.settleGuardMessage = $"[JellyQuest] {reason} on level {Definition.levelNumber} after {passIndex} settle passes.\n{snapshot}";
                    Debug.LogError(report.settleGuardMessage);
                }

                private string BuildBoardSnapshot()
                {
                    var builder = new StringBuilder((_width * _height * 4) + _height);
                    for (var row = 0; row < _height; row++)
                    {
                        for (var column = 0; column < _width; column++)
                        {
                            var cell = _cells[row, column];
                            if (!cell.exists)
                            {
                                builder.Append("## ");
                                continue;
                            }

                            if (cell.piece == null)
                            {
                                builder.Append("__ ");
                                continue;
                            }

                            if (cell.piece.IsBlocker)
                            {
                                builder.Append('B');
                                builder.Append(cell.piece.durability);
                                builder.Append(' ');
                                continue;
                            }

                            builder.Append((char)('A' + Mathf.Clamp((int)cell.piece.color, 0, 25)));
                            builder.Append(cell.piece.durability);
                            builder.Append(cell.piece.movable ? 'M' : 'L');
                            builder.Append(' ');
                        }

                        builder.Append('\n');
                    }

                    return builder.ToString();
                }
    }
}
