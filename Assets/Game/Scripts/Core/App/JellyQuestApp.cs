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
        public sealed class JellyQuestApp : MonoBehaviour
        {
            private const float ReferenceWidth = 640f;
            private const float ReferenceHeight = 960f;
            private const int TransitionSortingOrder = 1000;

            public static JellyQuestApp Instance { get; private set; }

            public SaveService Save { get; private set; }
            public AudioService Audio { get; private set; }
            public PlatformServices Platform { get; private set; }
            public SceneFlowService SceneFlow { get; private set; }
            public JellyQuestCatalogService Catalog { get; private set; }
            public LegacySpriteLibrary Sprites { get; private set; }
            public LegacyUiSpriteCatalog UiSprites { get; private set; }

            private GameObject _activeScreenHost;
            private IGameScreenController _activeScreenController;
            private CanvasGroup _transitionCanvasGroup;

            private void Awake()
            {
                if (Instance != null && Instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                Instance = this;
                Save = new SaveService();
                Catalog = new JellyQuestCatalogService();
                Catalog.Load();
                Sprites = new LegacySpriteLibrary();
                UiSprites = Resources.Load<LegacyUiSpriteCatalog>("JellyQuest/Data/LegacyUiSpriteCatalog");
                Platform = new PlatformServices();
                Audio = new AudioService(this, Sprites);
                EnsureTransitionOverlay();
                SceneFlow = new SceneFlowService(this);
                Save.LoadOrCreate();
                Audio.SetMuted(!Save.Profile.musicEnabled);
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
            }

            private void OnEnable()
            {
                SceneManager.sceneLoaded += HandleSceneLoaded;
            }

            private void OnDisable()
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }

            private void Start()
            {
                var currentScene = SceneManager.GetActiveScene().name;
                if (!SceneNames.IsKnown(currentScene))
                {
                    SceneManager.LoadScene(SceneNames.Boot);
                }
                else
                {
                    ConfigureCamera();
                    BuildScreenForScene(currentScene);
                }
            }

            private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                ConfigureCamera();
                BuildScreenForScene(scene.name);
            }

            private void ConfigureCamera()
            {
                var camera = Camera.main;
                if (camera == null)
                {
                    return;
                }

                camera.orthographic = true;
                camera.orthographicSize = ReferenceHeight / 200f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.11f, 0.12f, 0.18f, 1f);
            }

            private void BuildScreenForScene(string sceneName)
            {
                if (_activeScreenController != null)
                {
                    _activeScreenController.DisposeScreen();
                    _activeScreenController = null;
                }

                if (_activeScreenHost != null)
                {
                    Destroy(_activeScreenHost);
                    _activeScreenHost = null;
                }

                _activeScreenHost = new GameObject(sceneName + "Host");
                _activeScreenHost.transform.SetParent(transform, false);

                switch (sceneName)
                {
                    case SceneNames.Boot:
                        _activeScreenController = _activeScreenHost.AddComponent<BootScreenController>();
                        break;
                    case SceneNames.Menu:
                        _activeScreenController = _activeScreenHost.AddComponent<MenuScreenController>();
                        break;
                    case SceneNames.Map:
                        _activeScreenController = _activeScreenHost.AddComponent<MapScreenController>();
                        break;
                    case SceneNames.Gameplay:
                        _activeScreenController = _activeScreenHost.AddComponent<GameplayScreenController>();
                        break;
                    default:
                        return;
                }

                _activeScreenController.Initialize(this);
            }

            internal IEnumerator FadeTransitionOverlay(float targetAlpha, float duration)
            {
                EnsureTransitionOverlay();
                if (_transitionCanvasGroup == null)
                {
                    yield break;
                }

                var overlay = _transitionCanvasGroup.gameObject;
                overlay.SetActive(true);
                _transitionCanvasGroup.interactable = true;
                _transitionCanvasGroup.blocksRaycasts = true;

                var startAlpha = _transitionCanvasGroup.alpha;
                if (duration <= 0f)
                {
                    _transitionCanvasGroup.alpha = targetAlpha;
                }
                else
                {
                    var elapsed = 0f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        _transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                        yield return null;
                    }

                    _transitionCanvasGroup.alpha = targetAlpha;
                }

                var keepVisible = targetAlpha > 0.001f;
                _transitionCanvasGroup.interactable = keepVisible;
                _transitionCanvasGroup.blocksRaycasts = keepVisible;
                if (!keepVisible)
                {
                    overlay.SetActive(false);
                }
            }

            public void RefreshActiveScreenFromSave()
            {
                if (_activeScreenController == null)
                {
                    return;
                }

                _activeScreenController.RefreshFromSave();
            }

            private void EnsureTransitionOverlay()
            {
                if (_transitionCanvasGroup != null)
                {
                    return;
                }

                var canvasObject = new GameObject("SceneTransitionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.transform.SetParent(transform, false);

                var canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = TransitionSortingOrder;

                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                var blockerObject = new GameObject("TransitionBlocker", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
                blockerObject.transform.SetParent(canvasObject.transform, false);

                var blockerRect = blockerObject.GetComponent<RectTransform>();
                blockerRect.anchorMin = Vector2.zero;
                blockerRect.anchorMax = Vector2.one;
                blockerRect.pivot = new Vector2(0.5f, 0.5f);
                blockerRect.anchoredPosition = Vector2.zero;
                blockerRect.sizeDelta = Vector2.zero;

                var blockerImage = blockerObject.GetComponent<Image>();
                blockerImage.color = Color.black;
                blockerImage.raycastTarget = true;

                _transitionCanvasGroup = blockerObject.GetComponent<CanvasGroup>();
                _transitionCanvasGroup.alpha = 0f;
                _transitionCanvasGroup.interactable = false;
                _transitionCanvasGroup.blocksRaycasts = false;
                blockerObject.SetActive(false);
            }
        }
}
