using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JellyQuestUnity.Editor;

namespace JellyQuestUnity.EditorTests
{
    public sealed class JellyQuestSpriteConfigTests
    {
        [Test]
        public void LegacyUiSpriteCatalog_LoadsFromResources()
        {
            var catalog = Resources.Load<LegacyUiSpriteCatalog>(JellyQuestUnity.Editor.JellyQuestSpriteCatalogEditorUtility.CatalogResourcePath);

            Assert.That(catalog, Is.Not.Null);
        }

        [Test]
        public void SpriteConfigLayout_VisibleTabs_ShowSingleBoardTileAndHideLegacyArray()
        {
            Assert.That(ContainsField(JellyQuestSpriteConfigLayout.VisibleTabs, "gameplayBoardTile"), Is.True);

            var gameplayWorldTab = FindTab(JellyQuestSpriteConfigLayout.AllTabs, "Gameplay World");
            var boardTileField = GetField(gameplayWorldTab.Fields, "gameplayBoardTile");
            var legacyArray = GetArray(gameplayWorldTab.ArrayGroups, "gameplayBoardTiles");

            Assert.That(boardTileField.Usage, Is.EqualTo(SpriteConfigUsage.Runtime));
            Assert.That(legacyArray.Usage, Is.EqualTo(SpriteConfigUsage.LegacyFallback));
        }

        [Test]
        public void SpriteConfigLayout_VisibleTabs_KeepSharedFieldsInOriginalTabs()
        {
            AssertFieldAppearsInTabs("logo", "Boot", "Menu");
            AssertFieldAppearsInTabs("musicOnButton", "Menu", "Gameplay");
            AssertFieldAppearsInTabs("closeSmallButton", "Menu", "Gameplay");
            AssertFieldAppearsInTabs("menuButton", "Map", "Gameplay");
            Assert.That(ContainsField(JellyQuestSpriteConfigLayout.VisibleTabs, "musicOffButton"), Is.False);
            Assert.That(ContainsField(JellyQuestSpriteConfigLayout.VisibleTabs, "creditsButton"), Is.False);
        }

        [Test]
        public void SpriteConfigLayout_GameplayTab_KeepsOverlayTitlesNextToPanels()
        {
            var gameplayTab = FindTab(JellyQuestSpriteConfigLayout.VisibleTabs, "Gameplay");
            var fieldPropertyNames = GetFieldPropertyNames(gameplayTab.Fields);

            Assert.That(GetFieldIndex(fieldPropertyNames, "gameplayPauseOverlayTitle"), Is.EqualTo(GetFieldIndex(fieldPropertyNames, "gameplayPauseOverlayPanel") + 1));
            Assert.That(GetFieldIndex(fieldPropertyNames, "gameplayShopOverlayTitle"), Is.EqualTo(GetFieldIndex(fieldPropertyNames, "gameplayShopOverlayPanel") + 1));
            Assert.That(GetFieldIndex(fieldPropertyNames, "gameplayWinOverlayTitle"), Is.EqualTo(GetFieldIndex(fieldPropertyNames, "gameplayWinOverlayPanel") + 1));
            Assert.That(GetFieldIndex(fieldPropertyNames, "gameplayLoseOverlayTitle"), Is.EqualTo(GetFieldIndex(fieldPropertyNames, "gameplayLoseOverlayPanel") + 1));
        }

        [Test]
        public void SpriteConfigLayout_VisibleTabs_MatchExpectedTabOrderAndKeyMembership()
        {
            var visibleTabs = JellyQuestSpriteConfigLayout.VisibleTabs;

            Assert.That(GetTabNames(visibleTabs), Is.EqualTo(new[]
            {
                "Boot",
                "Menu",
                "Map",
                "Gameplay",
                "Gameplay World",
                "Gameplay Pieces"
            }));

            var menuTab = FindTab(visibleTabs, "Menu");
            Assert.That(GetFieldPropertyNames(menuTab.Fields), Is.EqualTo(new[]
            {
                "menuBackground",
                "logo",
                "startButton",
                "musicOnButton",
                "closeSmallButton",
                "menuShopOpenButton",
                "menuShopCloseButton",
                "menuShopItemBackground",
                "menuShopCoinIcon"
            }));

            var mapTab = FindTab(visibleTabs, "Map");
            var levelButtonsArray = GetArray(mapTab.ArrayGroups, "levelButtonSprites");
            Assert.That(GetFieldPropertyNames(mapTab.Fields), Is.EqualTo(new[]
            {
                "map1Background",
                "map2Background",
                "map3Background",
                "menuButton"
            }));
            Assert.That(GetArrayPropertyNames(mapTab.ArrayGroups), Is.EqualTo(new[] { "levelButtonSprites" }));
            Assert.That(levelButtonsArray.VisibleIndices, Is.EqualTo(new[]
            {
                0, 1
            }));
            Assert.That(levelButtonsArray.Labels, Is.EqualTo(new[]
            {
                "Locked",
                "Unlocked"
            }));

            var gameplayTab = FindTab(visibleTabs, "Gameplay");
            Assert.That(ContainsField(gameplayTab.Fields, "singleBoosterActive"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "columnBoosterActive"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "rowBoosterActive"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "crossBoosterActive"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "areaBoosterActive"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "singleBoosterLocked"), Is.False);
            Assert.That(ContainsField(gameplayTab.Fields, "columnBoosterLocked"), Is.False);
            Assert.That(ContainsField(gameplayTab.Fields, "rowBoosterLocked"), Is.False);
            Assert.That(ContainsField(gameplayTab.Fields, "crossBoosterLocked"), Is.False);
            Assert.That(ContainsField(gameplayTab.Fields, "areaBoosterLocked"), Is.False);
            Assert.That(ContainsField(gameplayTab.Fields, "gameplayBoosterPreviewSprite"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "gameplayBoosterPreviewHintColor"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "gameplayBoosterPreviewTargetColor"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "gameplayBoosterActiveButtonColor"), Is.True);
            Assert.That(ContainsField(gameplayTab.Fields, "iceObjectiveSprite"), Is.True);
            Assert.That(GetArrayPropertyNames(gameplayTab.ArrayGroups), Is.EqualTo(new[] { "objectiveSprites" }));

            var gameplayWorldTab = FindTab(visibleTabs, "Gameplay World");
            Assert.That(GetFieldPropertyNames(gameplayWorldTab.Fields), Is.EqualTo(new[]
            {
                "gameplayBackgroundLevels1To16",
                "gameplayBackgroundLevels17To32",
                "gameplayBackgroundLevels33Plus",
                "gameplayIce",
                "gameplayBoardTile"
            }));
            Assert.That(GetArrayPropertyNames(gameplayWorldTab.ArrayGroups), Is.Empty);

            var gameplayPiecesTab = FindTab(visibleTabs, "Gameplay Pieces");
            Assert.That(GetFieldPropertyNames(gameplayPiecesTab.Fields), Is.EqualTo(new[]
            {
                "gameplayPieceLockBar"
            }));
            Assert.That(GetArrayPropertyNames(gameplayPiecesTab.ArrayGroups), Is.EqualTo(new[]
            {
                "gameplayPieceSprites",
                "gameplayBlockerSprites"
            }));
        }

        [Test]
        public void SpriteConfigLayout_GameplayPieces_HidesLegacyLockedPieceArrays()
        {
            var gameplayPiecesTab = FindTab(JellyQuestSpriteConfigLayout.AllTabs, "Gameplay Pieces");
            var simpleLockedArray = GetArray(gameplayPiecesTab.ArrayGroups, "gameplaySimpleLockedPieceSprites");
            var layeredLockedArray = GetArray(gameplayPiecesTab.ArrayGroups, "gameplayLayeredLockedPieceSprites");

            Assert.That(simpleLockedArray.Usage, Is.EqualTo(SpriteConfigUsage.LegacyFallback));
            Assert.That(layeredLockedArray.Usage, Is.EqualTo(SpriteConfigUsage.LegacyFallback));
            Assert.That(ContainsField(JellyQuestSpriteConfigLayout.VisibleTabs, "gameplayPieceLockBar"), Is.True);
        }

        [Test]
        public void SpritePreviewMetadataUtility_ReturnsFalseForNullSprite()
        {
            Assert.That(JellyQuestSpritePreviewMetadataUtility.TryCreate(null, out var metadata), Is.False);
            Assert.That(metadata, Is.EqualTo(default(JellyQuestSpritePreviewMetadata)));
        }

        [Test]
        public void SpritePreviewMetadataUtility_UsesSpriteRectForOriginalSizeAndRatio()
        {
            var sprite = CreateSprite(300, 200, Color.white);

            Assert.That(JellyQuestSpritePreviewMetadataUtility.TryCreate(sprite, out var metadata), Is.True);
            Assert.That(metadata.OriginalWidth, Is.EqualTo(300));
            Assert.That(metadata.OriginalHeight, Is.EqualTo(200));
            Assert.That(metadata.SimplifiedWidth, Is.EqualTo(3));
            Assert.That(metadata.SimplifiedHeight, Is.EqualTo(2));
            Assert.That(metadata.DecimalRatio, Is.EqualTo(1.5d).Within(0.0001d));
            Assert.That(metadata.OriginalSizeLabel, Is.EqualTo("Original: 300 x 200 px"));
            Assert.That(metadata.RatioLabel, Is.EqualTo("Ratio: 3:2 (1.5)"));

            Object.DestroyImmediate(sprite);
        }

        [Test]
        public void SpritePreviewMetadataUtility_FormatsNonSquareRatioWithoutOverReducing()
        {
            var sprite = CreateSprite(127, 43, Color.white);

            Assert.That(JellyQuestSpritePreviewMetadataUtility.TryCreate(sprite, out var metadata), Is.True);
            Assert.That(metadata.SimplifiedWidth, Is.EqualTo(127));
            Assert.That(metadata.SimplifiedHeight, Is.EqualTo(43));
            Assert.That(metadata.DecimalRatio, Is.EqualTo(127d / 43d).Within(0.0001d));
            Assert.That(metadata.RatioLabel, Is.EqualTo("Ratio: 127:43 (2.953)"));

            Object.DestroyImmediate(sprite);
        }

        [Test]
        public void SpritePreviewMetadataUtility_UsesSubSpriteRectInsteadOfFullTextureSize()
        {
            var sprite = CreateSpriteFromRect(32, 16, new Rect(5f, 6f, 12f, 4f), Color.white);

            Assert.That(JellyQuestSpritePreviewMetadataUtility.TryCreate(sprite, out var metadata), Is.True);
            Assert.That(metadata.OriginalWidth, Is.EqualTo(12));
            Assert.That(metadata.OriginalHeight, Is.EqualTo(4));
            Assert.That(metadata.SimplifiedWidth, Is.EqualTo(3));
            Assert.That(metadata.SimplifiedHeight, Is.EqualTo(1));
            Assert.That(metadata.OriginalSizeLabel, Is.EqualTo("Original: 12 x 4 px"));
            Assert.That(metadata.RatioLabel, Is.EqualTo("Ratio: 3:1 (3)"));

            Object.DestroyImmediate(sprite);
        }

        [Test]
        public void PreviewUtility_AppliesBootMenuAndMapSceneOwnedSprites()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            catalog.preloadBackground = CreateSprite(Color.red);
            catalog.logo = CreateSprite(Color.green);
            catalog.menuBackground = CreateSprite(Color.blue);
            catalog.startButton = CreateSprite(Color.yellow);
            catalog.musicOnButton = CreateSprite(Color.magenta);
            catalog.closeSmallButton = CreateSprite(new Color(0.2f, 0.3f, 0.4f));
            catalog.menuShopOpenButton = CreateSprite(new Color(0.3f, 0.6f, 0.9f));
            catalog.menuShopCloseButton = CreateSprite(new Color(0.9f, 0.6f, 0.3f));
            catalog.menuShopItemBackground = CreateSprite(new Color(0.4f, 0.5f, 0.6f));
            catalog.menuShopCoinIcon = CreateSprite(new Color(0.8f, 0.7f, 0.2f));
            catalog.map1Background = CreateSprite(new Color(0.5f, 0.2f, 0.2f));
            catalog.map2Background = CreateSprite(new Color(0.2f, 0.5f, 0.2f));
            catalog.map3Background = CreateSprite(new Color(0.2f, 0.2f, 0.5f));
            catalog.menuButton = CreateSprite(new Color(0.9f, 0.4f, 0.1f));

            var bootView = CreateBootView();
            var bootBackgroundSceneSprite = CreateSprite(new Color(0.1f, 0.1f, 0.1f));
            var bootLogoSceneSprite = CreateSprite(new Color(0.2f, 0.2f, 0.2f));
            bootView.backgroundImage.sprite = bootBackgroundSceneSprite;
            bootView.logoImage.sprite = bootLogoSceneSprite;
            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToBoot(bootView, catalog);
            Assert.That(bootView.backgroundImage.sprite, Is.SameAs(catalog.preloadBackground));
            Assert.That(bootView.logoImage.sprite, Is.SameAs(catalog.logo));

            var menuView = CreateMenuView();
            var menuBackgroundSceneSprite = CreateSprite(new Color(0.15f, 0.15f, 0.15f));
            var menuLogoSceneSprite = CreateSprite(new Color(0.25f, 0.25f, 0.25f));
            var startButtonSceneSprite = CreateSprite(new Color(0.35f, 0.35f, 0.35f));
            var creditsButtonSceneSprite = CreateSprite(new Color(0.45f, 0.45f, 0.45f));
            var creditsCloseSceneSprite = CreateSprite(new Color(0.55f, 0.55f, 0.55f));
            var shopOpenSceneSprite = CreateSprite(new Color(0.56f, 0.56f, 0.56f));
            var shopCloseSceneSprite = CreateSprite(new Color(0.57f, 0.57f, 0.57f));
            var shopItemBackgroundSceneSprite = CreateSprite(new Color(0.58f, 0.58f, 0.58f));
            var shopCoinSceneSprite = CreateSprite(new Color(0.59f, 0.59f, 0.59f));
            menuView.backgroundImage.sprite = menuBackgroundSceneSprite;
            menuView.logoImage.sprite = menuLogoSceneSprite;
            menuView.startButton.GetComponent<Image>().sprite = startButtonSceneSprite;
            menuView.creditsButton.GetComponent<Image>().sprite = creditsButtonSceneSprite;
            menuView.creditsCloseButton.GetComponent<Image>().sprite = creditsCloseSceneSprite;
            menuView.shopOpenButton.GetComponent<Image>().sprite = shopOpenSceneSprite;
            menuView.shopCloseButton.GetComponent<Image>().sprite = shopCloseSceneSprite;
            menuView.shopItemTemplate.GetComponent<Image>().sprite = shopItemBackgroundSceneSprite;
            menuView.shopItemCoinIcon.sprite = shopCoinSceneSprite;
            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToMenu(menuView, catalog);
            Assert.That(menuView.backgroundImage.sprite, Is.SameAs(catalog.menuBackground));
            Assert.That(menuView.logoImage.sprite, Is.SameAs(catalog.logo));
            Assert.That(menuView.startButton.GetComponent<Image>().sprite, Is.SameAs(catalog.startButton));
            Assert.That(menuView.musicButtonImage.sprite, Is.SameAs(catalog.musicOnButton));
            Assert.That(menuView.musicButtonImage.color, Is.EqualTo(Color.white));
            Assert.That(menuView.creditsButton.GetComponent<Image>().sprite, Is.SameAs(creditsButtonSceneSprite));
            Assert.That(menuView.creditsCloseButton.GetComponent<Image>().sprite, Is.SameAs(catalog.closeSmallButton));
            Assert.That(menuView.shopOpenButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuShopOpenButton));
            Assert.That(menuView.shopCloseButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuShopCloseButton));
            Assert.That(menuView.shopItemTemplate.GetComponent<Image>().sprite, Is.SameAs(catalog.menuShopItemBackground));
            Assert.That(menuView.shopItemCoinIcon.sprite, Is.SameAs(catalog.menuShopCoinIcon));

            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToMenu(menuView, catalog, null, false);
            Assert.That(menuView.musicButtonImage.sprite, Is.SameAs(catalog.musicOnButton));
            Assert.That(menuView.musicButtonImage.color, Is.EqualTo(LegacyUiSpriteSceneApplier.MusicDisabledTint));
            Assert.That(menuView.creditsButton.GetComponent<Image>().sprite, Is.SameAs(creditsButtonSceneSprite));
            Assert.That(menuView.shopItemTemplate.GetComponent<Image>().sprite, Is.SameAs(catalog.menuShopItemBackground));

            var mapView = CreateMapView();
            var map1SceneSprite = CreateSprite(new Color(0.11f, 0.21f, 0.31f));
            var map2SceneSprite = CreateSprite(new Color(0.12f, 0.22f, 0.32f));
            var map3SceneSprite = CreateSprite(new Color(0.13f, 0.23f, 0.33f));
            var homeButtonSceneSprite = CreateSprite(new Color(0.14f, 0.24f, 0.34f));
            mapView.map1Image.sprite = map1SceneSprite;
            mapView.map2Image.sprite = map2SceneSprite;
            mapView.map3Image.sprite = map3SceneSprite;
            mapView.homeButton.GetComponent<Image>().sprite = homeButtonSceneSprite;
            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToMap(mapView, catalog);
            Assert.That(mapView.map1Image.sprite, Is.SameAs(catalog.map1Background));
            Assert.That(mapView.map2Image.sprite, Is.SameAs(catalog.map2Background));
            Assert.That(mapView.map3Image.sprite, Is.SameAs(catalog.map3Background));
            Assert.That(mapView.homeButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuButton));
            Assert.That(mapView.buttonsContainer.childCount, Is.EqualTo(0));

            Object.DestroyImmediate(bootView.gameObject);
            Object.DestroyImmediate(menuView.gameObject);
            Object.DestroyImmediate(mapView.gameObject);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void MenuPreview_LeavesExistingShopSpritesUntouchedWhenCatalogFieldsAreNull()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            var menuView = CreateMenuView();
            var shopOpenSceneSprite = CreateSprite(new Color(0.31f, 0.41f, 0.51f));
            var shopCloseSceneSprite = CreateSprite(new Color(0.32f, 0.42f, 0.52f));
            var shopItemBackgroundSceneSprite = CreateSprite(new Color(0.33f, 0.43f, 0.53f));
            var shopCoinSceneSprite = CreateSprite(new Color(0.34f, 0.44f, 0.54f));

            menuView.shopOpenButton.GetComponent<Image>().sprite = shopOpenSceneSprite;
            menuView.shopCloseButton.GetComponent<Image>().sprite = shopCloseSceneSprite;
            menuView.shopItemTemplate.GetComponent<Image>().sprite = shopItemBackgroundSceneSprite;
            menuView.shopItemCoinIcon.sprite = shopCoinSceneSprite;

            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToMenu(menuView, catalog, sprites);

            Assert.That(menuView.shopOpenButton.GetComponent<Image>().sprite, Is.SameAs(shopOpenSceneSprite));
            Assert.That(menuView.shopCloseButton.GetComponent<Image>().sprite, Is.SameAs(shopCloseSceneSprite));
            Assert.That(menuView.shopItemTemplate.GetComponent<Image>().sprite, Is.SameAs(shopItemBackgroundSceneSprite));
            Assert.That(menuView.shopItemCoinIcon.sprite, Is.SameAs(shopCoinSceneSprite));

            Object.DestroyImmediate(menuView.gameObject);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void RefreshBuildScenes_RefreshesAllEnabledBuildSettingsScenesAndRestoresSetup()
        {
            const string tempFolderPath = "Assets/Temp/JellyQuestSpriteConfigTests";
            const string baselineScenePath = tempFolderPath + "/BaselineScene.unity";
            const string bootScenePath = tempFolderPath + "/BootPreviewScene.unity";
            const string menuScenePath = tempFolderPath + "/MenuPreviewScene.unity";
            const string mapScenePath = tempFolderPath + "/MapPreviewScene.unity";

            var originalBuildScenes = EditorBuildSettings.scenes;
            var originalSetup = EditorSceneManager.GetSceneManagerSetup();
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();

            try
            {
                EnsureFolder(tempFolderPath);

                catalog.preloadBackground = LoadProjectSprite("Assets/Sprites/bg.png");
                catalog.logo = LoadProjectSprite("Assets/Sprites/logo.png");
                catalog.menuBackground = LoadProjectSprite("Assets/Sprites/bgMap2.png");
                catalog.startButton = LoadProjectSprite("Assets/Sprites/Use/sprite_22.png");
                catalog.musicOnButton = LoadProjectSprite("Assets/Sprites/banner 2.png");
                catalog.closeSmallButton = LoadProjectSprite("Assets/Sprites/Use/sprite_21.png");
                catalog.menuShopOpenButton = LoadProjectSprite("Assets/Sprites/Use/sprite_22.png");
                catalog.menuShopCloseButton = LoadProjectSprite("Assets/Sprites/Use/sprite_21.png");
                catalog.menuShopItemBackground = LoadProjectSprite("Assets/Sprites/Use/sprite_41.png");
                catalog.menuShopCoinIcon = LoadProjectSprite("Assets/Sprites/Use/sprite_24.png");
                catalog.map1Background = LoadProjectSprite("Assets/Sprites/Tile.png");
                catalog.map2Background = LoadProjectSprite("Assets/Sprites/bgMap2.png");
                catalog.map3Background = LoadProjectSprite("Assets/Sprites/icon 4.png");
                catalog.menuButton = LoadProjectSprite("Assets/Sprites/banner 2.png");

                CreateAndSaveBaselineScene(baselineScenePath);
                CreateAndSaveBootPreviewScene(bootScenePath);
                CreateAndSaveMenuPreviewScene(menuScenePath);
                CreateAndSaveMapPreviewScene(mapScenePath);

                var baselineScene = EditorSceneManager.OpenScene(baselineScenePath, OpenSceneMode.Single);
                var baselineSetup = EditorSceneManager.GetSceneManagerSetup();
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(bootScenePath, true),
                    new EditorBuildSettingsScene(menuScenePath, true),
                    new EditorBuildSettingsScene(mapScenePath, true)
                };

                Assert.That(JellyQuestSpriteScenePreview.RefreshBuildScenes(catalog), Is.True);
                Assert.That(EditorSceneManager.GetActiveScene().path, Is.EqualTo(baselineScene.path));
                Assert.That(FindSceneComponent<BootScreenView>(), Is.Null);

                var refreshedBootScene = EditorSceneManager.OpenScene(bootScenePath, OpenSceneMode.Single);
                var bootView = FindSceneComponent<BootScreenView>();
                Assert.That(refreshedBootScene.IsValid(), Is.True);
                Assert.That(bootView, Is.Not.Null);
                Assert.That(bootView.backgroundImage.sprite, Is.SameAs(catalog.preloadBackground));
                Assert.That(bootView.logoImage.sprite, Is.SameAs(catalog.logo));

                var refreshedMenuScene = EditorSceneManager.OpenScene(menuScenePath, OpenSceneMode.Single);
                var menuView = FindSceneComponent<MenuScreenView>();
                Assert.That(refreshedMenuScene.IsValid(), Is.True);
                Assert.That(menuView, Is.Not.Null);
                Assert.That(menuView.backgroundImage.sprite, Is.SameAs(catalog.menuBackground));
                Assert.That(menuView.logoImage.sprite, Is.SameAs(catalog.logo));
                Assert.That(menuView.startButton.GetComponent<Image>().sprite, Is.SameAs(catalog.startButton));
                Assert.That(menuView.musicButtonImage.sprite, Is.SameAs(catalog.musicOnButton));
                Assert.That(menuView.musicButtonImage.color, Is.EqualTo(Color.white));
                Assert.That(menuView.creditsCloseButton.GetComponent<Image>().sprite, Is.SameAs(catalog.closeSmallButton));
                Assert.That(menuView.shopOpenButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuShopOpenButton));
                Assert.That(menuView.shopCloseButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuShopCloseButton));
                Assert.That(menuView.shopItemTemplate.GetComponent<Image>().sprite, Is.SameAs(catalog.menuShopItemBackground));
                Assert.That(menuView.shopItemCoinIcon.sprite, Is.SameAs(catalog.menuShopCoinIcon));

                var refreshedMapScene = EditorSceneManager.OpenScene(mapScenePath, OpenSceneMode.Single);
                var mapView = FindSceneComponent<MapScreenView>();
                Assert.That(refreshedMapScene.IsValid(), Is.True);
                Assert.That(mapView, Is.Not.Null);
                Assert.That(mapView.map1Image.sprite, Is.SameAs(catalog.map1Background));
                Assert.That(mapView.map2Image.sprite, Is.SameAs(catalog.map2Background));
                Assert.That(mapView.map3Image.sprite, Is.SameAs(catalog.map3Background));
                Assert.That(mapView.homeButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuButton));
                Assert.That(mapView.buttonsContainer.childCount, Is.EqualTo(0));

                EditorSceneManager.RestoreSceneManagerSetup(baselineSetup);
            }
            finally
            {
                EditorBuildSettings.scenes = originalBuildScenes;
                EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                AssetDatabase.DeleteAsset(tempFolderPath);
                AssetDatabase.Refresh();
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void PreviewUtility_AppliesGameplaySceneOwnedSprites()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            catalog.gameplayMovesPanel = CreateSprite(Color.red);
            catalog.gameplayObjectivePanel = CreateSprite(Color.green);
            catalog.gameplayObjectiveStrip = CreateSprite(Color.blue);
            catalog.gameplayTimerBarBackground = CreateSprite(Color.yellow);
            catalog.gameplayTimerBarFill = CreateSprite(Color.magenta);
            catalog.gameplayTimerClock = CreateSprite(Color.cyan);
            catalog.musicOnButton = CreateSprite(new Color(0.2f, 0.3f, 0.4f));
            catalog.pauseButton = CreateSprite(new Color(0.3f, 0.4f, 0.5f));
            catalog.resumeButton = CreateSprite(new Color(0.4f, 0.5f, 0.6f));
            catalog.replayButton = CreateSprite(new Color(0.5f, 0.6f, 0.7f));
            catalog.menuButton = CreateSprite(new Color(0.6f, 0.7f, 0.8f));
            catalog.buyButton = CreateSprite(new Color(0.7f, 0.8f, 0.9f));
            catalog.closeSmallButton = CreateSprite(new Color(0.8f, 0.1f, 0.1f));
            catalog.gameplayPauseOverlayPanel = CreateSprite(new Color(0.1f, 0.8f, 0.1f));
            catalog.gameplayPauseOverlayTitle = CreateSprite(new Color(0.15f, 0.95f, 0.15f));
            catalog.gameplayShopOverlayPanel = CreateSprite(new Color(0.1f, 0.1f, 0.8f));
            catalog.gameplayShopOverlayTitle = CreateSprite(new Color(0.15f, 0.15f, 0.95f));
            catalog.gameplayWinOverlayPanel = CreateSprite(new Color(0.8f, 0.8f, 0.1f));
            catalog.gameplayWinOverlayTitle = CreateSprite(new Color(0.95f, 0.95f, 0.15f));
            catalog.gameplayLoseOverlayPanel = CreateSprite(new Color(0.8f, 0.1f, 0.8f));
            catalog.gameplayLoseOverlayTitle = CreateSprite(new Color(0.95f, 0.15f, 0.95f));
            catalog.gameplayTutorialOverlayPanel = CreateSprite(new Color(0.1f, 0.8f, 0.8f));
            catalog.gameplayWinLeftStar = CreateSprite(new Color(0.9f, 0.2f, 0.2f));
            catalog.gameplayWinCenterStar = CreateSprite(new Color(0.2f, 0.9f, 0.2f));
            catalog.gameplayWinRightStar = CreateSprite(new Color(0.2f, 0.2f, 0.9f));
            catalog.singleBoosterActive = CreateSprite(new Color(0.4f, 0.1f, 0.1f));
            catalog.columnBoosterActive = CreateSprite(new Color(0.1f, 0.4f, 0.1f));
            catalog.rowBoosterActive = CreateSprite(new Color(0.1f, 0.1f, 0.4f));
            catalog.crossBoosterActive = CreateSprite(new Color(0.4f, 0.4f, 0.1f));
            catalog.areaBoosterActive = CreateSprite(new Color(0.4f, 0.1f, 0.4f));
            catalog.objectiveSprites = new[]
            {
                CreateSprite(new Color(0.9f, 0.3f, 0.3f)),
                CreateSprite(new Color(0.3f, 0.9f, 0.3f)),
                CreateSprite(new Color(0.9f, 0.9f, 0.3f)),
                CreateSprite(new Color(0.9f, 0.3f, 0.9f)),
                CreateSprite(new Color(0.3f, 0.3f, 0.9f))
            };
            catalog.iceObjectiveSprite = CreateSprite(new Color(0.7f, 0.9f, 1f));

            var gameplayView = CreateGameplayView();
            var movesPanelSceneSprite = CreateSprite(new Color(0.12f, 0.12f, 0.12f));
            var objectivePanelSceneSprite = CreateSprite(new Color(0.22f, 0.22f, 0.22f));
            var objectiveStripSceneSprite = CreateSprite(new Color(0.32f, 0.32f, 0.32f));
            var timerBackgroundSceneSprite = CreateSprite(new Color(0.42f, 0.42f, 0.42f));
            var timerFillSceneSprite = CreateSprite(new Color(0.52f, 0.52f, 0.52f));
            var timerClockSceneSprite = CreateSprite(new Color(0.62f, 0.62f, 0.62f));
            var pauseButtonSceneSprite = CreateSprite(new Color(0.72f, 0.72f, 0.72f));
            var pauseResumeSceneSprite = CreateSprite(new Color(0.73f, 0.73f, 0.73f));
            var pauseReplaySceneSprite = CreateSprite(new Color(0.74f, 0.74f, 0.74f));
            var pauseMenuSceneSprite = CreateSprite(new Color(0.75f, 0.75f, 0.75f));
            var shopBuySceneSprite = CreateSprite(new Color(0.76f, 0.76f, 0.76f));
            var shopCloseSceneSprite = CreateSprite(new Color(0.77f, 0.77f, 0.77f));
            var winMapSceneSprite = CreateSprite(new Color(0.78f, 0.78f, 0.78f));
            var winReplaySceneSprite = CreateSprite(new Color(0.79f, 0.79f, 0.79f));
            var loseMapSceneSprite = CreateSprite(new Color(0.8f, 0.8f, 0.8f));
            var loseReplaySceneSprite = CreateSprite(new Color(0.81f, 0.81f, 0.81f));
            var winNextSceneSprite = CreateSprite(new Color(0.82f, 0.82f, 0.82f));
            var tutorialOkSceneSprite = CreateSprite(new Color(0.83f, 0.83f, 0.83f));
            var shopIconSceneSprite = CreateSprite(new Color(0.84f, 0.84f, 0.84f));
            var pauseOverlayPanelSceneSprite = CreateSprite(new Color(0.15f, 0.25f, 0.35f));
            var shopOverlayPanelSceneSprite = CreateSprite(new Color(0.16f, 0.26f, 0.36f));
            var winOverlayPanelSceneSprite = CreateSprite(new Color(0.17f, 0.27f, 0.37f));
            var loseOverlayPanelSceneSprite = CreateSprite(new Color(0.18f, 0.28f, 0.38f));
            var tutorialOverlayPanelSceneSprite = CreateSprite(new Color(0.19f, 0.29f, 0.39f));
            var pauseTitleSceneSprite = CreateSprite(new Color(0.2f, 0.3f, 0.4f));
            var shopTitleSceneSprite = CreateSprite(new Color(0.21f, 0.31f, 0.41f));
            var winTitleSceneSprite = CreateSprite(new Color(0.22f, 0.32f, 0.42f));
            var loseTitleSceneSprite = CreateSprite(new Color(0.23f, 0.33f, 0.43f));
            var winLeftStarSceneSprite = CreateSprite(new Color(0.24f, 0.34f, 0.44f));
            var winCenterStarSceneSprite = CreateSprite(new Color(0.25f, 0.35f, 0.45f));
            var winRightStarSceneSprite = CreateSprite(new Color(0.26f, 0.36f, 0.46f));

            gameplayView.legacyMovesPanelImage.sprite = movesPanelSceneSprite;
            gameplayView.legacyObjectivePanelImage.sprite = objectivePanelSceneSprite;
            gameplayView.legacyObjectiveStripImage.sprite = objectiveStripSceneSprite;
            gameplayView.legacyTimerBarBackgroundImage.sprite = timerBackgroundSceneSprite;
            gameplayView.legacyTimerBarFillImage.sprite = timerFillSceneSprite;
            gameplayView.legacyTimerClockImage.sprite = timerClockSceneSprite;
            gameplayView.pauseButton.GetComponent<Image>().sprite = pauseButtonSceneSprite;
            gameplayView.pauseResumeButton.GetComponent<Image>().sprite = pauseResumeSceneSprite;
            gameplayView.pauseReplayButton.GetComponent<Image>().sprite = pauseReplaySceneSprite;
            gameplayView.pauseMenuButton.GetComponent<Image>().sprite = pauseMenuSceneSprite;
            gameplayView.shopBuyButton.GetComponent<Image>().sprite = shopBuySceneSprite;
            gameplayView.shopCloseButton.GetComponent<Image>().sprite = shopCloseSceneSprite;
            gameplayView.winMapButton.GetComponent<Image>().sprite = winMapSceneSprite;
            gameplayView.winReplayButton.GetComponent<Image>().sprite = winReplaySceneSprite;
            gameplayView.loseMapButton.GetComponent<Image>().sprite = loseMapSceneSprite;
            gameplayView.loseReplayButton.GetComponent<Image>().sprite = loseReplaySceneSprite;
            gameplayView.winNextButton.GetComponent<Image>().sprite = winNextSceneSprite;
            gameplayView.tutorialOkButton.GetComponent<Image>().sprite = tutorialOkSceneSprite;
            gameplayView.shopIconImage.sprite = shopIconSceneSprite;
            JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.pauseOverlay).sprite = pauseOverlayPanelSceneSprite;
            JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.shopOverlay).sprite = shopOverlayPanelSceneSprite;
            JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.winOverlay).sprite = winOverlayPanelSceneSprite;
            JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.loseOverlay).sprite = loseOverlayPanelSceneSprite;
            JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.tutorialOverlay).sprite = tutorialOverlayPanelSceneSprite;
            var pauseTitleImage = CreateImage("Title", JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.pauseOverlay));
            pauseTitleImage.sprite = pauseTitleSceneSprite;
            var shopTitleImage = CreateImage("Title", JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.shopOverlay));
            shopTitleImage.sprite = shopTitleSceneSprite;
            var winTitleImage = CreateImage("Title", JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.winOverlay));
            winTitleImage.sprite = winTitleSceneSprite;
            var loseTitleImage = CreateImage("Title", JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.loseOverlay));
            loseTitleImage.sprite = loseTitleSceneSprite;
            gameplayView.winLeftStarImage.sprite = winLeftStarSceneSprite;
            gameplayView.winCenterStarImage.sprite = winCenterStarSceneSprite;
            gameplayView.winRightStarImage.sprite = winRightStarSceneSprite;
            var previewObjectiveRow = CreateObjectiveRow("ObjectivePreviewRow", gameplayView.objectiveContainer);
            var previewObjectiveSceneSprite = CreateSprite(new Color(0.27f, 0.37f, 0.47f));
            previewObjectiveRow.iconImage.sprite = previewObjectiveSceneSprite;

            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToGameplay(gameplayView, catalog);

            Assert.That(gameplayView.legacyMovesPanelImage.sprite, Is.SameAs(catalog.gameplayMovesPanel));
            Assert.That(gameplayView.legacyObjectivePanelImage.sprite, Is.SameAs(catalog.gameplayObjectivePanel));
            Assert.That(gameplayView.legacyObjectiveStripImage.sprite, Is.SameAs(catalog.gameplayObjectiveStrip));
            Assert.That(gameplayView.legacyTimerBarBackgroundImage.sprite, Is.SameAs(catalog.gameplayTimerBarBackground));
            Assert.That(gameplayView.legacyTimerBarFillImage.sprite, Is.SameAs(catalog.gameplayTimerBarFill));
            Assert.That(gameplayView.legacyTimerClockImage.sprite, Is.SameAs(catalog.gameplayTimerClock));
            Assert.That(gameplayView.musicButtonImage.sprite, Is.SameAs(catalog.musicOnButton));
            Assert.That(gameplayView.musicButtonImage.color, Is.EqualTo(Color.white));
            Assert.That(gameplayView.pauseButton.GetComponent<Image>().sprite, Is.SameAs(catalog.pauseButton));
            Assert.That(gameplayView.pauseResumeButton.GetComponent<Image>().sprite, Is.SameAs(catalog.resumeButton));
            Assert.That(gameplayView.pauseReplayButton.GetComponent<Image>().sprite, Is.SameAs(catalog.replayButton));
            Assert.That(gameplayView.pauseMenuButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuButton));
            Assert.That(gameplayView.shopBuyButton.GetComponent<Image>().sprite, Is.SameAs(catalog.buyButton));
            Assert.That(gameplayView.shopCloseButton.GetComponent<Image>().sprite, Is.SameAs(catalog.closeSmallButton));
            Assert.That(gameplayView.winMapButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuButton));
            Assert.That(gameplayView.winReplayButton.GetComponent<Image>().sprite, Is.SameAs(catalog.replayButton));
            Assert.That(gameplayView.loseMapButton.GetComponent<Image>().sprite, Is.SameAs(catalog.menuButton));
            Assert.That(gameplayView.loseReplayButton.GetComponent<Image>().sprite, Is.SameAs(catalog.replayButton));
            Assert.That(gameplayView.winNextButton.GetComponent<Image>().sprite, Is.SameAs(winNextSceneSprite));
            Assert.That(gameplayView.tutorialOkButton.GetComponent<Image>().sprite, Is.SameAs(tutorialOkSceneSprite));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.pauseOverlay).sprite, Is.SameAs(catalog.gameplayPauseOverlayPanel));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.pauseOverlay).sprite, Is.SameAs(catalog.gameplayPauseOverlayTitle));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.shopOverlay).sprite, Is.SameAs(catalog.gameplayShopOverlayPanel));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.shopOverlay).sprite, Is.SameAs(catalog.gameplayShopOverlayTitle));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.winOverlay).sprite, Is.SameAs(catalog.gameplayWinOverlayPanel));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.winOverlay).sprite, Is.SameAs(catalog.gameplayWinOverlayTitle));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.loseOverlay).sprite, Is.SameAs(catalog.gameplayLoseOverlayPanel));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.loseOverlay).sprite, Is.SameAs(catalog.gameplayLoseOverlayTitle));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.tutorialOverlay).sprite, Is.SameAs(catalog.gameplayTutorialOverlayPanel));
            Assert.That(gameplayView.winLeftStarImage.sprite, Is.SameAs(catalog.gameplayWinLeftStar));
            Assert.That(gameplayView.winCenterStarImage.sprite, Is.SameAs(catalog.gameplayWinCenterStar));
            Assert.That(gameplayView.winRightStarImage.sprite, Is.SameAs(catalog.gameplayWinRightStar));
            Assert.That(gameplayView.shopIconImage.sprite, Is.SameAs(catalog.singleBoosterActive));
            Assert.That(gameplayView.objectiveRowTemplate.iconImage.sprite, Is.SameAs(catalog.objectiveSprites[0]));
            Assert.That(previewObjectiveRow.iconImage.sprite, Is.SameAs(catalog.objectiveSprites[0]));
            Assert.That(gameplayView.boosterButtons[0].image.sprite, Is.SameAs(catalog.singleBoosterActive));
            Assert.That(gameplayView.boosterButtons[1].image.sprite, Is.SameAs(catalog.columnBoosterActive));
            Assert.That(gameplayView.boosterButtons[2].image.sprite, Is.SameAs(catalog.rowBoosterActive));
            Assert.That(gameplayView.boosterButtons[3].image.sprite, Is.SameAs(catalog.crossBoosterActive));
            Assert.That(gameplayView.boosterButtons[4].image.sprite, Is.SameAs(catalog.areaBoosterActive));
            Assert.That(gameplayView.GetComponentsInChildren<SpriteRenderer>(true), Is.Empty);

            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToGameplay(gameplayView, catalog, null, false);
            Assert.That(gameplayView.musicButtonImage.sprite, Is.SameAs(catalog.musicOnButton));
            Assert.That(gameplayView.musicButtonImage.color, Is.EqualTo(LegacyUiSpriteSceneApplier.MusicDisabledTint));
            Assert.That(gameplayView.legacyMovesPanelImage.sprite, Is.SameAs(catalog.gameplayMovesPanel));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.pauseOverlay).sprite, Is.SameAs(catalog.gameplayPauseOverlayTitle));

            Object.DestroyImmediate(gameplayView.gameObject);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayPreview_FallsBackForDynamicSpritesWhenCatalogFieldsAreNull()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            var gameplayView = CreateGameplayView();
            var pausePanelSceneSprite = CreateSprite(new Color(0.11f, 0.21f, 0.31f));
            var pauseTitleSceneSprite = CreateSprite(new Color(0.12f, 0.22f, 0.32f));
            var winCenterStarSceneSprite = CreateSprite(new Color(0.13f, 0.23f, 0.33f));
            JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.pauseOverlay).sprite = pausePanelSceneSprite;
            CreateImage("Title", JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.pauseOverlay)).sprite = pauseTitleSceneSprite;
            gameplayView.winCenterStarImage.sprite = winCenterStarSceneSprite;

            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToGameplay(gameplayView, catalog, sprites);

            Assert.That(gameplayView.legacyMovesPanelImage.sprite, Is.Not.Null);
            Assert.That(gameplayView.musicButtonImage.sprite, Is.Not.Null);
            Assert.That(gameplayView.musicButtonImage.color, Is.EqualTo(Color.white));
            Assert.That(gameplayView.pauseButton.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(gameplayView.pauseResumeButton.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(gameplayView.shopBuyButton.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(gameplayView.loseMapButton.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(gameplayView.shopIconImage.sprite, Is.Not.Null);
            Assert.That(gameplayView.objectiveRowTemplate.iconImage.sprite, Is.Not.Null);
            Assert.That(gameplayView.boosterButtons[0].image.sprite, Is.Not.Null);
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelImage(gameplayView.pauseOverlay).sprite, Is.Not.SameAs(pausePanelSceneSprite));
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.pauseOverlay).sprite, Is.SameAs(pauseTitleSceneSprite));
            Assert.That(gameplayView.winCenterStarImage.sprite, Is.Not.SameAs(winCenterStarSceneSprite));
            Assert.That(gameplayView.winCenterStarImage.sprite, Is.Not.Null);

            Object.DestroyImmediate(gameplayView.gameObject);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayPreview_LeavesExistingOverlayTitleUntouchedWhenLookupReturnsNull()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var gameplayView = CreateGameplayView();
            var pauseTitleImage = CreateImage("Title", JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.pauseOverlay));
            var sceneTitleSprite = CreateSprite(new Color(0.8f, 0.2f, 0.2f));
            pauseTitleImage.sprite = sceneTitleSprite;

            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToGameplay(gameplayView, catalog);

            Assert.That(pauseTitleImage, Is.Not.Null);
            Assert.That(pauseTitleImage.sprite, Is.SameAs(sceneTitleSprite));
            Assert.That(pauseTitleImage.gameObject.activeSelf, Is.True);

            Object.DestroyImmediate(gameplayView.gameObject);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayPreview_DoesNotCreateOverlayTitleImagesOnRepeatedApply()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            catalog.gameplayPauseOverlayTitle = CreateSprite(new Color(0.8f, 0.2f, 0.2f));
            catalog.gameplayShopOverlayTitle = CreateSprite(new Color(0.2f, 0.8f, 0.2f));
            catalog.gameplayWinOverlayTitle = CreateSprite(new Color(0.2f, 0.2f, 0.8f));
            catalog.gameplayLoseOverlayTitle = CreateSprite(new Color(0.8f, 0.2f, 0.8f));

            var gameplayView = CreateGameplayView();

            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToGameplay(gameplayView, catalog);
            JellyQuestUnity.Editor.JellyQuestSpriteScenePreview.ApplyToGameplay(gameplayView, catalog);

            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.pauseOverlay), Is.Null);
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.shopOverlay), Is.Null);
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.winOverlay), Is.Null);
            Assert.That(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayTitleImage(gameplayView.loseOverlay), Is.Null);
            Assert.That(CountChildrenNamed(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.pauseOverlay), "Title"), Is.EqualTo(0));
            Assert.That(CountChildrenNamed(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.shopOverlay), "Title"), Is.EqualTo(0));
            Assert.That(CountChildrenNamed(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.winOverlay), "Title"), Is.EqualTo(0));
            Assert.That(CountChildrenNamed(JellyQuestUnity.LegacyUiSpriteSceneApplier.FindOverlayPanelTransform(gameplayView.loseOverlay), "Title"), Is.EqualTo(0));

            Object.DestroyImmediate(gameplayView.gameObject);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayTinyStarLookup_UsesCatalogSpriteAndFallsBackToLegacyAtlas()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            var configuredStar = CreateSprite(new Color(1f, 0.9f, 0.2f));
            catalog.gameplayTinyStar = configuredStar;

            Assert.That(LegacyUiSpriteLookup.GetGameplayTinyStar(catalog, sprites), Is.SameAs(configuredStar));

            catalog.gameplayTinyStar = null;

            Assert.That(LegacyUiSpriteLookup.GetGameplayTinyStar(catalog, sprites), Is.SameAs(sprites.GetSheetSprite("star_play.png")));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void BoosterLookup_UsesActiveSpritesForUnlockedButtonsAndFallbacks()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            catalog.singleBoosterActive = CreateSprite(new Color(0.9f, 0.3f, 0.3f));
            catalog.columnBoosterActive = CreateSprite(new Color(0.3f, 0.9f, 0.3f));
            catalog.rowBoosterActive = CreateSprite(new Color(0.3f, 0.3f, 0.9f));
            catalog.crossBoosterActive = CreateSprite(new Color(0.9f, 0.9f, 0.3f));
            catalog.areaBoosterActive = CreateSprite(new Color(0.9f, 0.3f, 0.9f));
            catalog.singleBoosterLocked = CreateSprite(new Color(0.2f, 0.2f, 0.2f));

            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, null, BoosterType.Single, true), Is.SameAs(catalog.singleBoosterActive));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, null, BoosterType.Column, true), Is.SameAs(catalog.columnBoosterActive));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, null, BoosterType.Row, true), Is.SameAs(catalog.rowBoosterActive));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, null, BoosterType.Cross, true), Is.SameAs(catalog.crossBoosterActive));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, null, BoosterType.Area, true), Is.SameAs(catalog.areaBoosterActive));

            catalog.singleBoosterActive = null;
            catalog.columnBoosterActive = null;
            catalog.rowBoosterActive = null;
            catalog.crossBoosterActive = null;
            catalog.areaBoosterActive = null;

            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, sprites, BoosterType.Single, true), Is.SameAs(sprites.GetSheetSprite("power_up_32.png")));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, sprites, BoosterType.Column, true), Is.SameAs(sprites.GetSheetSprite("power_up_02.png")));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, sprites, BoosterType.Row, true), Is.SameAs(sprites.GetSheetSprite("power_up_12.png")));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, sprites, BoosterType.Cross, true), Is.SameAs(sprites.GetSheetSprite("power_up_22.png")));
            Assert.That(LegacyUiSpriteLookup.GetBoosterSprite(catalog, sprites, BoosterType.Area, true), Is.SameAs(sprites.GetSheetSprite("power_up_42.png")));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void LevelButtonLookup_UsesLockedAndUnlockedSlotsOnly()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var lockedSprite = CreateSprite(new Color(0.4f, 0.4f, 0.4f));
            var unlockedSprite = CreateSprite(new Color(0.2f, 0.9f, 1f));
            catalog.levelButtonSprites = new Sprite[25];
            catalog.levelButtonSprites[0] = lockedSprite;
            catalog.levelButtonSprites[1] = unlockedSprite;
            catalog.levelButtonSprites[6] = CreateSprite(new Color(0.1f, 0.9f, 0.1f));

            Assert.That(catalog.GetLevelButtonSprite(false), Is.SameAs(lockedSprite));
            Assert.That(catalog.GetLevelButtonSprite(true), Is.SameAs(unlockedSprite));
            Assert.That(LegacyUiSpriteLookup.GetLevelButtonSprite(catalog, null, false), Is.SameAs(lockedSprite));
            Assert.That(LegacyUiSpriteLookup.GetLevelButtonSprite(catalog, null, true), Is.SameAs(unlockedSprite));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void LevelButtonLookup_FallsBackToLegacyAtlasLockedAndUnlockedSprites()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            catalog.levelButtonSprites = new Sprite[25];

            Assert.That(LegacyUiSpriteLookup.GetLevelButtonSprite(catalog, sprites, false), Is.SameAs(sprites.GetLevelButtonSprite(0)));
            Assert.That(LegacyUiSpriteLookup.GetLevelButtonSprite(catalog, sprites, true), Is.SameAs(sprites.GetLevelButtonSprite(1)));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayWorldLookup_ReturnsCorrectBackgroundForLevelRanges()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            catalog.gameplayBackgroundLevels1To16 = CreateSprite(new Color(0.8f, 0.2f, 0.2f));
            catalog.gameplayBackgroundLevels17To32 = CreateSprite(new Color(0.2f, 0.8f, 0.2f));
            catalog.gameplayBackgroundLevels33Plus = CreateSprite(new Color(0.2f, 0.2f, 0.8f));

            Assert.That(LegacyUiSpriteLookup.GetGameplayBackground(catalog, null, 1), Is.SameAs(catalog.gameplayBackgroundLevels1To16));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBackground(catalog, null, 16), Is.SameAs(catalog.gameplayBackgroundLevels1To16));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBackground(catalog, null, 17), Is.SameAs(catalog.gameplayBackgroundLevels17To32));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBackground(catalog, null, 32), Is.SameAs(catalog.gameplayBackgroundLevels17To32));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBackground(catalog, null, 33), Is.SameAs(catalog.gameplayBackgroundLevels33Plus));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayWorldLookup_UsesCatalogSpritesAndFallsBackWhenMissing()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            catalog.gameplayBoardTile = CreateSprite(new Color(0.4f, 0.4f, 0.4f));
            catalog.gameplayBoardTiles = new[]
            {
                CreateSprite(new Color(0.9f, 0.7f, 0.1f)),
                CreateSprite(new Color(0.1f, 0.9f, 0.7f)),
                CreateSprite(new Color(0.7f, 0.1f, 0.9f))
            };
            catalog.gameplayIce = CreateSprite(new Color(0.6f, 0.9f, 1f));

            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 1), Is.SameAs(catalog.gameplayBoardTile));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 2), Is.SameAs(catalog.gameplayBoardTile));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 4), Is.SameAs(catalog.gameplayBoardTile));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 3), Is.SameAs(catalog.gameplayBoardTile));
            Assert.That(LegacyUiSpriteLookup.GetGameplayIce(catalog, sprites), Is.SameAs(catalog.gameplayIce));

            catalog.gameplayBoardTile = null;
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 1), Is.SameAs(catalog.gameplayBoardTiles[0]));

            catalog.gameplayBoardTiles = new[]
            {
                null,
                CreateSprite(new Color(0.2f, 0.2f, 0.6f)),
                CreateSprite(new Color(0.6f, 0.2f, 0.2f))
            };
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 2), Is.SameAs(catalog.gameplayBoardTiles[1]));

            catalog.gameplayBoardTiles = new Sprite[3];
            catalog.gameplayIce = null;

            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 1), Is.SameAs(sprites.GetBoardTileSprite(0)));
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoardTile(catalog, sprites, 2), Is.SameAs(sprites.GetBoardTileSprite(0)));
            Assert.That(LegacyUiSpriteLookup.GetGameplayIce(catalog, sprites), Is.Not.Null);

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayBoosterPreviewLookup_UsesCatalogSpriteAndFallsBackToBoardTile()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            var configuredPreviewSprite = CreateSprite(new Color(0.95f, 0.85f, 0.35f));
            var configuredBoardTile = CreateSprite(new Color(0.4f, 0.4f, 0.4f));

            catalog.gameplayBoosterPreviewSprite = configuredPreviewSprite;
            catalog.gameplayBoardTile = configuredBoardTile;

            Assert.That(LegacyUiSpriteLookup.GetGameplayBoosterPreviewSprite(catalog, sprites, 1), Is.SameAs(configuredPreviewSprite));

            catalog.gameplayBoosterPreviewSprite = null;
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoosterPreviewSprite(catalog, sprites, 2), Is.SameAs(configuredBoardTile));

            catalog.gameplayBoardTile = null;
            catalog.gameplayBoardTiles = new[]
            {
                CreateSprite(new Color(0.9f, 0.7f, 0.1f)),
                null,
                null
            };
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoosterPreviewSprite(catalog, sprites, 4), Is.SameAs(catalog.gameplayBoardTiles[0]));

            catalog.gameplayBoardTiles = new Sprite[3];
            Assert.That(LegacyUiSpriteLookup.GetGameplayBoosterPreviewSprite(catalog, sprites, 3), Is.SameAs(sprites.GetBoardTileSprite(0)));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayPieceLookup_UsesCatalogSpritesForRepresentativeFrames()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            catalog.gameplayPieceSprites = new[]
            {
                CreateSprite(new Color(0.9f, 0.2f, 0.2f)),
                CreateSprite(new Color(0.2f, 0.9f, 0.2f)),
                CreateSprite(new Color(0.9f, 0.9f, 0.2f)),
                CreateSprite(new Color(0.9f, 0.2f, 0.9f)),
                CreateSprite(new Color(0.2f, 0.6f, 0.9f))
            };
            catalog.gameplaySimpleLockedPieceSprites = new[]
            {
                CreateSprite(new Color(0.6f, 0.2f, 0.2f)),
                CreateSprite(new Color(0.2f, 0.6f, 0.2f)),
                CreateSprite(new Color(0.6f, 0.6f, 0.2f)),
                CreateSprite(new Color(0.6f, 0.2f, 0.6f)),
                CreateSprite(new Color(0.2f, 0.4f, 0.6f))
            };
            catalog.gameplayBlockerSprites = new[]
            {
                CreateSprite(new Color(0.2f, 0.2f, 0.2f)),
                CreateSprite(new Color(0.4f, 0.4f, 0.4f)),
                CreateSprite(new Color(0.6f, 0.6f, 0.6f))
            };
            catalog.gameplayLayeredLockedPieceSprites = new Sprite[15];
            catalog.gameplayLayeredLockedPieceSprites[5] = CreateSprite(new Color(0.3f, 0.7f, 0.5f));
            catalog.gameplayLayeredLockedPieceSprites[14] = CreateSprite(new Color(0.3f, 0.5f, 0.7f));

            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, null, 0), Is.SameAs(catalog.gameplayPieceSprites[0]));
            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, null, 7), Is.SameAs(catalog.gameplaySimpleLockedPieceSprites[2]));
            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, null, 10), Is.SameAs(catalog.gameplayBlockerSprites[0]));
            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, null, 18), Is.SameAs(catalog.gameplayLayeredLockedPieceSprites[5]));
            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, null, 27), Is.SameAs(catalog.gameplayLayeredLockedPieceSprites[14]));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayPieceLockBarLookup_UsesCatalogSpriteAndFallsBackToResource()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            var configuredLockBar = CreateSprite(new Color(0.7f, 0.7f, 0.75f));
            catalog.gameplayPieceLockBar = configuredLockBar;

            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceLockBar(catalog, sprites), Is.SameAs(configuredLockBar));

            catalog.gameplayPieceLockBar = null;

            Assert.That(
                LegacyUiSpriteLookup.GetGameplayPieceLockBar(catalog, sprites),
                Is.SameAs(sprites.GetFullTextureSprite("JellyQuest/Raw/assets/piece_lock_bar")));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GameplayPieceLookup_FallsBackToLegacyAtlasAndDoesNotShareObjectiveSprites()
        {
            var catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            var sprites = new LegacySpriteLibrary();
            catalog.gameplayPieceSprites = new Sprite[5];
            catalog.gameplaySimpleLockedPieceSprites = new Sprite[5];
            catalog.gameplayBlockerSprites = new Sprite[3];
            catalog.gameplayLayeredLockedPieceSprites = new Sprite[15];
            catalog.objectiveSprites = new[]
            {
                CreateSprite(Color.red),
                CreateSprite(Color.green),
                CreateSprite(Color.yellow),
                CreateSprite(Color.magenta),
                CreateSprite(Color.blue)
            };

            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, sprites, 0), Is.SameAs(sprites.GetObjectSprite(0)));
            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, sprites, 7), Is.SameAs(sprites.GetObjectSprite(7)));
            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, sprites, 10), Is.SameAs(sprites.GetObjectSprite(10)));
            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, sprites, 18), Is.SameAs(sprites.GetObjectSprite(18)));

            catalog.gameplayPieceSprites[0] = CreateSprite(Color.cyan);

            Assert.That(LegacyUiSpriteLookup.GetGameplayPieceSprite(catalog, null, 0), Is.SameAs(catalog.gameplayPieceSprites[0]));
            Assert.That(LegacyUiSpriteLookup.GetObjectiveSprite(catalog, null, ObjectiveType.Red), Is.SameAs(catalog.objectiveSprites[0]));

            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void LevelDefinition_GetBackgroundMaskValue_UsesBoardCoordinates()
        {
            var definition = new LevelDefinition
            {
                boardWidth = 2,
                boardHeight = 2,
                backgroundWidth = 4,
                backgroundHeight = 4,
                backgroundMask = new[]
                {
                    0, 0, 0, 0,
                    0, 1, 2, 0,
                    0, 3, 4, 0,
                    0, 0, 0, 0
                }
            };

            Assert.That(definition.GetBackgroundMaskValue(0, 0), Is.EqualTo(1));
            Assert.That(definition.GetBackgroundMaskValue(0, 1), Is.EqualTo(2));
            Assert.That(definition.GetBackgroundMaskValue(1, 0), Is.EqualTo(3));
            Assert.That(definition.GetBackgroundMaskValue(1, 1), Is.EqualTo(4));
            Assert.That(definition.HasIce(1, 0), Is.True);
            Assert.That(definition.HasIce(0, 1), Is.False);
        }

        private static BootScreenView CreateBootView()
        {
            var root = new GameObject("BootView");
            var view = root.AddComponent<BootScreenView>();
            view.backgroundImage = CreateImage("Background", root.transform);
            view.logoImage = CreateImage("Logo", root.transform);
            view.loadingText = CreateText("Loading", root.transform);
            return view;
        }

        private static MenuScreenView CreateMenuView()
        {
            var root = new GameObject("MenuView");
            var view = root.AddComponent<MenuScreenView>();
            view.backgroundImage = CreateImage("Background", root.transform);
            view.logoImage = CreateImage("Logo", root.transform);
            view.startButton = CreateButton("StartButton", root.transform, out _);
            view.musicButton = CreateButton("MusicButton", root.transform, out var musicImage);
            view.musicButtonImage = musicImage;
            view.creditsButton = CreateButton("CreditsButton", root.transform, out _);
            view.creditsOverlay = new GameObject("CreditsOverlay");
            view.creditsOverlay.transform.SetParent(root.transform, false);
            view.creditsTitleText = CreateText("CreditsTitle", view.creditsOverlay.transform);
            view.creditsBodyText = CreateText("CreditsBody", view.creditsOverlay.transform);
            view.creditsCloseButton = CreateButton("CreditsCloseButton", view.creditsOverlay.transform, out _);
            view.shopOpenButton = CreateButton("ShopOpenButton", root.transform, out _);
            view.shopCloseButton = CreateButton("ShopCloseButton", root.transform, out _);
            view.shopItemTemplate = CreateShopItemTemplate(root.transform, out var shopCoinIcon);
            view.shopItemCoinIcon = shopCoinIcon;
            return view;
        }

        private static MapScreenView CreateMapView()
        {
            var root = new GameObject("MapView");
            var view = root.AddComponent<MapScreenView>();
            view.headerText = CreateText("Header", root.transform);
            view.homeButton = CreateButton("HomeButton", root.transform, out _);
            view.scrollRect = root.AddComponent<ScrollRect>();
            view.viewport = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
            view.viewport.SetParent(root.transform, false);
            view.content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            view.content.SetParent(root.transform, false);
            view.map3Image = CreateImage("Map3", root.transform);
            view.map2Image = CreateImage("Map2", root.transform);
            view.map1Image = CreateImage("Map1", root.transform);
            view.buttonsContainer = new GameObject("ButtonsContainer", typeof(RectTransform)).GetComponent<RectTransform>();
            view.buttonsContainer.SetParent(root.transform, false);
            return view;
        }

        private static GameplayScreenView CreateGameplayView()
        {
            var root = new GameObject("GameplayView");
            var view = root.AddComponent<GameplayScreenView>();
            view.movesText = CreateText("MovesText", root.transform);
            view.scoreText = CreateText("ScoreText", root.transform);
            view.timerText = CreateText("TimerText", root.transform);
            view.coinsText = CreateText("CoinsText", root.transform);
            view.levelText = CreateText("LevelText", root.transform);
            view.legacyMovesPanelImage = CreateImage("MovesPanel", root.transform);
            view.legacyObjectivePanelImage = CreateImage("ObjectivePanel", root.transform);
            view.legacyObjectiveStripImage = CreateImage("ObjectiveStrip", root.transform);
            view.legacyTimerBarBackgroundImage = CreateImage("TimerBarBackground", root.transform);
            view.legacyTimerBarFillImage = CreateImage("TimerBarFill", view.legacyTimerBarBackgroundImage.transform);
            view.legacyTimerBarFillRect = view.legacyTimerBarFillImage.rectTransform;
            view.legacyTimerClockImage = CreateImage("TimerClock", root.transform);
            view.musicButton = CreateButton("MusicButton", root.transform, out var musicImage);
            view.musicButtonImage = musicImage;
            view.pauseButton = CreateButton("PauseButton", root.transform, out _);
            view.objectiveContainer = new GameObject("ObjectiveContainer", typeof(RectTransform)).GetComponent<RectTransform>();
            view.objectiveContainer.SetParent(root.transform, false);
            view.objectiveRowTemplate = CreateObjectiveRow("ObjectiveTemplate", view.objectiveContainer);
            view.boosterButtons = new[]
            {
                CreateBoosterButton("SingleBooster", root.transform, BoosterType.Single),
                CreateBoosterButton("ColumnBooster", root.transform, BoosterType.Column),
                CreateBoosterButton("RowBooster", root.transform, BoosterType.Row),
                CreateBoosterButton("CrossBooster", root.transform, BoosterType.Cross),
                CreateBoosterButton("AreaBooster", root.transform, BoosterType.Area)
            };
            view.pauseOverlay = CreateOverlay("PauseOverlay", root.transform);
            view.pauseResumeButton = CreateButton("PauseResumeButton", root.transform, out _);
            view.pauseReplayButton = CreateButton("PauseReplayButton", root.transform, out _);
            view.pauseMenuButton = CreateButton("PauseMenuButton", root.transform, out _);
            view.shopOverlay = CreateOverlay("ShopOverlay", root.transform);
            view.shopPriceText = CreateText("ShopPrice", root.transform);
            view.shopCoinsText = CreateText("ShopCoins", root.transform);
            view.shopOwnedText = CreateText("ShopOwned", root.transform);
            view.shopIconImage = CreateImage("ShopIcon", root.transform);
            view.shopBuyButton = CreateButton("ShopBuyButton", root.transform, out _);
            view.shopCloseButton = CreateButton("ShopCloseButton", root.transform, out _);
            view.winOverlay = CreateOverlay("WinOverlay", root.transform);
            view.winScoreText = CreateText("WinScore", root.transform);
            view.winStarsText = CreateText("WinStars", root.transform);
            view.winStarsContainer = new GameObject("WinStarsContainer", typeof(RectTransform)).GetComponent<RectTransform>();
            view.winStarsContainer.SetParent(root.transform, false);
            view.winLeftStarImage = CreateImage("WinLeftStar", view.winStarsContainer);
            view.winCenterStarImage = CreateImage("WinCenterStar", view.winStarsContainer);
            view.winRightStarImage = CreateImage("WinRightStar", view.winStarsContainer);
            view.winNextButton = CreateButton("WinNextButton", root.transform, out _);
            view.winMapButton = CreateButton("WinMapButton", root.transform, out _);
            view.winReplayButton = CreateButton("WinReplayButton", root.transform, out _);
            view.loseOverlay = CreateOverlay("LoseOverlay", root.transform);
            view.loseInfoText = CreateText("LoseInfo", root.transform);
            view.loseReplayButton = CreateButton("LoseReplayButton", root.transform, out _);
            view.loseMapButton = CreateButton("LoseMapButton", root.transform, out _);
            view.tutorialOverlay = CreateOverlay("TutorialOverlay", root.transform);
            view.tutorialText = CreateText("TutorialText", root.transform);
            view.tutorialOkButton = CreateButton("TutorialOkButton", root.transform, out _);
            view.toastCanvasGroup = root.AddComponent<CanvasGroup>();
            view.toastText = CreateText("ToastText", root.transform);
            return view;
        }

        private static BoosterButtonView CreateBoosterButton(string name, Transform parent, BoosterType type)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var view = root.AddComponent<BoosterButtonView>();
            view.boosterType = type;
            view.button = CreateButton("Button", root.transform, out var image);
            view.image = image;
            view.countText = CreateText("Count", root.transform);
            return view;
        }

        private static ObjectiveRowView CreateObjectiveRow(string name, Transform parent)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var view = root.AddComponent<ObjectiveRowView>();
            view.rectTransform = root.GetComponent<RectTransform>();
            view.iconImage = CreateImage("Icon", root.transform);
            view.progressText = CreateText("Progress", root.transform);
            return view;
        }

        private static GameObject CreateOverlay(string name, Transform parent)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            return root;
        }

        private static Image CreateImage(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            return go.GetComponent<Image>();
        }

        private static Button CreateButton(string name, Transform parent, out Image image)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            image = go.GetComponent<Image>();
            return go.GetComponent<Button>();
        }

        private static IAPButtonView CreateShopItemTemplate(Transform parent, out Image coinIcon)
        {
            var button = CreateButton("IAPButtonView", parent, out _);
            var template = button.gameObject.AddComponent<IAPButtonView>();
            coinIcon = CreateImage("CoinIcon", button.transform);
            return template;
        }

        private static Text CreateText(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return text;
        }

        private static Sprite CreateSprite(Color color)
        {
            return CreateSprite(2, 2, color);
        }

        private static Sprite CreateSprite(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateSpriteFromRect(int textureWidth, int textureHeight, Rect spriteRect, Color color)
        {
            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            var pixels = new Color[textureWidth * textureHeight];
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, spriteRect, new Vector2(0.5f, 0.5f), 100f);
        }

        private static void CreateAndSaveBaselineScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("BaselineMarker");
            Assert.That(EditorSceneManager.SaveScene(scene, scenePath), Is.True);
        }

        private static void CreateAndSaveBootPreviewScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootView = CreateBootView();
            bootView.backgroundImage.sprite = LoadProjectSprite("Assets/Sprites/Rectangle 11 (2).png");
            bootView.logoImage.sprite = LoadProjectSprite("Assets/Sprites/Use/play.png");
            Assert.That(EditorSceneManager.SaveScene(scene, scenePath), Is.True);
        }

        private static void CreateAndSaveMenuPreviewScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var menuView = CreateMenuView();
            menuView.backgroundImage.sprite = LoadProjectSprite("Assets/Sprites/Tile.png");
            menuView.logoImage.sprite = LoadProjectSprite("Assets/Sprites/icon 4.png");
            menuView.startButton.GetComponent<Image>().sprite = LoadProjectSprite("Assets/Sprites/banner 2.png");
            menuView.musicButtonImage.sprite = LoadProjectSprite("Assets/Sprites/Use/sprite_24.png");
            menuView.creditsCloseButton.GetComponent<Image>().sprite = LoadProjectSprite("Assets/Sprites/Use/sprite_22.png");
            menuView.shopOpenButton.GetComponent<Image>().sprite = LoadProjectSprite("Assets/Sprites/Use/sprite_21.png");
            menuView.shopCloseButton.GetComponent<Image>().sprite = LoadProjectSprite("Assets/Sprites/banner 2.png");
            menuView.shopItemTemplate.GetComponent<Image>().sprite = LoadProjectSprite("Assets/Sprites/bg.png");
            menuView.shopItemCoinIcon.sprite = LoadProjectSprite("Assets/Sprites/logo.png");
            Assert.That(EditorSceneManager.SaveScene(scene, scenePath), Is.True);
        }

        private static void CreateAndSaveMapPreviewScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var mapView = CreateMapView();
            mapView.map1Image.sprite = LoadProjectSprite("Assets/Sprites/Use/sprite_01.png");
            mapView.map2Image.sprite = LoadProjectSprite("Assets/Sprites/Use/sprite_02.png");
            mapView.map3Image.sprite = LoadProjectSprite("Assets/Sprites/Use/sprite_03.png");
            mapView.homeButton.GetComponent<Image>().sprite = LoadProjectSprite("Assets/Sprites/Use/sprite_04.png");
            Assert.That(EditorSceneManager.SaveScene(scene, scenePath), Is.True);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var segments = folderPath.Split('/');
            var currentPath = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var nextPath = currentPath + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[index]);
                }

                currentPath = nextPath;
            }
        }

        private static Sprite LoadProjectSprite(string assetPath)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            Assert.That(sprite, Is.Not.Null, "Unable to load sprite asset: " + assetPath);
            return sprite;
        }

        private static T FindSceneComponent<T>() where T : Component
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var index = 0; index < roots.Length; index++)
            {
                var component = roots[index].GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void AssertFieldAppearsInTabs(string propertyName, params string[] expectedTabNames)
        {
            Assert.That(GetFieldOccurrenceTabNames(propertyName), Is.EqualTo(expectedTabNames));
        }

        private static string[] GetTabNames(JellyQuestSpriteConfigLayout.TabDefinition[] tabs)
        {
            var tabNames = new string[tabs.Length];
            for (var index = 0; index < tabs.Length; index++)
            {
                tabNames[index] = tabs[index].Name;
            }

            return tabNames;
        }

        private static string[] GetFieldPropertyNames(JellyQuestSpriteConfigLayout.SpriteFieldDefinition[] fields)
        {
            var propertyNames = new string[fields.Length];
            for (var index = 0; index < fields.Length; index++)
            {
                propertyNames[index] = fields[index].PropertyName;
            }

            return propertyNames;
        }

        private static string[] GetArrayPropertyNames(JellyQuestSpriteConfigLayout.SpriteArrayDefinition[] arrays)
        {
            var propertyNames = new string[arrays.Length];
            for (var index = 0; index < arrays.Length; index++)
            {
                propertyNames[index] = arrays[index].PropertyName;
            }

            return propertyNames;
        }

        private static int GetFieldIndex(string[] propertyNames, string propertyName)
        {
            for (var index = 0; index < propertyNames.Length; index++)
            {
                if (propertyNames[index] == propertyName)
                {
                    return index;
                }
            }

            Assert.Fail("Unable to find field index: " + propertyName);
            return -1;
        }

        private static int CountChildrenNamed(Transform parent, string childName)
        {
            if (parent == null)
            {
                return 0;
            }

            var childCount = 0;
            for (var index = 0; index < parent.childCount; index++)
            {
                if (parent.GetChild(index).name == childName)
                {
                    childCount += 1;
                }
            }

            return childCount;
        }

        private static string[] GetFieldOccurrenceTabNames(string propertyName)
        {
            var tabNames = new List<string>();
            var tabs = JellyQuestSpriteConfigLayout.VisibleTabs;
            for (var tabIndex = 0; tabIndex < tabs.Length; tabIndex++)
            {
                if (ContainsField(tabs[tabIndex].Fields, propertyName))
                {
                    tabNames.Add(tabs[tabIndex].Name);
                }
            }

            return tabNames.ToArray();
        }

        private static JellyQuestSpriteConfigLayout.TabDefinition FindTab(JellyQuestSpriteConfigLayout.TabDefinition[] tabs, string tabName)
        {
            for (var index = 0; index < tabs.Length; index++)
            {
                if (tabs[index].Name == tabName)
                {
                    return tabs[index];
                }
            }

            Assert.Fail("Unable to find tab: " + tabName);
            return default;
        }

        private static JellyQuestSpriteConfigLayout.SpriteFieldDefinition GetField(JellyQuestSpriteConfigLayout.SpriteFieldDefinition[] fields, string propertyName)
        {
            for (var index = 0; index < fields.Length; index++)
            {
                if (fields[index].PropertyName == propertyName)
                {
                    return fields[index];
                }
            }

            Assert.Fail("Unable to find field: " + propertyName);
            return default;
        }

        private static JellyQuestSpriteConfigLayout.SpriteArrayDefinition GetArray(JellyQuestSpriteConfigLayout.SpriteArrayDefinition[] arrays, string propertyName)
        {
            for (var index = 0; index < arrays.Length; index++)
            {
                if (arrays[index].PropertyName == propertyName)
                {
                    return arrays[index];
                }
            }

            Assert.Fail("Unable to find array: " + propertyName);
            return default;
        }

        private static bool ContainsField(JellyQuestSpriteConfigLayout.TabDefinition[] tabs, string propertyName)
        {
            for (var index = 0; index < tabs.Length; index++)
            {
                if (ContainsField(tabs[index].Fields, propertyName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsField(JellyQuestSpriteConfigLayout.SpriteFieldDefinition[] fields, string propertyName)
        {
            for (var index = 0; index < fields.Length; index++)
            {
                if (fields[index].PropertyName == propertyName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
