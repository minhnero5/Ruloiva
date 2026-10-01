using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
        public sealed class MapScreenController : GameScreenControllerBase
        {
            private static readonly Color InactiveStarColor = new Color(1f, 1f, 1f, 0.35f);

            private readonly List<LevelButtonView> _spawnedButtons = new List<LevelButtonView>();
            private MapScreenView _view;
            private LevelButtonView _levelButtonTemplate;

            public override void Initialize(JellyQuestApp app)
            {
                base.Initialize(app);
                _view = RequireSceneView<MapScreenView>();
                _levelButtonTemplate = TemplateResources.LoadLevelButtonTemplate();
                InitializeSceneContent();
                ClearGeneratedButtons();
                BuildButtons();
                ScrollToLevel(Mathf.Clamp(App.Save.Profile.highestUnlockedLevel, 1, App.Catalog.MapNodePositions.Length));
            }

            public override void DisposeScreen()
            {
                ClearGeneratedButtons();
                _levelButtonTemplate = null;
                base.DisposeScreen();
            }

            public override void RefreshFromSave()
            {
                if (_view == null || _view.content == null)
                {
                    return;
                }

                var currentPosition = _view.content.anchoredPosition;
                ClearGeneratedButtons();
                BuildButtons();
                RestoreScrollPosition(currentPosition);
            }

            private void InitializeSceneContent()
            {
                if (_view.headerText != null)
                {
                    _view.headerText.text = "Level Selection";
                }

                BindButton(_view.homeButton, () =>
                {
                    App.Audio.PlayMarker("bouton");
                    App.SceneFlow.LoadMenu();
                });
            }

            private void BuildButtons()
            {
                if (_levelButtonTemplate == null || _view.buttonsContainer == null)
                {
                    return;
                }

                var starSprite = GetLevelButtonStarSprite();

                foreach (var node in App.Catalog.MapNodePositions)
                {
                    var nodeNumber = node.num;
                    var isUnlocked = nodeNumber <= App.Save.Profile.highestUnlockedLevel;
                    var stars = isUnlocked ? Mathf.Clamp(App.Save.GetStars(nodeNumber), 0, 3) : 0;
                    var sprite = GetLevelButtonSprite(isUnlocked);

                    var buttonView = Instantiate(_levelButtonTemplate, _view.buttonsContainer);
                    buttonView.gameObject.name = "Level" + nodeNumber;
                    buttonView.gameObject.SetActive(true);
                    SetTopLeftRect(buttonView.rectTransform, new Vector2(node.x, node.y + 1920f), new Vector2(81f, 105f));
                    buttonView.frameImage.sprite = sprite;
                    buttonView.frameImage.color = Color.white;
                    buttonView.button.interactable = isUnlocked;
                    buttonView.labelText.text = isUnlocked ? nodeNumber.ToString() : string.Empty;
                    buttonView.labelText.enabled = isUnlocked;
                    ConfigureStars(buttonView, isUnlocked, stars, starSprite);
                    BindButton(buttonView.button, () =>
                    {
                        if (!isUnlocked)
                        {
                            return;
                        }

                        App.Audio.PlayMarker("bouton");
                        App.SceneFlow.LoadGameplay(nodeNumber);
                    });

                    _spawnedButtons.Add(buttonView);
                }
            }

            private void ClearGeneratedButtons()
            {
                foreach (var buttonView in _spawnedButtons)
                {
                    if (buttonView != null)
                    {
                        Destroy(buttonView.gameObject);
                    }
                }

                _spawnedButtons.Clear();
            }

            private void ScrollToLevel(int levelNumber)
            {
                foreach (var node in App.Catalog.MapNodePositions)
                {
                    if (node.num != levelNumber)
                    {
                        continue;
                    }

                    var target = Mathf.Clamp((node.y + 1920f) - 480f, 0f, _view.content.sizeDelta.y - _view.viewport.rect.height);
                    _view.content.anchoredPosition = new Vector2(0f, target);
                    return;
                }
            }

            private void RestoreScrollPosition(Vector2 currentPosition)
            {
                if (_view.content == null)
                {
                    return;
                }

                var viewportHeight = _view.viewport != null ? _view.viewport.rect.height : 0f;
                var maxY = Mathf.Max(0f, _view.content.sizeDelta.y - viewportHeight);
                _view.content.anchoredPosition = new Vector2(currentPosition.x, Mathf.Clamp(currentPosition.y, 0f, maxY));
            }

            private Sprite GetLevelButtonSprite(bool unlocked)
            {
                return LegacyUiSpriteLookup.GetLevelButtonSprite(App.UiSprites, App.Sprites, unlocked);
            }

            private Sprite GetLevelButtonStarSprite()
            {
                return LegacyUiSpriteLookup.GetGameplayTinyStar(App.UiSprites, App.Sprites);
            }

            private static void ConfigureStars(LevelButtonView buttonView, bool isUnlocked, int stars, Sprite starSprite)
            {
                if (buttonView.starImages == null)
                {
                    return;
                }

                for (var index = 0; index < buttonView.starImages.Length; index++)
                {
                    var starImage = buttonView.starImages[index];
                    if (starImage == null)
                    {
                        continue;
                    }

                    starImage.gameObject.SetActive(isUnlocked);
                    if (!isUnlocked)
                    {
                        continue;
                    }

                    starImage.sprite = starSprite;
                    starImage.color = index < stars ? Color.white : InactiveStarColor;
                }
            }

        }
}
