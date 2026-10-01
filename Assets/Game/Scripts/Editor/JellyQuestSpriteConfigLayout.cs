using System.Collections.Generic;

namespace JellyQuestUnity.Editor
{
    internal enum SpriteConfigUsage
    {
        Runtime,
        LegacyFallback
    }

    internal static class JellyQuestSpriteConfigLayout
    {
        private static readonly string[] ObjectiveLabels =
        {
            "Objective Red",
            "Objective Green",
            "Objective Yellow",
            "Objective Pink",
            "Objective Blue"
        };

        private static readonly string[] GameplayBoardTileLabels =
        {
            "Board Tile 1 (Mask 1/3)",
            "Board Tile 2 (Mask 2)",
            "Board Tile 3 (Mask 4)"
        };

        private static readonly string[] GameplayPieceLabels =
        {
            "Red Piece",
            "Green Piece",
            "Yellow Piece",
            "Pink Piece",
            "Blue Piece"
        };

        private static readonly string[] GameplaySimpleLockedPieceLabels =
        {
            "Red Simple Lock",
            "Green Simple Lock",
            "Yellow Simple Lock",
            "Pink Simple Lock",
            "Blue Simple Lock"
        };

        private static readonly string[] GameplayBlockerLabels =
        {
            "Blocker 1",
            "Blocker 2",
            "Blocker 3"
        };

        private static readonly string[] GameplayLayeredLockedPieceLabels =
        {
            "Red Layered Lock 1",
            "Red Layered Lock 2",
            "Red Layered Lock 3",
            "Green Layered Lock 1",
            "Green Layered Lock 2",
            "Green Layered Lock 3",
            "Yellow Layered Lock 1",
            "Yellow Layered Lock 2",
            "Yellow Layered Lock 3",
            "Pink Layered Lock 1",
            "Pink Layered Lock 2",
            "Pink Layered Lock 3",
            "Blue Layered Lock 1",
            "Blue Layered Lock 2",
            "Blue Layered Lock 3"
        };

        private static readonly int[] MapLevelButtonVisibleIndices =
        {
            0, 1
        };

        private static readonly string[] MapLevelButtonLabels =
        {
            "Locked",
            "Unlocked"
        };

        private static readonly TabDefinition[] AllTabDefinitions =
        {
            new TabDefinition(
                "Boot",
                new[]
                {
                    new SpriteFieldDefinition("preloadBackground", "Preload Background"),
                    new SpriteFieldDefinition("logo", "Logo [Shared]")
                }),
            new TabDefinition(
                "Menu",
                new[]
                {
                    new SpriteFieldDefinition("menuBackground", "Menu Background"),
                    new SpriteFieldDefinition("logo", "Logo [Shared]"),
                    new SpriteFieldDefinition("startButton", "Start Button"),
                    new SpriteFieldDefinition("musicOnButton", "Music On [Shared]"),
                    new SpriteFieldDefinition("closeSmallButton", "Close Small [Shared]"),
                    new SpriteFieldDefinition("menuShopOpenButton", "Menu Shop Open Button"),
                    new SpriteFieldDefinition("menuShopCloseButton", "Menu Shop Close Button"),
                    new SpriteFieldDefinition("menuShopItemBackground", "Menu Shop Item Background"),
                    new SpriteFieldDefinition("menuShopCoinIcon", "Menu Shop Coin Icon")
                }),
            new TabDefinition(
                "Map",
                new[]
                {
                    new SpriteFieldDefinition("map1Background", "Map 1 Background"),
                    new SpriteFieldDefinition("map2Background", "Map 2 Background"),
                    new SpriteFieldDefinition("map3Background", "Map 3 Background"),
                    new SpriteFieldDefinition("menuButton", "Home/Menu Button [Shared]")
                },
                new[]
                {
                    new SpriteArrayDefinition("levelButtonSprites", "Level Buttons", MapLevelButtonLabels, MapLevelButtonVisibleIndices)
                }),
            new TabDefinition(
                "Gameplay",
                new[]
                {
                    new SpriteFieldDefinition("gameplayMovesPanel", "Moves Panel"),
                    new SpriteFieldDefinition("gameplayObjectivePanel", "Objective Panel"),
                    new SpriteFieldDefinition("gameplayObjectiveStrip", "Objective Strip"),
                    new SpriteFieldDefinition("gameplayTimerBarBackground", "Timer Bar Background"),
                    new SpriteFieldDefinition("gameplayTimerBarFill", "Timer Bar Fill"),
                    new SpriteFieldDefinition("gameplayTimerClock", "Timer Clock"),
                    new SpriteFieldDefinition("gameplayTinyStar", "Tiny Star"),
                    new SpriteFieldDefinition("musicOnButton", "Music On [Shared]"),
                    new SpriteFieldDefinition("pauseButton", "Pause Button"),
                    new SpriteFieldDefinition("resumeButton", "Resume Button"),
                    new SpriteFieldDefinition("replayButton", "Replay Button"),
                    new SpriteFieldDefinition("menuButton", "Menu Button [Shared]"),
                    new SpriteFieldDefinition("buyButton", "Buy Button"),
                    new SpriteFieldDefinition("closeSmallButton", "Close Small [Shared]"),
                    new SpriteFieldDefinition("gameplayPauseOverlayPanel", "Pause Overlay Panel"),
                    new SpriteFieldDefinition("gameplayPauseOverlayTitle", "Pause Overlay Title"),
                    new SpriteFieldDefinition("gameplayShopOverlayPanel", "Shop Overlay Panel"),
                    new SpriteFieldDefinition("gameplayShopOverlayTitle", "Shop Overlay Title"),
                    new SpriteFieldDefinition("gameplayWinOverlayPanel", "Win Overlay Panel"),
                    new SpriteFieldDefinition("gameplayWinOverlayTitle", "Win Overlay Title"),
                    new SpriteFieldDefinition("gameplayLoseOverlayPanel", "Lose Overlay Panel"),
                    new SpriteFieldDefinition("gameplayLoseOverlayTitle", "Lose Overlay Title"),
                    new SpriteFieldDefinition("gameplayTutorialOverlayPanel", "Tutorial Overlay Panel"),
                    new SpriteFieldDefinition("gameplayWinLeftStar", "Win Left Star"),
                    new SpriteFieldDefinition("gameplayWinCenterStar", "Win Center Star"),
                    new SpriteFieldDefinition("gameplayWinRightStar", "Win Right Star"),
                    new SpriteFieldDefinition("singleBoosterActive", "Booster Single Active"),
                    new SpriteFieldDefinition("columnBoosterActive", "Booster Column Active"),
                    new SpriteFieldDefinition("rowBoosterActive", "Booster Row Active"),
                    new SpriteFieldDefinition("crossBoosterActive", "Booster Cross Active"),
                    new SpriteFieldDefinition("areaBoosterActive", "Booster Area Active"),
                    new SpriteFieldDefinition("gameplayBoosterPreviewSprite", "Booster Preview Sprite"),
                    new SpriteFieldDefinition("gameplayBoosterPreviewHintColor", "Booster Hint Color"),
                    new SpriteFieldDefinition("gameplayBoosterPreviewTargetColor", "Booster Target Color"),
                    new SpriteFieldDefinition("gameplayBoosterActiveButtonColor", "Booster Active Color"),
                    new SpriteFieldDefinition("iceObjectiveSprite", "Objective Ice")
                },
                new[]
                {
                    new SpriteArrayDefinition("objectiveSprites", "Objective Sprites", ObjectiveLabels)
                }),
            new TabDefinition(
                "Gameplay World",
                new[]
                {
                    new SpriteFieldDefinition("gameplayBackgroundLevels1To16", "Background Levels 1-16"),
                    new SpriteFieldDefinition("gameplayBackgroundLevels17To32", "Background Levels 17-32"),
                    new SpriteFieldDefinition("gameplayBackgroundLevels33Plus", "Background Levels 33+"),
                    new SpriteFieldDefinition("gameplayIce", "Ice"),
                    new SpriteFieldDefinition("gameplayBoardTile", "Board Tile")
                },
                new[]
                {
                    new SpriteArrayDefinition("gameplayBoardTiles", "Board Tiles [Legacy]", GameplayBoardTileLabels, usage: SpriteConfigUsage.LegacyFallback)
                }),
            new TabDefinition(
                "Gameplay Pieces",
                new[]
                {
                    new SpriteFieldDefinition("gameplayPieceLockBar", "Piece Lock Bar")
                },
                new[]
                {
                    new SpriteArrayDefinition("gameplayPieceSprites", "Pieces", GameplayPieceLabels),
                    new SpriteArrayDefinition("gameplaySimpleLockedPieceSprites", "Simple Locked Pieces", GameplaySimpleLockedPieceLabels, usage: SpriteConfigUsage.LegacyFallback),
                    new SpriteArrayDefinition("gameplayBlockerSprites", "Blockers", GameplayBlockerLabels),
                    new SpriteArrayDefinition("gameplayLayeredLockedPieceSprites", "Layered Locked Pieces", GameplayLayeredLockedPieceLabels, usage: SpriteConfigUsage.LegacyFallback)
                })
        };

        private static readonly TabDefinition[] VisibleTabDefinitions = BuildVisibleTabs();

        internal static TabDefinition[] AllTabs => AllTabDefinitions;
        internal static TabDefinition[] VisibleTabs => VisibleTabDefinitions;

        private static TabDefinition[] BuildVisibleTabs()
        {
            var visibleTabs = new List<TabDefinition>(AllTabDefinitions.Length);
            for (var index = 0; index < AllTabDefinitions.Length; index++)
            {
                var tab = AllTabDefinitions[index];
                var visibleFields = FilterFields(tab.Fields);
                var visibleArrays = FilterArrays(tab.ArrayGroups);
                if (visibleFields.Length == 0 && visibleArrays.Length == 0)
                {
                    continue;
                }

                visibleTabs.Add(new TabDefinition(tab.Name, visibleFields, visibleArrays));
            }

            return visibleTabs.ToArray();
        }

        private static SpriteFieldDefinition[] FilterFields(SpriteFieldDefinition[] fields)
        {
            var visibleFields = new List<SpriteFieldDefinition>(fields.Length);
            for (var index = 0; index < fields.Length; index++)
            {
                if (fields[index].Usage == SpriteConfigUsage.Runtime)
                {
                    visibleFields.Add(fields[index]);
                }
            }

            return visibleFields.ToArray();
        }

        private static SpriteArrayDefinition[] FilterArrays(SpriteArrayDefinition[] arrays)
        {
            var visibleArrays = new List<SpriteArrayDefinition>(arrays.Length);
            for (var index = 0; index < arrays.Length; index++)
            {
                if (arrays[index].Usage == SpriteConfigUsage.Runtime)
                {
                    visibleArrays.Add(arrays[index]);
                }
            }

            return visibleArrays.ToArray();
        }

        internal readonly struct TabDefinition
        {
            internal TabDefinition(string name, SpriteFieldDefinition[] fields, SpriteArrayDefinition[] arrayGroups = null)
            {
                Name = name;
                Fields = fields ?? new SpriteFieldDefinition[0];
                ArrayGroups = arrayGroups ?? new SpriteArrayDefinition[0];
            }

            internal string Name { get; }
            internal SpriteFieldDefinition[] Fields { get; }
            internal SpriteArrayDefinition[] ArrayGroups { get; }
        }

        internal readonly struct SpriteArrayDefinition
        {
            internal SpriteArrayDefinition(string propertyName, string label, string[] labels, int[] visibleIndices = null, SpriteConfigUsage usage = SpriteConfigUsage.Runtime)
            {
                PropertyName = propertyName;
                Label = label;
                Labels = labels;
                VisibleIndices = visibleIndices;
                Usage = usage;
            }

            internal string PropertyName { get; }
            internal string Label { get; }
            internal string[] Labels { get; }
            internal int[] VisibleIndices { get; }
            internal SpriteConfigUsage Usage { get; }
        }

        internal readonly struct SpriteFieldDefinition
        {
            internal SpriteFieldDefinition(string propertyName, string label, SpriteConfigUsage usage = SpriteConfigUsage.Runtime)
            {
                PropertyName = propertyName;
                Label = label;
                Usage = usage;
            }

            internal string PropertyName { get; }
            internal string Label { get; }
            internal SpriteConfigUsage Usage { get; }
        }
    }
}
