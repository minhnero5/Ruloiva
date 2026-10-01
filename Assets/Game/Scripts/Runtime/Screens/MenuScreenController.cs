using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
        public sealed class MenuScreenController : GameScreenControllerBase
        {
            private MenuScreenView _view;

            public override void Initialize(JellyQuestApp app)
            {
                base.Initialize(app);
                _view = RequireSceneView<MenuScreenView>();
                InitializeSceneContent();
                HideCreditsOverlay();

                BindButton(_view.startButton, () =>
                {
                    App.Audio.PlayMarker("bouton");
                    App.SceneFlow.LoadMap();
                });

                BindButton(_view.musicButton, ToggleMusic);
                BindButton(_view.creditsButton, () =>
                {
                    App.Audio.PlayMarker("bouton");
                    ShowCredits();
                });
                BindButton(_view.creditsCloseButton, () =>
                {
                    App.Audio.PlayMarker("bouton");
                    HideCreditsOverlay();
                });

                RefreshMusicButton();
                App.Audio.PlayMusicLoop();
            }

            public override void RefreshFromSave()
            {
            }

            private void InitializeSceneContent()
            {
                if (_view.creditsTitleText != null)
                {
                    _view.creditsTitleText.text = "Jelly Quest Mania";
                }

                if (_view.creditsBodyText != null)
                {
                    _view.creditsBodyText.text = "Unity 6 native port of the legacy HTML5 Android release.\nOffline build: gameplay, save, boosters, map and content preserved.";
                }
            }

            private void ToggleMusic()
            {
                var enabled = !App.Save.Profile.musicEnabled;
                App.Save.SetMusicEnabled(enabled);
                App.Audio.SetMuted(!enabled);
                if (enabled)
                {
                    App.Audio.PlayMusicLoop();
                }
                else
                {
                    App.Audio.StopMusic();
                }

                RefreshMusicButton();
            }

            private void RefreshMusicButton()
            {
                if (_view.musicButtonImage == null)
                {
                    return;
                }

                LegacyUiSpriteSceneApplier.ApplyMusicButtonState(
                    _view.musicButtonImage,
                    App.UiSprites,
                    App.Sprites,
                    App.Save.Profile.musicEnabled);
            }

            private void ShowCredits()
            {
                if (_view.creditsOverlay != null)
                {
                    _view.creditsOverlay.SetActive(true);
                }
            }

            private void HideCreditsOverlay()
            {
                if (_view.creditsOverlay != null)
                {
                    _view.creditsOverlay.SetActive(false);
                }
            }

        }
}
