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
        public sealed class SceneFlowService
        {
            private const float FadeOutDuration = 0.2f;
            private const float FadeInDuration = 0.2f;

            private readonly JellyQuestApp _app;
            private bool _isTransitioning;

            public int SelectedLevel { get; private set; } = 1;

            public SceneFlowService(JellyQuestApp app)
            {
                _app = app;
            }

            public void LoadBoot()
            {
                BeginTransition(SceneNames.Boot);
            }

            public void LoadMenu()
            {
                BeginTransition(SceneNames.Menu);
            }

            public void LoadMap()
            {
                BeginTransition(SceneNames.Map);
            }

            public void LoadGameplay(int levelNumber)
            {
                SelectedLevel = Mathf.Max(1, levelNumber);
                BeginTransition(SceneNames.Gameplay);
            }

            private void BeginTransition(string sceneName)
            {
                if (_app == null || _isTransitioning)
                {
                    return;
                }

                _app.StartCoroutine(TransitionToScene(sceneName));
            }

            private IEnumerator TransitionToScene(string sceneName)
            {
                _isTransitioning = true;

                yield return _app.FadeTransitionOverlay(1f, FadeOutDuration);

                var loadOperation = SceneManager.LoadSceneAsync(sceneName);
                if (loadOperation == null)
                {
                    Debug.LogError("Failed to load scene transition target: " + sceneName);
                    yield return _app.FadeTransitionOverlay(0f, FadeInDuration);
                    _isTransitioning = false;
                    yield break;
                }

                while (!loadOperation.isDone)
                {
                    yield return null;
                }

                yield return null;
                yield return _app.FadeTransitionOverlay(0f, FadeInDuration);
                _isTransitioning = false;
            }
        }
}
