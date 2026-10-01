using UnityEngine;

namespace JellyQuestUnity
{
    internal static class LegacyUiSpriteLookup
    {
        private const string GameplayPieceLockBarResourcePath = "JellyQuest/Raw/assets/piece_lock_bar";

        internal static Sprite GetPreloadBackground(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.preloadBackground : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/bgPreload2") : null);
        }

        internal static Sprite GetMenuBackground(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.menuBackground : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/bgMenu2") : null);
        }

        internal static Sprite GetMap1Background(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.map1Background : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/map1") : null);
        }

        internal static Sprite GetMap2Background(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.map2Background : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/map2") : null);
        }

        internal static Sprite GetMap3Background(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.map3Background : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/map3") : null);
        }

        internal static Sprite GetLogo(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.logo : null, sprites != null ? sprites.GetSheetSprite("logo.png") : null);
        }

        internal static Sprite GetStartButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.startButton : null, sprites != null ? sprites.GetSheetSprite("start2.png") : null);
        }

        internal static Sprite GetMusicSprite(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            var sprite = catalog != null ? catalog.GetMusicSprite() : null;
            return First(sprite, sprites != null ? sprites.GetSheetSprite("musicbnt2.png") : null);
        }

        internal static Sprite GetCloseSmallButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.closeSmallButton : null, sprites != null ? sprites.GetSheetSprite("closeS_bnt.png") : null);
        }

        internal static Sprite GetMenuShopOpenButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.menuShopOpenButton : null;
        }

        internal static Sprite GetMenuShopCloseButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.menuShopCloseButton : null;
        }

        internal static Sprite GetMenuShopItemBackground(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.menuShopItemBackground : null;
        }

        internal static Sprite GetMenuShopCoinIcon(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.menuShopCoinIcon : null;
        }

        internal static Sprite GetMenuButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.menuButton : null, sprites != null ? sprites.GetSheetSprite("menubnt2.png") : null);
        }

        internal static Sprite GetReplayButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.replayButton : null, sprites != null ? sprites.GetSheetSprite("replaybnt2.png") : null);
        }

        internal static Sprite GetResumeButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.resumeButton : null, sprites != null ? sprites.GetSheetSprite("resumebnt2.png") : null);
        }

        internal static Sprite GetPauseButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.pauseButton : null, sprites != null ? sprites.GetSheetSprite("pausebnt2.png") : null);
        }

        internal static Sprite GetBuyButton(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.buyButton : null, sprites != null ? sprites.GetSheetSprite("buy_bnt.png") : null);
        }

        internal static Sprite GetGameplayMovesPanel(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayMovesPanel : null, sprites != null ? sprites.GetSheetSprite("container_stars2.png") : null);
        }

        internal static Sprite GetGameplayObjectivePanel(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayObjectivePanel : null, sprites != null ? sprites.GetSheetSprite("top_container2.png") : null);
        }

        internal static Sprite GetGameplayObjectiveStrip(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayObjectiveStrip : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/objectifs_cnt") : null);
        }

        internal static Sprite GetGameplayTimerBarBackground(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayTimerBarBackground : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/load_bar2") : null);
        }

        internal static Sprite GetGameplayTimerBarFill(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayTimerBarFill : null, sprites != null ? sprites.GetFullTextureSprite("JellyQuest/Raw/assets/bar_stars2") : null);
        }

        internal static Sprite GetGameplayTimerClock(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayTimerClock : null, sprites != null ? sprites.GetSheetSprite("timer_clock.png") : null);
        }

        internal static Sprite GetGameplayTinyStar(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayTinyStar : null, sprites != null ? sprites.GetSheetSprite("star_play.png") : null);
        }

        internal static Sprite GetGameplayPauseOverlayPanel(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayPauseOverlayPanel : null, sprites != null ? sprites.GetSheetSprite("bgPause2.png") : null);
        }

        internal static Sprite GetGameplayPauseOverlayTitle(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.gameplayPauseOverlayTitle : null;
        }

        internal static Sprite GetGameplayShopOverlayPanel(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayShopOverlayPanel : null, sprites != null ? sprites.GetSheetSprite("shop_cnt.png") : null);
        }

        internal static Sprite GetGameplayShopOverlayTitle(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.gameplayShopOverlayTitle : null;
        }

        internal static Sprite GetGameplayWinOverlayPanel(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayWinOverlayPanel : null, sprites != null ? sprites.GetSheetSprite("bgwin2.png") : null);
        }

        internal static Sprite GetGameplayWinOverlayTitle(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.gameplayWinOverlayTitle : null;
        }

        internal static Sprite GetGameplayLoseOverlayPanel(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayLoseOverlayPanel : null, sprites != null ? sprites.GetSheetSprite("game_over.png") : null);
        }

        internal static Sprite GetGameplayLoseOverlayTitle(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            _ = sprites;
            return catalog != null ? catalog.gameplayLoseOverlayTitle : null;
        }

        internal static Sprite GetGameplayTutorialOverlayPanel(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayTutorialOverlayPanel : null, sprites != null ? sprites.GetSheetSprite("container_tuto2.png") : null);
        }

        internal static Sprite GetGameplayWinLeftStar(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayWinLeftStar : null, sprites != null ? sprites.GetSheetSprite("star_left.png") : null);
        }

        internal static Sprite GetGameplayWinCenterStar(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayWinCenterStar : null, sprites != null ? sprites.GetSheetSprite("full_star2.png") : null);
        }

        internal static Sprite GetGameplayWinRightStar(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayWinRightStar : null, sprites != null ? sprites.GetSheetSprite("star_right.png") : null);
        }

        internal static Sprite GetGameplayBackground(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, int levelNumber)
        {
            if (catalog != null)
            {
                if (levelNumber > 32 && catalog.gameplayBackgroundLevels33Plus != null)
                {
                    return catalog.gameplayBackgroundLevels33Plus;
                }

                if (levelNumber >= 17 && catalog.gameplayBackgroundLevels17To32 != null)
                {
                    return catalog.gameplayBackgroundLevels17To32;
                }

                if (catalog.gameplayBackgroundLevels1To16 != null)
                {
                    return catalog.gameplayBackgroundLevels1To16;
                }
            }

            if (sprites == null)
            {
                return null;
            }

            var resourcePath = levelNumber > 32
                ? "JellyQuest/Raw/assets/bgPlay4"
                : (levelNumber >= 17 ? "JellyQuest/Raw/assets/bgPlay3" : "JellyQuest/Raw/assets/bgPlay2");
            return sprites.GetFullTextureSprite(resourcePath);
        }

        internal static Sprite GetGameplayBoardTile(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return GetGameplayBoardTile(catalog, sprites, 1);
        }

        internal static Sprite GetGameplayBoardTile(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, int backgroundMaskValue)
        {
            _ = backgroundMaskValue;

            if (catalog != null)
            {
                if (catalog.gameplayBoardTile != null)
                {
                    return catalog.gameplayBoardTile;
                }

                var legacySprite = GetLegacyGameplayBoardTile(catalog.gameplayBoardTiles);
                if (legacySprite != null)
                {
                    return legacySprite;
                }
            }

            return sprites != null ? sprites.GetBoardTileSprite(0) : null;
        }

        internal static Sprite GetGameplayBoosterPreviewSprite(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, int backgroundMaskValue)
        {
            if (catalog != null && catalog.gameplayBoosterPreviewSprite != null)
            {
                return catalog.gameplayBoosterPreviewSprite;
            }

            return GetGameplayBoardTile(catalog, sprites, backgroundMaskValue);
        }

        internal static Sprite GetGameplayIce(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            return First(catalog != null ? catalog.gameplayIce : null, sprites != null ? sprites.GetSheetSprite("case2.png") : null);
        }

        internal static Sprite GetGameplayPieceSprite(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, int frameIndex)
        {
            if (frameIndex < 0)
            {
                return null;
            }

            var sprite = GetGameplayPieceCatalogSprite(catalog, frameIndex);
            return First(sprite, sprites != null ? sprites.GetObjectSprite(frameIndex) : null);
        }

        internal static Sprite GetGameplayPieceLockBar(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            if (catalog != null && catalog.gameplayPieceLockBar != null)
            {
                return catalog.gameplayPieceLockBar;
            }

            if (sprites != null)
            {
                return sprites.GetFullTextureSprite(GameplayPieceLockBarResourcePath);
            }

            return Resources.Load<Sprite>(GameplayPieceLockBarResourcePath);
        }

        internal static Sprite GetLevelButtonSprite(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, int frameIndex)
        {
            var sprite = catalog != null ? catalog.GetLevelButtonSprite(frameIndex) : null;
            return First(sprite, sprites != null ? sprites.GetLevelButtonSprite(frameIndex) : null);
        }

        internal static Sprite GetLevelButtonSprite(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, bool unlocked)
        {
            var sprite = catalog != null ? catalog.GetLevelButtonSprite(unlocked) : null;
            return First(sprite, sprites != null ? sprites.GetLevelButtonSprite(unlocked ? 1 : 0) : null);
        }

        internal static Sprite GetObjectiveSprite(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, ObjectiveType type)
        {
            var sprite = catalog != null ? catalog.GetObjectiveSprite(type) : null;
            if (sprite != null)
            {
                return sprite;
            }

            if (sprites == null)
            {
                return null;
            }

            return type == ObjectiveType.Ice ? sprites.GetSheetSprite("case2.png") : sprites.GetObjectSprite((int)type);
        }

        internal static Sprite GetBoosterSprite(LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, BoosterType type, bool unlocked)
        {
            var sprite = catalog != null ? catalog.GetBoosterSprite(type, unlocked) : null;
            if (sprite != null)
            {
                return sprite;
            }

            if (sprites == null)
            {
                return null;
            }

            switch (type)
            {
                case BoosterType.Single:
                    return sprites.GetSheetSprite(unlocked ? "power_up_32.png" : "Gpower_up_32.png");
                case BoosterType.Column:
                    return sprites.GetSheetSprite(unlocked ? "power_up_02.png" : "Gpower_up_02.png");
                case BoosterType.Row:
                    return sprites.GetSheetSprite(unlocked ? "power_up_12.png" : "Gpower_up_12.png");
                case BoosterType.Cross:
                    return sprites.GetSheetSprite(unlocked ? "power_up_22.png" : "Gpower_up_22.png");
                case BoosterType.Area:
                    return sprites.GetSheetSprite(unlocked ? "power_up_42.png" : "Gpower_up_42.png");
                default:
                    return null;
            }
        }

        private static Sprite First(Sprite primary, Sprite fallback)
        {
            return primary != null ? primary : fallback;
        }

        private static Sprite GetGameplayPieceCatalogSprite(LegacyUiSpriteCatalog catalog, int frameIndex)
        {
            if (catalog == null || frameIndex < 0)
            {
                return null;
            }

            if (frameIndex < 5)
            {
                return GetArraySprite(catalog.gameplayPieceSprites, frameIndex);
            }

            if (frameIndex < 10)
            {
                return GetArraySprite(catalog.gameplaySimpleLockedPieceSprites, frameIndex - 5);
            }

            if (frameIndex < 13)
            {
                return GetArraySprite(catalog.gameplayBlockerSprites, frameIndex - 10);
            }

            if (frameIndex < 28)
            {
                return GetArraySprite(catalog.gameplayLayeredLockedPieceSprites, frameIndex - 13);
            }

            return null;
        }

        private static Sprite GetArraySprite(Sprite[] sprites, int index)
        {
            if (sprites == null || index < 0 || index >= sprites.Length)
            {
                return null;
            }

            return sprites[index];
        }

        private static int GetGameplayBoardTileSlotIndex(int backgroundMaskValue)
        {
            switch (backgroundMaskValue)
            {
                case 2:
                    return 1;
                case 4:
                    return 2;
                case 1:
                case 3:
                default:
                    return 0;
            }
        }

        private static Sprite GetLegacyGameplayBoardTile(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0)
            {
                return null;
            }

            if (sprites[0] != null)
            {
                return sprites[0];
            }

            for (var index = 1; index < sprites.Length; index++)
            {
                if (sprites[index] != null)
                {
                    return sprites[index];
                }
            }

            return null;
        }
    }
}
