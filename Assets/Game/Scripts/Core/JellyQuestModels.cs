using System;
using UnityEngine;

namespace JellyQuestUnity
{
    public enum JellyColor
    {
        None = -1,
        Red = 0,
        Green = 1,
        Yellow = 2,
        Pink = 3,
        Blue = 4
    }

    public enum CellKind
    {
        Hole = 0,
        Jelly = 1,
        Blocker = 2
    }

    public enum ObjectiveType
    {
        Red = 0,
        Green = 1,
        Yellow = 2,
        Pink = 3,
        Blue = 4,
        Ice = 5
    }

    public enum BoosterType
    {
        Single = 0,
        Column = 1,
        Row = 2,
        Cross = 3,
        Area = 4
    }

    public enum LockVisualStyle
    {
        None = 0,
        Simple = 1,
        Layered = 2
    }

    [Serializable]
    public sealed class GridCellSpec
    {
        public CellKind kind;
        public JellyColor color;
        public int durability;
        public bool movable;
        public int sourceTileId;
        public LockVisualStyle lockVisualStyle;
    }

    [Serializable]
    public sealed class ObjectiveDefinition
    {
        public ObjectiveType type;
        public int targetCount;
    }

    [Serializable]
    public sealed class MapNodePosition
    {
        public int x;
        public int y;
        public int num;
    }

    [Serializable]
    public sealed class BoosterUnlockDefinition
    {
        public int level;
        public int price;
    }

    [Serializable]
    public sealed class BoosterUnlockTable
    {
        public BoosterUnlockDefinition single;
        public BoosterUnlockDefinition column;
        public BoosterUnlockDefinition row;
        public BoosterUnlockDefinition area;
        public BoosterUnlockDefinition cross;
    }

    [Serializable]
    public sealed class LevelDefinition
    {
        public int levelNumber;
        public int boardWidth;
        public int boardHeight;
        public int backgroundWidth;
        public int backgroundHeight;
        public GridCellSpec[] board;
        public int[] backgroundMask;
        public ObjectiveDefinition[] objectives;
        public int moveCount;
        public int timerSeconds;

        public GridCellSpec GetCell(int row, int column)
        {
            if (row < 0 || row >= boardHeight || column < 0 || column >= boardWidth)
            {
                return null;
            }

            return board[(row * boardWidth) + column];
        }

        public int GetBackgroundMaskValue(int row, int column)
        {
            if (row < 0 || row >= boardHeight || column < 0 || column >= boardWidth || backgroundMask == null)
            {
                return 0;
            }

            var mapRow = row + 1;
            var mapColumn = column + 1;
            var mapIndex = (mapRow * backgroundWidth) + mapColumn;
            return mapIndex >= 0 && mapIndex < backgroundMask.Length ? backgroundMask[mapIndex] : 0;
        }

        public bool HasIce(int row, int column)
        {
            return GetBackgroundMaskValue(row, column) == 3;
        }
    }

    [Serializable]
    public sealed class BoosterInventory
    {
        public int single;
        public int column;
        public int row;
        public int cross;
        public int area;

        public int GetCount(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.Single:
                    return single;
                case BoosterType.Column:
                    return column;
                case BoosterType.Row:
                    return row;
                case BoosterType.Cross:
                    return cross;
                case BoosterType.Area:
                    return area;
                default:
                    return 0;
            }
        }

        public void Add(BoosterType type, int amount)
        {
            SetCount(type, GetCount(type) + amount);
        }

        public void SetCount(BoosterType type, int amount)
        {
            amount = Mathf.Max(0, amount);
            switch (type)
            {
                case BoosterType.Single:
                    single = amount;
                    break;
                case BoosterType.Column:
                    column = amount;
                    break;
                case BoosterType.Row:
                    row = amount;
                    break;
                case BoosterType.Cross:
                    cross = amount;
                    break;
                case BoosterType.Area:
                    area = amount;
                    break;
            }
        }

        public void ClampToNonNegative()
        {
            single = Mathf.Max(0, single);
            column = Mathf.Max(0, column);
            row = Mathf.Max(0, row);
            cross = Mathf.Max(0, cross);
            area = Mathf.Max(0, area);
        }

        public bool TryConsume(BoosterType type)
        {
            if (GetCount(type) <= 0)
            {
                return false;
            }

            Add(type, -1);
            return true;
        }
    }

    [Serializable]
    public sealed class SaveProfile
    {
        public int highestUnlockedLevel = 1;
        public int[] starsByLevel = new int[50];
        public int coins;
        public BoosterInventory boosters = new BoosterInventory();
        public bool musicEnabled = true;
    }

    [Serializable]
    public sealed class LegacyCatalogDto
    {
        public int catalogVersion;
        public int resolution;
        public int boardWidth;
        public int boardHeight;
        public int backgroundWidth;
        public int backgroundHeight;
        public int[] tutorialLevels;
        public BoosterUnlockTable boosterUnlocks;
        public MapNodePosition[] mapNodePositions;
        public LegacyLevelDto[] levels;
    }

    [Serializable]
    public sealed class LegacyLevelDto
    {
        public int levelNumber;
        public int[] boardTiles;
        public int[] backgroundTiles;
        public int[] objectiveTypes;
        public int[] objectiveCounts;
        public int moveCount;
        public int timerSeconds;
    }

    public static class LegacyTileDecoder
    {
        public static GridCellSpec Decode(int tileId)
        {
            if (tileId == 1)
            {
                return new GridCellSpec
                {
                    kind = CellKind.Hole,
                    color = JellyColor.None,
                    durability = 0,
                    movable = false,
                    sourceTileId = tileId,
                    lockVisualStyle = LockVisualStyle.None
                };
            }

            if (tileId >= 2 && tileId <= 6)
            {
                return CreateJelly(tileId - 2, 0, true, tileId, LockVisualStyle.None);
            }

            if (tileId >= 7 && tileId <= 11)
            {
                return CreateJelly(tileId - 7, 1, false, tileId, LockVisualStyle.Simple);
            }

            if (tileId >= 15 && tileId <= 29)
            {
                var localIndex = tileId - 15;
                var color = localIndex / 3;
                var durability = (localIndex % 3) + 1;
                return CreateJelly(color, durability, false, tileId, LockVisualStyle.Layered);
            }

            if (tileId >= 12 && tileId <= 14)
            {
                return new GridCellSpec
                {
                    kind = CellKind.Blocker,
                    color = JellyColor.None,
                    durability = -(tileId - 11),
                    movable = false,
                    sourceTileId = tileId,
                    lockVisualStyle = LockVisualStyle.None
                };
            }

            return new GridCellSpec
            {
                kind = CellKind.Hole,
                color = JellyColor.None,
                durability = 0,
                movable = false,
                sourceTileId = tileId,
                lockVisualStyle = LockVisualStyle.None
            };
        }

        public static ObjectiveType ToObjectiveType(int legacyType)
        {
            return legacyType == 5 ? ObjectiveType.Ice : (ObjectiveType)legacyType;
        }

        private static GridCellSpec CreateJelly(int colorIndex, int durability, bool movable, int tileId, LockVisualStyle visualStyle)
        {
            return new GridCellSpec
            {
                kind = CellKind.Jelly,
                color = (JellyColor)colorIndex,
                durability = durability,
                movable = movable,
                sourceTileId = tileId,
                lockVisualStyle = visualStyle
            };
        }
    }
}
