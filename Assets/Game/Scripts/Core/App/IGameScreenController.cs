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
        public interface IGameScreenController
        {
            void Initialize(JellyQuestApp app);
            void DisposeScreen();
            void RefreshFromSave();
        }
}
