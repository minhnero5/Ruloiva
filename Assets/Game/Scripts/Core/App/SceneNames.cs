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
        public static class SceneNames
        {
            public const string Boot = "BootScene";
            public const string Menu = "MenuScene";
            public const string Map = "MapScene";
            public const string Gameplay = "GameplayScene";

            public static bool IsKnown(string sceneName)
            {
                return sceneName == Boot || sceneName == Menu || sceneName == Map || sceneName == Gameplay;
            }
        }
}
