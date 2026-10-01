using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
    public sealed partial class BoardModel
    {
                private struct SettleMove
                {
                    public Vector2Int from;
                    public Vector2Int to;
                }

                private readonly System.Random _random;
                private readonly int _initialTimer;
                private readonly int _width;
                private readonly int _height;
                private readonly BoardCell[,] _cells;
                private readonly ObjectiveProgressState[] _objectives;
                private int _nextPieceRuntimeId = 1;

                public BoardModel(LevelDefinition definition, int randomSeed)
                {
                    Definition = definition;
                    _width = definition.boardWidth;
                    _height = definition.boardHeight;
                    _cells = new BoardCell[_height, _width];
                    _random = new System.Random(randomSeed);
                    _initialTimer = definition.timerSeconds;
                    MovesRemaining = definition.moveCount;
                    TimeRemaining = definition.timerSeconds;

                    for (var row = 0; row < _height; row++)
                    {
                        for (var column = 0; column < _width; column++)
                        {
                            var spec = definition.GetCell(row, column);
                            var cell = new BoardCell
                            {
                                exists = spec != null && spec.kind != CellKind.Hole,
                                hasIce = definition.HasIce(row, column),
                                piece = spec != null && spec.kind != CellKind.Hole ? CreatePieceFromSpec(spec) : null
                            };
                            _cells[row, column] = cell;
                        }
                    }

                    _objectives = new ObjectiveProgressState[definition.objectives.Length];
                    for (var index = 0; index < definition.objectives.Length; index++)
                    {
                        _objectives[index] = new ObjectiveProgressState
                        {
                            definition = definition.objectives[index],
                            currentCount = 0
                        };
                    }
                }

                public LevelDefinition Definition { get; }
                public int Width => _width;
                public int Height => _height;
                public int MovesRemaining { get; private set; }
                public int TimeRemaining { get; private set; }
                public int Score { get; private set; }
                public int EarnedCoins { get; private set; }
                public ObjectiveProgressState[] Objectives => _objectives;
                public BoardEndState EndState { get; private set; }
                public BoardPendingEndReason PendingEndReason { get; private set; }

                public BoardCell GetCell(int row, int column)
                {
                    if (!IsInside(row, column))
                    {
                        return null;
                    }

                    return _cells[row, column];
                }

                public BoardVisualSnapshot CreateVisualSnapshot()
                {
                    var snapshot = new BoardVisualSnapshot(_width, _height);
                    for (var row = 0; row < _height; row++)
                    {
                        for (var column = 0; column < _width; column++)
                        {
                            var sourceCell = _cells[row, column];
                            var targetCell = snapshot.GetCell(row, column);
                            targetCell.exists = sourceCell.exists;
                            targetCell.hasIce = sourceCell.exists && sourceCell.hasIce;
                            targetCell.pieceRuntimeId = 0;
                            targetCell.frameIndex = -1;
                            targetCell.lockBarCount = 0;
                            targetCell.doubleCount = 0;
                            if (!sourceCell.exists || sourceCell.piece == null)
                            {
                                continue;
                            }

                            targetCell.pieceRuntimeId = sourceCell.piece.runtimeId;
                            targetCell.frameIndex = sourceCell.piece.GetFrameIndex();
                            targetCell.lockBarCount = sourceCell.piece.GetLockBarCount();
                            targetCell.doubleCount = sourceCell.piece.doubleCount;
                        }
                    }

                    return snapshot;
                }

                public BoardHudSnapshot CreateHudSnapshot()
                {
                    var objectives = new BoardHudObjectiveSnapshot[_objectives.Length];
                    for (var index = 0; index < _objectives.Length; index++)
                    {
                        var objective = _objectives[index];
                        objectives[index] = new BoardHudObjectiveSnapshot
                        {
                            type = objective.definition.type,
                            currentCount = objective.currentCount,
                            targetCount = objective.definition.targetCount,
                            isComplete = objective.IsComplete
                        };
                    }

                    return new BoardHudSnapshot
                    {
                        score = Score,
                        coins = EarnedCoins,
                        movesRemaining = MovesRemaining,
                        timeRemaining = TimeRemaining,
                        stars = CurrentStars,
                        endState = EndState,
                        pendingEndReason = PendingEndReason,
                        objectives = objectives
                    };
                }

                public int CurrentStars
                {
                    get
                    {
                        if (TimeRemaining < 10)
                        {
                            return 0;
                        }

                        if (TimeRemaining < (2f * _initialTimer) / 9f)
                        {
                            return 1;
                        }

                        if (TimeRemaining < (3f * _initialTimer) / 9f)
                        {
                            return 2;
                        }

                        return 3;
                    }
                }

                public bool TickSecond()
                {
                    if (EndState != BoardEndState.None)
                    {
                        return false;
                    }

                    TimeRemaining = Mathf.Max(0, TimeRemaining - 1);
                    if (TimeRemaining <= 0 && !AllObjectivesComplete())
                    {
                        SetPendingEndState(BoardEndState.LosePending, BoardPendingEndReason.TimeExpired);
                    }

                    return true;
                }

                public bool FinalizePendingEndState()
                {
                    switch (EndState)
                    {
                        case BoardEndState.WinPending:
                            EndState = BoardEndState.Win;
                            return true;
                        case BoardEndState.LosePending:
                            EndState = BoardEndState.Lose;
                            return true;
                        default:
                            return false;
                    }
                }

                private int ToIndex(int row, int column)
                {
                    return (row * _width) + column;
                }

                private int AllocatePieceRuntimeId()
                {
                    var runtimeId = _nextPieceRuntimeId;
                    _nextPieceRuntimeId += 1;
                    return runtimeId;
                }

                private void SpendMove()
                {
                    MovesRemaining = Mathf.Max(0, MovesRemaining - 1);
                }

                private void AddPhase(BoardActionReport report, BoardVisualPhaseKind kind, float suggestedDuration, List<BoardScorePopupInfo> scorePopups = null)
                {
                    var phase = new BoardVisualPhase
                    {
                        kind = kind,
                        snapshot = CreateVisualSnapshot(),
                        hud = CreateHudSnapshot(),
                        suggestedDuration = suggestedDuration
                    };

                    if (scorePopups != null)
                    {
                        phase.scorePopups.AddRange(scorePopups);
                    }

                    report.phases.Add(phase);
                }

                private void FinalizeReport(BoardActionReport report)
                {
                    report.stars = CurrentStars;
                    report.endState = EndState;
                    report.pendingEndReason = PendingEndReason;
                    if (report.phases.Count > 0)
                    {
                        report.phases[report.phases.Count - 1].hud = CreateHudSnapshot();
                    }
                }

                private static float CalculateMotionDuration(int movedPieceCount)
                {
                    return Mathf.Min(0.28f, 0.18f + (0.03f * Mathf.Max(0, movedPieceCount)));
                }

                private void SwapPieces(Vector2Int first, Vector2Int second)
                {
                    var temp = _cells[first.y, first.x].piece;
                    _cells[first.y, first.x].piece = _cells[second.y, second.x].piece;
                    _cells[second.y, second.x].piece = temp;
                }

                private void SetPendingEndState(BoardEndState endState, BoardPendingEndReason reason)
                {
                    EndState = endState;
                    PendingEndReason = reason;
                }

                private bool IsInside(int row, int column)
                {
                    return row >= 0 && row < _height && column >= 0 && column < _width;
                }

                private static bool AreAdjacent(Vector2Int first, Vector2Int second)
                {
                    var delta = first - second;
                    return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1;
                }
    }
}
