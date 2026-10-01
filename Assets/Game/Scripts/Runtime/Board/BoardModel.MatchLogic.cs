using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
    public sealed partial class BoardModel
    {
                public bool HasAvailableMoves()
                {
                    for (var row = 0; row < _height; row++)
                    {
                        for (var column = 0; column < _width; column++)
                        {
                            if (!CanSelect(column, row))
                            {
                                continue;
                            }

                            if (TestSwapForMatch(column, row, column + 1, row) || TestSwapForMatch(column, row, column, row + 1))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                public bool ShuffleUntilPlayable()
                {
                    var movablePositions = new List<Vector2Int>();
                    var movablePieces = new List<BoardPiece>();
                    for (var row = 0; row < _height; row++)
                    {
                        for (var column = 0; column < _width; column++)
                        {
                            if (CanSelect(column, row))
                            {
                                movablePositions.Add(new Vector2Int(column, row));
                                movablePieces.Add(_cells[row, column].piece);
                            }
                        }
                    }

                    if (movablePieces.Count < 2)
                    {
                        return false;
                    }

                    var shuffled = false;
                    for (var attempt = 0; attempt < 128; attempt++)
                    {
                        ShuffleList(movablePieces);
                        shuffled = true;
                        for (var index = 0; index < movablePositions.Count; index++)
                        {
                            var coordinate = movablePositions[index];
                            _cells[coordinate.y, coordinate.x].piece = movablePieces[index];
                        }

                        if (FindMatches().Count == 0 && HasAvailableMoves())
                        {
                            return true;
                        }
                    }

                    return shuffled;
                }

                private void ExpandBlockerDamage(HashSet<Vector2Int> marks)
                {
                    var extraMarks = new List<Vector2Int>();
                    foreach (var coordinate in marks)
                    {
                        if (!IsInside(coordinate.y, coordinate.x))
                        {
                            continue;
                        }

                        var cell = _cells[coordinate.y, coordinate.x];
                        if (!cell.exists || cell.piece == null || !cell.piece.IsColored)
                        {
                            continue;
                        }

                        var neighbors = new[]
                        {
                            new Vector2Int(coordinate.x - 1, coordinate.y),
                            new Vector2Int(coordinate.x + 1, coordinate.y),
                            new Vector2Int(coordinate.x, coordinate.y - 1),
                            new Vector2Int(coordinate.x, coordinate.y + 1)
                        };

                        foreach (var neighbor in neighbors)
                        {
                            if (!IsInside(neighbor.y, neighbor.x))
                            {
                                continue;
                            }

                            var adjacentCell = _cells[neighbor.y, neighbor.x];
                            if (adjacentCell.exists && adjacentCell.piece != null && adjacentCell.piece.IsBlocker)
                            {
                                extraMarks.Add(neighbor);
                            }
                        }
                    }

                    foreach (var extraMark in extraMarks)
                    {
                        marks.Add(extraMark);
                    }
                }

                private HashSet<Vector2Int> FindMatches()
                {
                    var matches = new HashSet<Vector2Int>();
                    for (var row = 0; row < _height; row++)
                    {
                        var runStart = 0;
                        while (runStart < _width)
                        {
                            var runLength = GetRunLength(row, runStart, 1, 0);
                            if (runLength >= 3)
                            {
                                for (var step = 0; step < runLength; step++)
                                {
                                    matches.Add(new Vector2Int(runStart + step, row));
                                }
                            }

                            runStart += Mathf.Max(1, runLength);
                        }
                    }

                    for (var column = 0; column < _width; column++)
                    {
                        var runStart = 0;
                        while (runStart < _height)
                        {
                            var runLength = GetRunLength(runStart, column, 0, 1);
                            if (runLength >= 3)
                            {
                                for (var step = 0; step < runLength; step++)
                                {
                                    matches.Add(new Vector2Int(column, runStart + step));
                                }
                            }

                            runStart += Mathf.Max(1, runLength);
                        }
                    }

                    return matches;
                }

                private int GetRunLength(int startRow, int startColumn, int stepColumn, int stepRow)
                {
                    if (!HasMatchablePiece(startRow, startColumn))
                    {
                        return 1;
                    }

                    var color = _cells[startRow, startColumn].piece.color;
                    var runLength = 1;
                    var row = startRow + stepRow;
                    var column = startColumn + stepColumn;
                    while (IsInside(row, column) && HasMatchablePiece(row, column) && _cells[row, column].piece.color == color)
                    {
                        runLength += 1;
                        row += stepRow;
                        column += stepColumn;
                    }

                    return runLength;
                }

                private bool HasMatchablePiece(int row, int column)
                {
                    if (!IsInside(row, column))
                    {
                        return false;
                    }

                    var cell = _cells[row, column];
                    return cell.exists && cell.piece != null && cell.piece.IsColored;
                }

                private bool TestSwapForMatch(int fromColumn, int fromRow, int toColumn, int toRow)
                {
                    if (!IsInside(toRow, toColumn) || !CanSelect(fromColumn, fromRow) || !CanSelect(toColumn, toRow))
                    {
                        return false;
                    }

                    SwapPieces(new Vector2Int(fromColumn, fromRow), new Vector2Int(toColumn, toRow));
                    var hasMatch = FindMatches().Count > 0;
                    SwapPieces(new Vector2Int(fromColumn, fromRow), new Vector2Int(toColumn, toRow));
                    return hasMatch;
                }

                private void AddRandomDoubleBonuses()
                {
                    var candidates = new List<BoardPiece>();
                    for (var row = 0; row < _height; row++)
                    {
                        for (var column = 0; column < _width; column++)
                        {
                            var cell = _cells[row, column];
                            if (cell.exists && cell.piece != null && cell.piece.CanBeSelected)
                            {
                                candidates.Add(cell.piece);
                            }
                        }
                    }

                    if (candidates.Count == 0)
                    {
                        return;
                    }

                    for (var index = 0; index < 4; index++)
                    {
                        candidates[_random.Next(candidates.Count)].doubleCount += 1;
                    }
                }

                private void IncrementObjective(ObjectiveType type, int amount)
                {
                    foreach (var objective in _objectives)
                    {
                        if (objective.definition.type == type)
                        {
                            objective.currentCount = Mathf.Min(objective.definition.targetCount, objective.currentCount + amount);
                        }
                    }
                }

                private bool AllObjectivesComplete()
                {
                    foreach (var objective in _objectives)
                    {
                        if (!objective.IsComplete)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                private BoardPiece CreatePieceFromSpec(GridCellSpec spec)
                {
                    return new BoardPiece
                    {
                        runtimeId = AllocatePieceRuntimeId(),
                        kind = spec.kind,
                        color = spec.color,
                        durability = spec.durability,
                        movable = spec.movable,
                        sourceTileId = spec.sourceTileId,
                        lockVisualStyle = spec.lockVisualStyle,
                        doubleCount = 0
                    };
                }

                private BoardPiece CreateRandomPiece()
                {
                    var color = (JellyColor)_random.Next(0, 5);
                    return new BoardPiece
                    {
                        runtimeId = AllocatePieceRuntimeId(),
                        kind = CellKind.Jelly,
                        color = color,
                        durability = 0,
                        movable = true,
                        sourceTileId = 2 + (int)color,
                        lockVisualStyle = LockVisualStyle.None,
                        doubleCount = 0
                    };
                }

                private void ShuffleList<T>(IList<T> list)
                {
                    for (var index = list.Count - 1; index > 0; index--)
                    {
                        var swapIndex = _random.Next(index + 1);
                        var temp = list[index];
                        list[index] = list[swapIndex];
                        list[swapIndex] = temp;
                    }
                }

                private bool CanSelect(Vector2Int coordinate)
                {
                    return CanSelect(coordinate.x, coordinate.y);
                }

                private bool CanSelect(int column, int row)
                {
                    if (!IsInside(row, column))
                    {
                        return false;
                    }

                    var cell = _cells[row, column];
                    return cell.exists && cell.piece != null && cell.piece.CanBeSelected;
                }

                private bool CanCollapse(BoardPiece piece)
                {
                    return piece != null && piece.IsColored && piece.durability <= 0 && piece.movable;
                }
    }
}
