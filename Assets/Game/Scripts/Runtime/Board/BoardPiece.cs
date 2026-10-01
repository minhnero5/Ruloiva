using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
        public sealed class BoardPiece
        {
            public int runtimeId;
            public CellKind kind;
            public JellyColor color;
            public int durability;
            public bool movable;
            public int sourceTileId;
            public LockVisualStyle lockVisualStyle;
            public int doubleCount;

            public bool IsColored => kind == CellKind.Jelly && color != JellyColor.None;
            public bool IsBlocker => kind == CellKind.Blocker;
            public bool CanBeSelected => IsColored && durability == 0 && movable;

            public BoardPiece Clone()
            {
                return new BoardPiece
                {
                    runtimeId = runtimeId,
                    kind = kind,
                    color = color,
                    durability = durability,
                    movable = movable,
                    sourceTileId = sourceTileId,
                    lockVisualStyle = lockVisualStyle,
                    doubleCount = doubleCount
                };
            }

            public int GetFrameIndex()
            {
                if (IsBlocker)
                {
                    if (durability <= -3)
                    {
                        return 12;
                    }

                    if (durability == -2)
                    {
                        return 11;
                    }

                    return 10;
                }

                var colorIndex = (int)color;
                if (!IsColored)
                {
                    return -1;
                }

                return colorIndex;
            }

            public int GetLockBarCount()
            {
                if (!IsColored || durability <= 0)
                {
                    return 0;
                }

                return Mathf.Clamp(durability, 0, 3);
            }
        }
}
