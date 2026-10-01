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
        public static class JellyQuestRuntimeBootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Bootstrap()
            {
                if (JellyQuestApp.Instance != null)
                {
                    return;
                }

                var root = new GameObject("JellyQuestApp");
                root.AddComponent<JellyQuestApp>();
                UnityEngine.Object.DontDestroyOnLoad(root);
            }
        }
}
