using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
        public sealed class LegacyGridSpriteSheet
        {
            private readonly Texture2D _texture;
            private readonly int _cellWidth;
            private readonly int _cellHeight;
            private readonly int _columns;
            private readonly Dictionary<int, Sprite> _spriteCache = new Dictionary<int, Sprite>();

            public LegacyGridSpriteSheet(Texture2D texture, int cellWidth, int cellHeight)
            {
                _texture = texture;
                _cellWidth = cellWidth;
                _cellHeight = cellHeight;
                _columns = texture != null ? Mathf.Max(1, texture.width / cellWidth) : 1;
            }

            public Sprite GetSprite(int frameIndex)
            {
                if (_texture == null)
                {
                    return null;
                }

                if (_spriteCache.TryGetValue(frameIndex, out var cachedSprite))
                {
                    return cachedSprite;
                }

                var column = frameIndex % _columns;
                var row = frameIndex / _columns;
                var x = column * _cellWidth;
                var y = _texture.height - ((row + 1) * _cellHeight);
                var rect = new Rect(x, y, _cellWidth, _cellHeight);
                var sprite = Sprite.Create(_texture, rect, new Vector2(0.5f, 0.5f), 100f);
                _spriteCache[frameIndex] = sprite;
                return sprite;
            }
        }
}
