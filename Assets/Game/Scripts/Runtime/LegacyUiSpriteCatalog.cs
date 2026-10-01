using UnityEngine;

namespace JellyQuestUnity
{
    [CreateAssetMenu(fileName = "LegacyUiSpriteCatalog", menuName = "Jelly Quest/Legacy UI Sprite Catalog")]
    public sealed class LegacyUiSpriteCatalog : ScriptableObject
    {
        public Sprite preloadBackground;
        public Sprite menuBackground;
        public Sprite map1Background;
        public Sprite map2Background;
        public Sprite map3Background;
        public Sprite logo;
        public Sprite startButton;
        public Sprite musicOnButton;
        public Sprite closeSmallButton;
        public Sprite menuShopOpenButton;
        public Sprite menuShopCloseButton;
        public Sprite menuShopItemBackground;
        public Sprite menuShopCoinIcon;
        public Sprite menuButton;
        public Sprite replayButton;
        public Sprite resumeButton;
        public Sprite pauseButton;
        public Sprite buyButton;
        public Sprite gameplayMovesPanel;
        public Sprite gameplayObjectivePanel;
        public Sprite gameplayObjectiveStrip;
        public Sprite gameplayTimerBarBackground;
        public Sprite gameplayTimerBarFill;
        public Sprite gameplayTimerClock;
        public Sprite gameplayTinyStar;
        public Sprite gameplayPauseOverlayPanel;
        public Sprite gameplayPauseOverlayTitle;
        public Sprite gameplayShopOverlayPanel;
        public Sprite gameplayShopOverlayTitle;
        public Sprite gameplayWinOverlayPanel;
        public Sprite gameplayWinOverlayTitle;
        public Sprite gameplayLoseOverlayPanel;
        public Sprite gameplayLoseOverlayTitle;
        public Sprite gameplayTutorialOverlayPanel;
        public Sprite gameplayWinLeftStar;
        public Sprite gameplayWinCenterStar;
        public Sprite gameplayWinRightStar;
        public Sprite gameplayBackgroundLevels1To16;
        public Sprite gameplayBackgroundLevels17To32;
        public Sprite gameplayBackgroundLevels33Plus;
        public Sprite gameplayBoardTile;
        public Sprite[] gameplayBoardTiles = new Sprite[3];
        public Sprite gameplayIce;
        public Sprite[] gameplayPieceSprites = new Sprite[5];
        public Sprite gameplayPieceLockBar;
        public Sprite[] gameplaySimpleLockedPieceSprites = new Sprite[5];
        public Sprite[] gameplayBlockerSprites = new Sprite[3];
        public Sprite[] gameplayLayeredLockedPieceSprites = new Sprite[15];
        public Sprite[] levelButtonSprites = new Sprite[25];
        public Sprite[] objectiveSprites = new Sprite[5];
        public Sprite iceObjectiveSprite;
        public Sprite singleBoosterActive;
        public Sprite columnBoosterActive;
        public Sprite rowBoosterActive;
        public Sprite crossBoosterActive;
        public Sprite areaBoosterActive;
        public Sprite singleBoosterLocked;
        public Sprite columnBoosterLocked;
        public Sprite rowBoosterLocked;
        public Sprite crossBoosterLocked;
        public Sprite areaBoosterLocked;
        public Sprite gameplayBoosterPreviewSprite;
        public Color gameplayBoosterPreviewHintColor = new Color(0.38f, 0.88f, 1f, 1f);
        public Color gameplayBoosterPreviewTargetColor = new Color(1f, 0.8f, 0.18f, 1f);
        public Color gameplayBoosterActiveButtonColor = new Color(1f, 0.95f, 0.68f, 1f);

        public Sprite GetMusicSprite()
        {
            return musicOnButton;
        }

        public Sprite GetLevelButtonSprite(int frameIndex)
        {
            if (levelButtonSprites == null || frameIndex < 0 || frameIndex >= levelButtonSprites.Length)
            {
                return null;
            }

            return levelButtonSprites[frameIndex];
        }

        public Sprite GetLevelButtonSprite(bool unlocked)
        {
            return GetLevelButtonSprite(unlocked ? 1 : 0);
        }

        public Sprite GetObjectiveSprite(ObjectiveType type)
        {
            if (type == ObjectiveType.Ice)
            {
                return iceObjectiveSprite;
            }

            var index = (int)type;
            if (objectiveSprites == null || index < 0 || index >= objectiveSprites.Length)
            {
                return null;
            }

            return objectiveSprites[index];
        }

        public Sprite GetBoosterSprite(BoosterType type, bool unlocked)
        {
            switch (type)
            {
                case BoosterType.Single:
                    return unlocked ? singleBoosterActive : singleBoosterLocked;
                case BoosterType.Column:
                    return unlocked ? columnBoosterActive : columnBoosterLocked;
                case BoosterType.Row:
                    return unlocked ? rowBoosterActive : rowBoosterLocked;
                case BoosterType.Cross:
                    return unlocked ? crossBoosterActive : crossBoosterLocked;
                case BoosterType.Area:
                    return unlocked ? areaBoosterActive : areaBoosterLocked;
                default:
                    return null;
            }
        }
    }
}
