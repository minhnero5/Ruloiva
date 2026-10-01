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
        public sealed class LegacySpriteLibrary
        {
            private readonly Dictionary<string, Sprite> _fullTextureSpriteCache = new Dictionary<string, Sprite>();
            private readonly Dictionary<string, AudioClip> _audioClipCache = new Dictionary<string, AudioClip>();
            private readonly Dictionary<string, Sprite> _sheetSpriteCache = new Dictionary<string, Sprite>();
            private readonly Dictionary<string, LegacyGridSpriteSheet> _gridSheets = new Dictionary<string, LegacyGridSpriteSheet>();

            private Texture2D _sheetTexture;
            private TextAsset _sheetXml;
            private Font _cachedFont;

            public Font MainFont
            {
                get
                {
                    if (_cachedFont == null)
                    {
                        _cachedFont = Resources.Load<Font>("JellyQuest/Raw/assets/fnt/Acme-Regular");
                        if (_cachedFont == null)
                        {
                            _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        }
                    }

                    return _cachedFont;
                }
            }

            public Sprite GetFullTextureSprite(string resourcePath)
            {
                if (_fullTextureSpriteCache.TryGetValue(resourcePath, out var cachedSprite))
                {
                    return cachedSprite;
                }

                var texture = Resources.Load<Texture2D>(resourcePath);
                if (texture == null)
                {
                    return null;
                }

                var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                _fullTextureSpriteCache[resourcePath] = sprite;
                return sprite;
            }

            public Sprite GetSheetSprite(string spriteName)
            {
                if (_sheetSpriteCache.TryGetValue(spriteName, out var cachedSprite))
                {
                    return cachedSprite;
                }

                EnsureSheetAtlas();
                if (_sheetTexture == null || _sheetXml == null)
                {
                    return null;
                }

                var xml = new XmlDocument();
                xml.LoadXml(_sheetXml.text);
                var nodes = xml.SelectNodes("//SubTexture");
                if (nodes == null)
                {
                    return null;
                }

                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null)
                    {
                        continue;
                    }

                    var name = node.Attributes["name"]?.Value;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var x = int.Parse(node.Attributes["x"].Value);
                    var y = int.Parse(node.Attributes["y"].Value);
                    var width = int.Parse(node.Attributes["width"].Value);
                    var height = int.Parse(node.Attributes["height"].Value);
                    var rect = new Rect(x, _sheetTexture.height - y - height, width, height);
                    _sheetSpriteCache[name] = Sprite.Create(_sheetTexture, rect, new Vector2(0.5f, 0.5f), 100f);
                }

                _sheetSpriteCache.TryGetValue(spriteName, out cachedSprite);
                return cachedSprite;
            }

            public Sprite GetObjectSprite(int frameIndex)
            {
                return GetGridSheet("JellyQuest/Raw/assets/objects2", 80, 80).GetSprite(frameIndex);
            }

            public Sprite GetDestroyEffectSprite(int frameIndex)
            {
                return GetGridSheet("JellyQuest/Raw/assets/died_animation2", 90, 90).GetSprite(frameIndex);
            }

            public Sprite GetObjectiveCounterSprite(int frameIndex)
            {
                return GetGridSheet("JellyQuest/Raw/assets/objectifs_cnt", 116, 40).GetSprite(frameIndex);
            }

            public Sprite GetLevelButtonSprite(int frameIndex)
            {
                return GetGridSheet("JellyQuest/Raw/assets/levelbnt2", 81, 105).GetSprite(frameIndex);
            }

            public Sprite GetBoardTileSprite(int frameIndex = 0)
            {
                return GetGridSheet("JellyQuest/Raw/assets/tile2", 80, 80).GetSprite(frameIndex);
            }

            public AudioClip GetAudioClip(string resourcePath)
            {
                if (_audioClipCache.TryGetValue(resourcePath, out var cachedClip))
                {
                    return cachedClip;
                }

                var clip = Resources.Load<AudioClip>(resourcePath);
                if (clip != null)
                {
                    _audioClipCache[resourcePath] = clip;
                }

                return clip;
            }

            private LegacyGridSpriteSheet GetGridSheet(string resourcePath, int cellWidth, int cellHeight)
            {
                if (_gridSheets.TryGetValue(resourcePath, out var gridSheet))
                {
                    return gridSheet;
                }

                var texture = Resources.Load<Texture2D>(resourcePath);
                gridSheet = new LegacyGridSpriteSheet(texture, cellWidth, cellHeight);
                _gridSheets[resourcePath] = gridSheet;
                return gridSheet;
            }

            private void EnsureSheetAtlas()
            {
                if (_sheetTexture != null && _sheetXml != null)
                {
                    return;
                }

                _sheetTexture = Resources.Load<Texture2D>("JellyQuest/Raw/assets/sheet");
                _sheetXml = Resources.Load<TextAsset>("JellyQuest/Raw/assets/sheet");
            }
        }
}
