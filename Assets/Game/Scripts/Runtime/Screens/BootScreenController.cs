using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
        public sealed class BootScreenController : GameScreenControllerBase
        {
            private BootScreenView _view;

            public override void Initialize(JellyQuestApp app)
            {
                base.Initialize(app);
                _view = RequireSceneView<BootScreenView>();
                InitializeSceneContent();
                if (_view.loadingText != null)
                {
                    _view.loadingText.text = "Loading...";
                }

                StartCoroutine(BootRoutine());
            }

            public override void RefreshFromSave()
            {
            }

            private IEnumerator BootRoutine()
            {
                yield return null;
                App.Audio.PlayMusicLoop();
                yield return new WaitForSecondsRealtime(0.4f);
                App.SceneFlow.LoadMenu();
            }

            private void InitializeSceneContent()
            {
            }

        }
}
