using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace JellyQuestUnity.EditorTests
{
    public sealed class JellyQuestBoardTests
    {
        [Test]
        public void CollapseAndFill_DoesNotPullFromSameRowWhenGapHasNoDiagonalSource()
        {
            var board = CreateBoardModel(
                1, 1,
                2, 6);

            board.GetCell(1, 1).piece.doubleCount = 7;

            var report = board.ApplyBooster(new Vector2Int(0, 1), BoosterType.Single);

            Assert.That(report.valid, Is.True);
            Assert.That(report.settleGuardTriggered, Is.False);
            Assert.That(board.GetCell(1, 1).piece, Is.Not.Null);
            Assert.That(board.GetCell(1, 1).piece.doubleCount, Is.EqualTo(7), "Same-row source should stay in place.");
        }

        [Test]
        public void CollapseAndFill_UsesAboveRightDiagonalSourceBeforeSpawning()
        {
            var board = CreateBoardModel(
                1, 3,
                2, 6);

            board.GetCell(0, 1).piece.doubleCount = 3;
            board.GetCell(1, 1).piece.doubleCount = 7;

            var report = board.ApplyBooster(new Vector2Int(0, 1), BoosterType.Single);

            Assert.That(report.valid, Is.True);
            Assert.That(report.settleGuardTriggered, Is.False);
            Assert.That(board.GetCell(1, 0).piece, Is.Not.Null);
            Assert.That(board.GetCell(1, 0).piece.doubleCount, Is.EqualTo(3), "Gap should be filled by the diagonal source above-right.");
            Assert.That(board.GetCell(1, 1).piece, Is.Not.Null);
            Assert.That(board.GetCell(1, 1).piece.doubleCount, Is.EqualTo(7), "Same-row source must not slide sideways into the gap.");
        }

        [Test]
        public void ApplyBooster_EmitsClearSettleFillPhasesInOrder()
        {
            var board = CreateBoardModel(
                1, 3,
                2, 6);

            var report = board.ApplyBooster(new Vector2Int(0, 1), BoosterType.Single);

            Assert.That(report.valid, Is.True);
            Assert.That(report.phases.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(report.phases[0].kind, Is.EqualTo(BoardVisualPhaseKind.Clear));
            Assert.That(report.phases[1].kind, Is.EqualTo(BoardVisualPhaseKind.Settle));
            Assert.That(report.phases[2].kind, Is.EqualTo(BoardVisualPhaseKind.Fill));
        }

        [Test]
        public void ClearPhase_EmitsScorePopupPerDestroyedColoredPiece()
        {
            var board = CreateBoardModel(
                3,
                1,
                new[] { 2, 3, 4 });

            var report = board.ApplyBooster(new Vector2Int(1, 0), BoosterType.Row);
            var clearPhase = FindPhase(report, BoardVisualPhaseKind.Clear);

            Assert.That(clearPhase, Is.Not.Null);
            Assert.That(clearPhase.scorePopups.Count, Is.EqualTo(3));
            foreach (var popup in clearPhase.scorePopups)
            {
                Assert.That(popup.scoreDelta, Is.EqualTo(15));
                Assert.That(popup.runtimeId, Is.Not.EqualTo(0));
            }
        }

        [Test]
        public void ClearPhase_ScorePopupsReflectDoubleMultiplier()
        {
            var board = CreateBoardModel(
                3,
                1,
                new[] { 2, 3, 4 });
            var boostedRuntimeId = board.GetCell(0, 1).piece.runtimeId;
            board.GetCell(0, 1).piece.doubleCount = 2;

            var report = board.ApplyBooster(new Vector2Int(1, 0), BoosterType.Row);
            var clearPhase = FindPhase(report, BoardVisualPhaseKind.Clear);

            Assert.That(clearPhase, Is.Not.Null);
            Assert.That(clearPhase.scorePopups.Count, Is.EqualTo(3));

            BoardScorePopupInfo boostedPopup = null;
            foreach (var popup in clearPhase.scorePopups)
            {
                if (popup.runtimeId == boostedRuntimeId)
                {
                    boostedPopup = popup;
                    break;
                }
            }

            Assert.That(boostedPopup, Is.Not.Null);
            Assert.That(boostedPopup.scoreDelta, Is.EqualTo(45));
        }

        [Test]
        public void FillPhase_SpawnsPiecesWithFreshRuntimeIds()
        {
            var board = CreateBoardModel(
                1, 3,
                2, 6);

            var initialIds = CollectRuntimeIds(board.CreateVisualSnapshot());
            var report = board.ApplyBooster(new Vector2Int(0, 1), BoosterType.Single);
            var fillPhase = FindPhase(report, BoardVisualPhaseKind.Fill);

            Assert.That(fillPhase, Is.Not.Null);
            var spawnedId = fillPhase.snapshot.GetCell(0, 1).pieceRuntimeId;
            Assert.That(spawnedId, Is.Not.EqualTo(0));
            Assert.That(initialIds.Contains(spawnedId), Is.False, "Filled pieces should receive a fresh runtime id.");
        }

        [Test]
        public void ClearPhase_ReflectsIceBreakAndDamageStatesImmediately()
        {
            var board = CreateBoardModel(
                3,
                1,
                new[] { 2, 8, 13 },
                CreateBackgroundMask(3, 1, new Vector2Int(0, 0)));

            var lockedRuntimeId = board.GetCell(0, 1).piece.runtimeId;
            var blockerRuntimeId = board.GetCell(0, 2).piece.runtimeId;

            var report = board.ApplyBooster(new Vector2Int(0, 0), BoosterType.Row);
            var clearPhase = FindPhase(report, BoardVisualPhaseKind.Clear);

            Assert.That(clearPhase, Is.Not.Null);

            var brokenIceCell = clearPhase.snapshot.GetCell(0, 0);
            Assert.That(brokenIceCell.hasIce, Is.False);
            Assert.That(brokenIceCell.pieceRuntimeId, Is.EqualTo(0));

            var unlockedCell = clearPhase.snapshot.GetCell(0, 1);
            Assert.That(unlockedCell.pieceRuntimeId, Is.EqualTo(lockedRuntimeId));
            Assert.That(unlockedCell.frameIndex, Is.EqualTo((int)JellyColor.Green));
            Assert.That(unlockedCell.lockBarCount, Is.EqualTo(0));

            var damagedBlockerCell = clearPhase.snapshot.GetCell(0, 2);
            Assert.That(damagedBlockerCell.pieceRuntimeId, Is.EqualTo(blockerRuntimeId));
            Assert.That(damagedBlockerCell.frameIndex, Is.EqualTo(10));
            Assert.That(damagedBlockerCell.lockBarCount, Is.EqualTo(0));
        }

        [Test]
        public void CreateVisualSnapshot_UsesBasePieceFramesAndLockBarCountsForLockedPieces()
        {
            var board = CreateBoardModel(
                5,
                1,
                new[] { 2, 8, 16, 17, 12 });

            var snapshot = board.CreateVisualSnapshot();

            var basePieceCell = snapshot.GetCell(0, 0);
            Assert.That(basePieceCell.frameIndex, Is.EqualTo((int)JellyColor.Red));
            Assert.That(basePieceCell.lockBarCount, Is.EqualTo(0));

            var simpleLockCell = snapshot.GetCell(0, 1);
            Assert.That(simpleLockCell.frameIndex, Is.EqualTo((int)JellyColor.Green));
            Assert.That(simpleLockCell.lockBarCount, Is.EqualTo(1));

            var layeredLockTwoCell = snapshot.GetCell(0, 2);
            Assert.That(layeredLockTwoCell.frameIndex, Is.EqualTo((int)JellyColor.Red));
            Assert.That(layeredLockTwoCell.lockBarCount, Is.EqualTo(2));

            var layeredLockThreeCell = snapshot.GetCell(0, 3);
            Assert.That(layeredLockThreeCell.frameIndex, Is.EqualTo((int)JellyColor.Red));
            Assert.That(layeredLockThreeCell.lockBarCount, Is.EqualTo(3));

            var blockerCell = snapshot.GetCell(0, 4);
            Assert.That(blockerCell.frameIndex, Is.EqualTo(10));
            Assert.That(blockerCell.lockBarCount, Is.EqualTo(0));
        }

        [Test]
        public void ClearPhase_ReducesLockBarCountWithoutChangingBasePieceSprite()
        {
            var board = CreateBoardModel(
                3,
                1,
                new[] { 2, 16, 2 });

            var lockedRuntimeId = board.GetCell(0, 1).piece.runtimeId;

            var report = board.ApplyBooster(new Vector2Int(0, 0), BoosterType.Row);
            var clearPhase = FindPhase(report, BoardVisualPhaseKind.Clear);

            Assert.That(clearPhase, Is.Not.Null);

            var damagedLockCell = clearPhase.snapshot.GetCell(0, 1);
            Assert.That(damagedLockCell.pieceRuntimeId, Is.EqualTo(lockedRuntimeId));
            Assert.That(damagedLockCell.frameIndex, Is.EqualTo((int)JellyColor.Red));
            Assert.That(damagedLockCell.lockBarCount, Is.EqualTo(1));
        }

        [Test]
        public void ClearPhase_ScorePopupsOnlyIncludeScoringPieces()
        {
            var board = CreateBoardModel(
                3,
                1,
                new[] { 2, 8, 13 },
                CreateBackgroundMask(3, 1, new Vector2Int(0, 0)));

            var scoringRuntimeId = board.GetCell(0, 0).piece.runtimeId;
            var lockedRuntimeId = board.GetCell(0, 1).piece.runtimeId;
            var blockerRuntimeId = board.GetCell(0, 2).piece.runtimeId;

            var report = board.ApplyBooster(new Vector2Int(0, 0), BoosterType.Row);
            var clearPhase = FindPhase(report, BoardVisualPhaseKind.Clear);

            Assert.That(clearPhase, Is.Not.Null);
            Assert.That(clearPhase.scorePopups.Count, Is.EqualTo(1));
            Assert.That(clearPhase.scorePopups[0].runtimeId, Is.EqualTo(scoringRuntimeId));
            Assert.That(clearPhase.scorePopups[0].runtimeId, Is.Not.EqualTo(lockedRuntimeId));
            Assert.That(clearPhase.scorePopups[0].runtimeId, Is.Not.EqualTo(blockerRuntimeId));
            Assert.That(clearPhase.scorePopups[0].scoreDelta, Is.EqualTo(15));
        }

        [Test]
        public void ResolveBoard_AddsShufflePhaseWhenBoardEndsWithoutMoves()
        {
            var board = CreateBoardModel(
                3,
                3,
                new[] { 3, 6, 3, 6, 3, 6, 6, 4, 5 });

            var report = board.ApplyBooster(new Vector2Int(1, 1), BoosterType.Single);

            Assert.That(report.valid, Is.True);
            Assert.That(report.shuffled, Is.True);
            Assert.That(report.phases.Count, Is.GreaterThan(0));
            Assert.That(report.phases[report.phases.Count - 1].kind, Is.EqualTo(BoardVisualPhaseKind.Shuffle));
        }

        [Test]
        public void GetBoosterTargets_ReturnsExpectedPatternForEveryBooster()
        {
            var board = CreateBoardModel(5, 5, CreateFilledSourceTileIds(5, 5));
            var target = new Vector2Int(2, 2);

            Assert.That(board.GetBoosterTargets(target, BoosterType.Single), Is.EquivalentTo(new[]
            {
                new Vector2Int(2, 2)
            }));

            Assert.That(board.GetBoosterTargets(target, BoosterType.Column), Is.EquivalentTo(new[]
            {
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(2, 3),
                new Vector2Int(2, 4)
            }));

            Assert.That(board.GetBoosterTargets(target, BoosterType.Row), Is.EquivalentTo(new[]
            {
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2)
            }));

            Assert.That(board.GetBoosterTargets(target, BoosterType.Cross), Is.EquivalentTo(new[]
            {
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(2, 3),
                new Vector2Int(2, 4),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2)
            }));

            Assert.That(board.GetBoosterTargets(target, BoosterType.Area), Is.EquivalentTo(new[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3),
                new Vector2Int(3, 3)
            }));
        }

        [Test]
        public void GetBoosterTargets_SkipsOutOfBoundsHolesAndMissingPieces()
        {
            var board = CreateBoardModel(3, 3, CreateFilledSourceTileIds(3, 3));
            board.GetCell(0, 1).piece = null;
            board.GetCell(1, 0).exists = false;
            board.GetCell(1, 0).piece = null;

            var targets = board.GetBoosterTargets(new Vector2Int(0, 0), BoosterType.Area);

            Assert.That(targets, Is.EquivalentTo(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 1)
            }));
        }

        [Test]
        public void GetBoosterTargets_IncludesLockedAndBlockerPiecesWithinPattern()
        {
            var board = CreateBoardModel(
                3,
                1,
                new[] { 2, 8, 13 });

            var targets = board.GetBoosterTargets(new Vector2Int(0, 0), BoosterType.Row);

            Assert.That(targets, Is.EquivalentTo(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            }));
        }

        [Test]
        public void CanTargetBoosterAt_OnlyReturnsTrueForSelectableColoredPieces()
        {
            var board = CreateBoardModel(
                7,
                1,
                new[] { 2, 7, 12, 15, 2, 2, 1 });

            board.GetCell(0, 4).piece.movable = false;
            board.GetCell(0, 5).piece = null;

            Assert.That(board.CanTargetBoosterAt(new Vector2Int(0, 0)), Is.True);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(-1, 0)), Is.False);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(1, 0)), Is.False);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(2, 0)), Is.False);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(3, 0)), Is.False);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(4, 0)), Is.False);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(5, 0)), Is.False);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(6, 0)), Is.False);
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(7, 0)), Is.False);
        }

        [Test]
        public void CanTargetBoosterAt_ReturnsFalseAfterBoardHasEnded()
        {
            var board = CreateBoardModel(1, 1, new[] { 2 });
            for (var tick = 0; tick < 60; tick++)
            {
                board.TickSecond();
            }

            Assert.That(board.FinalizePendingEndState(), Is.True);
            Assert.That(board.EndState, Is.EqualTo(BoardEndState.Lose));
            Assert.That(board.CanTargetBoosterAt(new Vector2Int(0, 0)), Is.False);
        }

        [Test]
        public void ApplyBooster_UsesSameTargetCoordinatesAsPreviewHelper()
        {
            var board = CreateBoardModel(3, 3, CreateFilledSourceTileIds(3, 3));
            var previewTargets = board.GetBoosterTargets(new Vector2Int(1, 1), BoosterType.Cross);

            var report = board.ApplyBooster(new Vector2Int(1, 1), BoosterType.Cross);
            var clearPhase = FindPhase(report, BoardVisualPhaseKind.Clear);

            Assert.That(clearPhase, Is.Not.Null);
            Assert.That(CollectEmptyCoordinates(clearPhase.snapshot), Is.EquivalentTo(previewTargets));
        }

        private static BoardVisualPhase FindPhase(BoardActionReport report, BoardVisualPhaseKind kind)
        {
            foreach (var phase in report.phases)
            {
                if (phase.kind == kind)
                {
                    return phase;
                }
            }

            return null;
        }

        private static HashSet<int> CollectRuntimeIds(BoardVisualSnapshot snapshot)
        {
            var runtimeIds = new HashSet<int>();
            for (var row = 0; row < snapshot.Height; row++)
            {
                for (var column = 0; column < snapshot.Width; column++)
                {
                    var cell = snapshot.GetCell(row, column);
                    if (cell != null && cell.pieceRuntimeId != 0)
                    {
                        runtimeIds.Add(cell.pieceRuntimeId);
                    }
                }
            }

            return runtimeIds;
        }

        private static List<Vector2Int> CollectEmptyCoordinates(BoardVisualSnapshot snapshot)
        {
            var coordinates = new List<Vector2Int>();
            for (var row = 0; row < snapshot.Height; row++)
            {
                for (var column = 0; column < snapshot.Width; column++)
                {
                    var cell = snapshot.GetCell(row, column);
                    if (cell != null && cell.exists && cell.pieceRuntimeId == 0)
                    {
                        coordinates.Add(new Vector2Int(column, row));
                    }
                }
            }

            return coordinates;
        }

        private static int[] CreateFilledSourceTileIds(int boardWidth, int boardHeight)
        {
            var sourceTileIds = new int[boardWidth * boardHeight];
            for (var index = 0; index < sourceTileIds.Length; index++)
            {
                sourceTileIds[index] = 2 + (index % 5);
            }

            return sourceTileIds;
        }

        private static int[] CreateBackgroundMask(int boardWidth, int boardHeight, params Vector2Int[] iceCells)
        {
            var backgroundWidth = boardWidth + 2;
            var backgroundHeight = boardHeight + 2;
            var mask = new int[backgroundWidth * backgroundHeight];
            foreach (var iceCell in iceCells)
            {
                var index = ((iceCell.y + 1) * backgroundWidth) + (iceCell.x + 1);
                mask[index] = 3;
            }

            return mask;
        }

        private static BoardModel CreateBoardModel(int topLeft, int topRight, int bottomLeft, int bottomRight)
        {
            return CreateBoardModel(
                2,
                2,
                new[] { topLeft, topRight, bottomLeft, bottomRight });
        }

        private static BoardModel CreateBoardModel(int boardWidth, int boardHeight, int[] sourceTileIds, int[] backgroundMask = null)
        {
            var backgroundWidth = boardWidth + 2;
            var backgroundHeight = boardHeight + 2;
            var board = new GridCellSpec[sourceTileIds.Length];
            for (var index = 0; index < sourceTileIds.Length; index++)
            {
                board[index] = LegacyTileDecoder.Decode(sourceTileIds[index]);
            }

            var definition = new LevelDefinition
            {
                levelNumber = 99,
                boardWidth = boardWidth,
                boardHeight = boardHeight,
                backgroundWidth = backgroundWidth,
                backgroundHeight = backgroundHeight,
                board = board,
                backgroundMask = backgroundMask ?? new int[backgroundWidth * backgroundHeight],
                objectives = new[]
                {
                    new ObjectiveDefinition
                    {
                        type = ObjectiveType.Red,
                        targetCount = 99
                    }
                },
                moveCount = 10,
                timerSeconds = 60
            };

            return new BoardModel(definition, 12345);
        }
    }
}
