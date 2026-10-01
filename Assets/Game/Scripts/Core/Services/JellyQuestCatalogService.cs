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
        public sealed class JellyQuestCatalogService
        {
            public LegacyCatalogDto RawCatalog { get; private set; }
            public LevelDefinition[] Levels { get; private set; }
            public MapNodePosition[] MapNodePositions => RawCatalog.mapNodePositions;
            public BoosterUnlockTable BoosterUnlocks => RawCatalog.boosterUnlocks;

            public void Load()
            {
                var catalogText = Resources.Load<TextAsset>("JellyQuest/Data/jelly_quest_catalog");
                if (catalogText == null)
                {
                    throw new InvalidOperationException("Missing Jelly Quest catalog at Resources/JellyQuest/Data/jelly_quest_catalog.json");
                }

                RawCatalog = JsonUtility.FromJson<LegacyCatalogDto>(catalogText.text);
                Levels = new LevelDefinition[RawCatalog.levels.Length];
                for (var index = 0; index < RawCatalog.levels.Length; index++)
                {
                    Levels[index] = BuildLevelDefinition(RawCatalog.levels[index]);
                }
            }

            public bool IsTutorialLevel(int levelNumber)
            {
                if (RawCatalog == null || RawCatalog.tutorialLevels == null)
                {
                    return false;
                }

                foreach (var tutorialLevel in RawCatalog.tutorialLevels)
                {
                    if (tutorialLevel == levelNumber)
                    {
                        return true;
                    }
                }

                return false;
            }

            public LevelDefinition GetLevel(int levelNumber)
            {
                var index = Mathf.Clamp(levelNumber - 1, 0, Levels.Length - 1);
                return Levels[index];
            }

            private LevelDefinition BuildLevelDefinition(LegacyLevelDto rawLevel)
            {
                var cells = new GridCellSpec[rawLevel.boardTiles.Length];
                for (var index = 0; index < rawLevel.boardTiles.Length; index++)
                {
                    cells[index] = LegacyTileDecoder.Decode(rawLevel.boardTiles[index]);
                }

                var objectives = new ObjectiveDefinition[rawLevel.objectiveTypes.Length];
                for (var index = 0; index < objectives.Length; index++)
                {
                    objectives[index] = new ObjectiveDefinition
                    {
                        type = LegacyTileDecoder.ToObjectiveType(rawLevel.objectiveTypes[index]),
                        targetCount = rawLevel.objectiveCounts[index]
                    };
                }

                return new LevelDefinition
                {
                    levelNumber = rawLevel.levelNumber,
                    boardWidth = RawCatalog.boardWidth,
                    boardHeight = RawCatalog.boardHeight,
                    backgroundWidth = RawCatalog.backgroundWidth,
                    backgroundHeight = RawCatalog.backgroundHeight,
                    board = cells,
                    backgroundMask = rawLevel.backgroundTiles,
                    objectives = objectives,
                    moveCount = rawLevel.moveCount,
                    timerSeconds = rawLevel.timerSeconds
                };
            }
        }
}
