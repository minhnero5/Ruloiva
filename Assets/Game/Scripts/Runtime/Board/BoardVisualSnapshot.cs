using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
        public sealed class BoardVisualCellSnapshot
        {
            public bool exists;
            public bool hasIce;
            public int pieceRuntimeId;
            public int frameIndex = -1;
            public int lockBarCount;
            public int doubleCount;
        }

        public sealed class BoardVisualSnapshot
        {
            private readonly BoardVisualCellSnapshot[,] _cells;

            public BoardVisualSnapshot(int width, int height)
            {
                Width = width;
                Height = height;
                _cells = new BoardVisualCellSnapshot[height, width];
                for (var row = 0; row < height; row++)
                {
                    for (var column = 0; column < width; column++)
                    {
                        _cells[row, column] = new BoardVisualCellSnapshot();
                    }
                }
            }

            public int Width { get; }
            public int Height { get; }

            public BoardVisualCellSnapshot GetCell(int row, int column)
            {
                if (row < 0 || row >= Height || column < 0 || column >= Width)
                {
                    return null;
                }

                return _cells[row, column];
            }

            public bool TryFindPiece(int runtimeId, out Vector2Int coordinate, out BoardVisualCellSnapshot cell)
            {
                for (var row = 0; row < Height; row++)
                {
                    for (var column = 0; column < Width; column++)
                    {
                        var candidate = _cells[row, column];
                        if (candidate.pieceRuntimeId != runtimeId)
                        {
                            continue;
                        }

                        coordinate = new Vector2Int(column, row);
                        cell = candidate;
                        return true;
                    }
                }

                coordinate = default;
                cell = null;
                return false;
            }
        }
}
