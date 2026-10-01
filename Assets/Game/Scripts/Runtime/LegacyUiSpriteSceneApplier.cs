using UnityEngine;
using UnityEngine.UI;

namespace JellyQuestUnity
{
    internal static class LegacyUiSpriteSceneApplier
    {
        internal static readonly Color MusicEnabledTint = Color.white;
        internal static readonly Color MusicDisabledTint = new Color(0.5f, 0.5f, 0.5f, 1f);

        private static readonly ObjectiveType[] PreviewObjectiveTypes =
        {
            ObjectiveType.Red,
            ObjectiveType.Green,
            ObjectiveType.Yellow,
            ObjectiveType.Pink,
            ObjectiveType.Blue,
            ObjectiveType.Ice
        };

        internal static void Apply(BootScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            if (view == null)
            {
                return;
            }

            var spriteLibrary = sprites ?? new LegacySpriteLibrary();
            ApplyImageSprite(view.backgroundImage, LegacyUiSpriteLookup.GetPreloadBackground(catalog, spriteLibrary));
            ApplyImageSprite(view.logoImage, LegacyUiSpriteLookup.GetLogo(catalog, spriteLibrary));
        }

        internal static void Apply(MenuScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, bool musicEnabled)
        {
            if (view == null)
            {
                return;
            }

            var spriteLibrary = sprites ?? new LegacySpriteLibrary();
            ApplyImageSprite(view.backgroundImage, LegacyUiSpriteLookup.GetMenuBackground(catalog, spriteLibrary));
            ApplyImageSprite(view.logoImage, LegacyUiSpriteLookup.GetLogo(catalog, spriteLibrary));
            ApplyButtonSprite(view.startButton, LegacyUiSpriteLookup.GetStartButton(catalog, spriteLibrary));
            ApplyMusicButtonState(view.musicButtonImage, catalog, spriteLibrary, musicEnabled);
            ApplyButtonSprite(view.creditsCloseButton, LegacyUiSpriteLookup.GetCloseSmallButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.shopOpenButton, LegacyUiSpriteLookup.GetMenuShopOpenButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.shopCloseButton, LegacyUiSpriteLookup.GetMenuShopCloseButton(catalog, spriteLibrary));
            ApplyImageSprite(view.shopItemTemplate != null ? view.shopItemTemplate.GetComponent<Image>() : null, LegacyUiSpriteLookup.GetMenuShopItemBackground(catalog, spriteLibrary));
            ApplyImageSprite(view.shopItemCoinIcon, LegacyUiSpriteLookup.GetMenuShopCoinIcon(catalog, spriteLibrary));
        }

        internal static void Apply(MapScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            if (view == null)
            {
                return;
            }

            var spriteLibrary = sprites ?? new LegacySpriteLibrary();
            ApplyImageSprite(view.map1Image, LegacyUiSpriteLookup.GetMap1Background(catalog, spriteLibrary));
            ApplyImageSprite(view.map2Image, LegacyUiSpriteLookup.GetMap2Background(catalog, spriteLibrary));
            ApplyImageSprite(view.map3Image, LegacyUiSpriteLookup.GetMap3Background(catalog, spriteLibrary));
            ApplyButtonSprite(view.homeButton, LegacyUiSpriteLookup.GetMenuButton(catalog, spriteLibrary));
        }

        internal static void Apply(GameplayScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, bool musicEnabled)
        {
            if (view == null)
            {
                return;
            }

            var spriteLibrary = sprites ?? new LegacySpriteLibrary();
            ApplyImageSprite(view.legacyMovesPanelImage, LegacyUiSpriteLookup.GetGameplayMovesPanel(catalog, spriteLibrary));
            ApplyImageSprite(view.legacyObjectivePanelImage, LegacyUiSpriteLookup.GetGameplayObjectivePanel(catalog, spriteLibrary));
            ApplyImageSprite(view.legacyObjectiveStripImage, LegacyUiSpriteLookup.GetGameplayObjectiveStrip(catalog, spriteLibrary));
            ApplyImageSprite(view.legacyTimerBarBackgroundImage, LegacyUiSpriteLookup.GetGameplayTimerBarBackground(catalog, spriteLibrary));
            ApplyImageSprite(view.legacyTimerBarFillImage, LegacyUiSpriteLookup.GetGameplayTimerBarFill(catalog, spriteLibrary));
            ApplyImageSprite(view.legacyTimerClockImage, LegacyUiSpriteLookup.GetGameplayTimerClock(catalog, spriteLibrary));
            ApplyMusicButtonState(view.musicButtonImage, catalog, spriteLibrary, musicEnabled);
            ApplyButtonSprite(view.pauseButton, LegacyUiSpriteLookup.GetPauseButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.pauseResumeButton, LegacyUiSpriteLookup.GetResumeButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.pauseReplayButton, LegacyUiSpriteLookup.GetReplayButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.pauseMenuButton, LegacyUiSpriteLookup.GetMenuButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.shopBuyButton, LegacyUiSpriteLookup.GetBuyButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.shopCloseButton, LegacyUiSpriteLookup.GetCloseSmallButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.winReplayButton, LegacyUiSpriteLookup.GetReplayButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.winMapButton, LegacyUiSpriteLookup.GetMenuButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.loseReplayButton, LegacyUiSpriteLookup.GetReplayButton(catalog, spriteLibrary));
            ApplyButtonSprite(view.loseMapButton, LegacyUiSpriteLookup.GetMenuButton(catalog, spriteLibrary));
            SetImageSprite(view.shopIconImage, LegacyUiSpriteLookup.GetBoosterSprite(catalog, spriteLibrary, BoosterType.Single, true), true);
            ApplyOverlayPreview(
                view.pauseOverlay,
                LegacyUiSpriteLookup.GetGameplayPauseOverlayPanel(catalog, spriteLibrary),
                LegacyUiSpriteLookup.GetGameplayPauseOverlayTitle(catalog, spriteLibrary));
            ApplyOverlayPreview(
                view.shopOverlay,
                LegacyUiSpriteLookup.GetGameplayShopOverlayPanel(catalog, spriteLibrary),
                LegacyUiSpriteLookup.GetGameplayShopOverlayTitle(catalog, spriteLibrary));
            ApplyOverlayPreview(
                view.winOverlay,
                LegacyUiSpriteLookup.GetGameplayWinOverlayPanel(catalog, spriteLibrary),
                LegacyUiSpriteLookup.GetGameplayWinOverlayTitle(catalog, spriteLibrary));
            ApplyOverlayPreview(
                view.loseOverlay,
                LegacyUiSpriteLookup.GetGameplayLoseOverlayPanel(catalog, spriteLibrary),
                LegacyUiSpriteLookup.GetGameplayLoseOverlayTitle(catalog, spriteLibrary));
            ApplyOverlayPreview(
                view.tutorialOverlay,
                LegacyUiSpriteLookup.GetGameplayTutorialOverlayPanel(catalog, spriteLibrary),
                null);
            ApplyImageSprite(view.winLeftStarImage, LegacyUiSpriteLookup.GetGameplayWinLeftStar(catalog, spriteLibrary));
            ApplyImageSprite(view.winCenterStarImage, LegacyUiSpriteLookup.GetGameplayWinCenterStar(catalog, spriteLibrary));
            ApplyImageSprite(view.winRightStarImage, LegacyUiSpriteLookup.GetGameplayWinRightStar(catalog, spriteLibrary));
            ApplyObjectivePreview(view, catalog, spriteLibrary);
            ApplyBoosterPreview(view, catalog, spriteLibrary);
        }

        internal static RectTransform FindOverlayPanelTransform(GameObject overlay)
        {
            if (overlay == null)
            {
                return null;
            }

            return overlay.transform.Find("Panel") as RectTransform;
        }

        internal static Image FindOverlayPanelImage(GameObject overlay)
        {
            var panel = FindOverlayPanelTransform(overlay);
            return panel != null ? panel.GetComponent<Image>() : null;
        }

        internal static Image FindOverlayTitleImage(GameObject overlay)
        {
            var panel = FindOverlayPanelTransform(overlay);
            if (panel == null)
            {
                return null;
            }

            var title = panel.Find("Title");
            return title != null ? title.GetComponent<Image>() : null;
        }

        internal static void SetImageSprite(Image image, Sprite sprite, bool preserveAspect)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
        }

        internal static void ApplyMusicButtonState(Image image, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, bool enabled)
        {
            if (image == null)
            {
                return;
            }

            SetImageSprite(image, LegacyUiSpriteLookup.GetMusicSprite(catalog, sprites), true);
            image.color = enabled ? MusicEnabledTint : MusicDisabledTint;
        }

        private static void ApplyButtonSprite(Button button, Sprite sprite)
        {
            if (button == null)
            {
                return;
            }

            ApplyImageSprite(button.GetComponent<Image>(), sprite);
        }

        private static void ApplyImageSprite(Image image, Sprite sprite)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
        }

        private static void ApplyOverlayPreview(GameObject overlay, Sprite panelSprite, Sprite titleSprite)
        {
            ApplyImageSprite(FindOverlayPanelImage(overlay), panelSprite);
            ApplyImageSprite(FindOverlayTitleImage(overlay), titleSprite);
        }

        private static void ApplyObjectivePreview(GameplayScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            if (view.objectiveRowTemplate != null)
            {
                ApplyObjectiveRow(view.objectiveRowTemplate, catalog, sprites, PreviewObjectiveTypes[0]);
            }

            if (view.objectiveContainer == null)
            {
                return;
            }

            var rows = view.objectiveContainer.GetComponentsInChildren<ObjectiveRowView>(true);
            var previewIndex = 0;
            foreach (var row in rows)
            {
                if (row == null || row == view.objectiveRowTemplate)
                {
                    continue;
                }

                var type = PreviewObjectiveTypes[Mathf.Min(previewIndex, PreviewObjectiveTypes.Length - 1)];
                ApplyObjectiveRow(row, catalog, sprites, type);
                previewIndex += 1;
            }
        }

        private static void ApplyObjectiveRow(ObjectiveRowView row, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites, ObjectiveType type)
        {
            if (row == null)
            {
                return;
            }

            SetImageSprite(row.iconImage, LegacyUiSpriteLookup.GetObjectiveSprite(catalog, sprites, type), true);
        }

        private static void ApplyBoosterPreview(GameplayScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            if (view.boosterButtons == null)
            {
                return;
            }

            foreach (var boosterView in view.boosterButtons)
            {
                if (boosterView == null)
                {
                    continue;
                }

                SetImageSprite(boosterView.image, LegacyUiSpriteLookup.GetBoosterSprite(catalog, sprites, boosterView.boosterType, true), true);
            }
        }
    }
}
