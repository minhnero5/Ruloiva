using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
    public sealed partial class GameplayScreenController : GameScreenControllerBase
    {
                private const float BoardCellSizePixels = 80f;
                private const float BoardLeftPixels = 120f;
                private const float BoardTopPixels = 206f;
                private const float LegacyTimerFillFallbackWidth = 282f;
                private const float InvalidSwapDuration = 0.08f;
                private const float CascadePauseDuration = 0.05f;
                private const float InvalidSwapTravelUnits = 0.32f;
                private const float WinStarRevealDuration = 0.28f;
                private const float WinStarRevealDelay = 0.06f;
                private const float WinStarZeroRevealDelay = 0.24f;
                private const float WinStarStartScale = 0.2f;
                private const float WinStarOvershootScale = 1.18f;
                private const float WinStarStartOffsetY = 28f;
                private const float WinStarSideStartRotation = 18f;
                private const float WinStarCenterStartRotation = 10f;
                private const string DestroyEffectResourceName = "died_animation2";
                private const float DestroyEffectFrameWidthPixels = 90f;
                private const float DestroyEffectFrameHeightPixels = 90f;
                private const int DestroyEffectFrameCount = 8;
                private const float DestroyEffectDuration = 0.16f;
                private const float ScorePopupDuration = 0.45f;
                private const float ScorePopupTravelUnits = 0.55f;
                private const int ScorePopupShadowSortingOrder = 11;
                private const int ScorePopupSortingOrder = 12;
                private const int GridSortingOrder = -5;
                private const int IceSortingOrder = 0;
                private const int PieceSortingOrder = 5;
                private const int BoosterPreviewSortingOrder = 6;
                private const int DestroyEffectSortingOrder = 9;
                private const float BoosterPreviewPulseSpeed = 5.2f;

                private static readonly Dictionary<int, string> TutorialText = new Dictionary<int, string>
                {
                    { 1, "Match 3 or more jellies\nof the same\ntype to collect them." },
                    { 5, "Some jellies are locked,\nfree them by making a match." },
                    { 13, "Match the icy jellies\nand\ncollect enough ice cubes!" }
                };

                private static readonly Vector2 WinStarsContainerSize = new Vector2(360f, 180f);
                private static readonly Vector2 WinStarFallbackAnchoredPosition = new Vector2(0f, 54f);
                private static readonly Vector2 WinLeftStarAnchoredPosition = new Vector2(-116f, 0f);
                private static readonly Vector2 WinCenterStarAnchoredPosition = new Vector2(0f, 0f);
                private static readonly Vector2 WinRightStarAnchoredPosition = new Vector2(116f, 0f);
                private static readonly Vector2 WinLeftStarSize = new Vector2(139f, 140f);
                private static readonly Vector2 WinCenterStarSize = new Vector2(173f, 164f);
                private static readonly Vector2 WinRightStarSize = new Vector2(139f, 141f);
                private static readonly Vector3 IceScale = new Vector3(0.8f, 0.8f, 1f);
                private static readonly Vector3 ScorePopupStartScale = new Vector3(0.85f, 0.85f, 0.85f);
                private static readonly Vector3 ScorePopupOvershootScale = new Vector3(1.08f, 1.08f, 1.08f);
                private static readonly Vector3 ScorePopupEndScale = new Vector3(0.98f, 0.98f, 0.98f);
                private static readonly Vector3 ScorePopupShadowOffset = new Vector3(-0.02f, -0.02f, 0f);
                private static readonly Vector3 DestroyEffectScale = new Vector3(
                    BoardCellSizePixels / DestroyEffectFrameWidthPixels,
                    BoardCellSizePixels / DestroyEffectFrameHeightPixels,
                    1f);
                private static readonly Color LegacyObjectiveTextColor = new Color(0.37f, 0.17f, 0.02f, 1f);
                private static readonly Color LegacyLevelTextColor = new Color(0.78f, 0.18f, 0.12f, 1f);
                private static readonly Color LegacyMovesTextColor = new Color(0.87f, 0.14f, 0.1f, 1f);
                private static readonly Color ScorePopupTextColor = new Color(1f, 0.93f, 0.45f, 1f);
                private static readonly Color ScorePopupShadowColor = new Color(0.45f, 0.16f, 0.05f, 0.85f);
                private static readonly Color BoosterPreviewHintColor = new Color(0.38f, 0.88f, 1f, 1f);
                private static readonly Color BoosterPreviewTargetColor = new Color(1f, 0.8f, 0.18f, 1f);
                private static readonly Color ActiveBoosterButtonColor = new Color(1f, 0.95f, 0.68f, 1f);
                private static readonly Vector3 ActiveBoosterButtonScale = new Vector3(1.08f, 1.08f, 1f);

                private readonly List<ObjectiveRowView> _spawnedObjectiveRows = new List<ObjectiveRowView>();
                private readonly Dictionary<BoosterType, BoosterButtonBinding> _boosterBindings = new Dictionary<BoosterType, BoosterButtonBinding>();
                private readonly Dictionary<int, PieceVisual> _pieceVisuals = new Dictionary<int, PieceVisual>();

                private GameplayScreenView _view;
                private BoardModel _board;
                private GameObject _boardLayer;
                private GameObject _boosterPreviewLayer;
                private GameObject _gridTileTemplate;
                private GameObject _pieceTemplate;
                private CellVisual[,] _cellVisuals;
                private BoardVisualSnapshot _currentSnapshot;
                private Sprite _iceSprite;
                private BoosterType? _activeBooster;
                private Vector2Int? _boosterPreviewCell;
                private Coroutine _timerCoroutine;
                private Coroutine _toastCoroutine;
                private Coroutine _pendingEndCoroutine;
                private Coroutine _winStarsCoroutine;
                private bool _inputLocked;
                private bool _sessionPersisted;
                private float _legacyTimerFillBaseWidth;
                private Vector2Int? _dragStartCell;
                private Vector2 _dragStartScreen;
                private bool _levelOneSecondTutorialShown;

                public override void Initialize(JellyQuestApp app)
                {
                    base.Initialize(app);
                    _view = RequireSceneView<GameplayScreenView>();
                    App.Platform.ShowBanner();

                    var levelNumber = Mathf.Clamp(App.SceneFlow.SelectedLevel, 1, App.Catalog.Levels.Length);
                    _board = new BoardModel(App.Catalog.GetLevel(levelNumber), Environment.TickCount ^ (levelNumber * 7919));

                    BuildWorld();
                    PrepareTemplates();
                    EnsureLegacyGameplayHud();
                    EnsureWinPopupStars();
                    InitializeDynamicUiBindings();
                    BindStaticButtons();
                    HideAllOverlays();
                    HideToastImmediate();
                    RebuildObjectives();
                    BuildBoardVisuals();
                    UpdateHud();

                    App.Audio.PlayMusicLoop();
                    _timerCoroutine = StartCoroutine(TimerLoop());
                    if (TutorialText.TryGetValue(levelNumber, out var tutorial))
                    {
                        ShowTutorial(tutorial);
                    }
                }

                public override void DisposeScreen()
                {
                    App.Platform.HideBanner();
                    if (_timerCoroutine != null)
                    {
                        StopCoroutine(_timerCoroutine);
                    }

                    if (_toastCoroutine != null)
                    {
                        StopCoroutine(_toastCoroutine);
                    }

                    if (_pendingEndCoroutine != null)
                    {
                        StopCoroutine(_pendingEndCoroutine);
                    }

                    StopWinStarsAnimation();
                    ClearBoosterPreviewState();
                    _pieceVisuals.Clear();
                    _cellVisuals = null;
                    _currentSnapshot = null;
                    _boosterPreviewLayer = null;
                    _gridTileTemplate = null;
                    _pieceTemplate = null;
                    _legacyTimerFillBaseWidth = 0f;
                    ClearObjectiveRows();
                    base.DisposeScreen();
                }

                public override void RefreshFromSave()
                {
                    if (_board == null)
                    {
                        return;
                    }

                    if (_activeBooster.HasValue && App.Save.GetBoosterCount(_activeBooster.Value) <= 0)
                    {
                        _activeBooster = null;
                    }

                    if (!_activeBooster.HasValue)
                    {
                        ClearBoosterPreviewState();
                    }

                    UpdateHud();
                }

                private void Update()
                {
                    if (_board == null)
                    {
                        ClearBoosterPreviewState();
                        return;
                    }

                    if (_inputLocked || _board.EndState != BoardEndState.None || IsBlockingOverlayVisible())
                    {
                        HideBoosterPreviewImmediate();
                        return;
                    }

                    RefreshBoosterPreviewFromPointer();
                    HandlePointerInput();
                }

                private void BuildWorld()
                {
                    var levelNumber = _board.Definition.levelNumber;
                    CreateWorldSprite(LegacyUiSpriteLookup.GetGameplayBackground(App.UiSprites, App.Sprites, levelNumber), Vector3.zero, -20);
                    _boardLayer = new GameObject("BoardLayer");
                    _boardLayer.transform.SetParent(WorldRoot.transform, false);
                    _boosterPreviewLayer = new GameObject("BoosterPreviewLayer");
                    _boosterPreviewLayer.transform.SetParent(_boardLayer.transform, false);
                    _iceSprite = LegacyUiSpriteLookup.GetGameplayIce(App.UiSprites, App.Sprites);
                }

                private void PrepareTemplates()
                {
                    _gridTileTemplate = TemplateResources.LoadGridTileTemplate();
                    _pieceTemplate = TemplateResources.LoadGameplayPieceTemplate();
                }

                private void InitializeDynamicUiBindings()
                {
                    _boosterBindings.Clear();
                    if (_view.boosterButtons != null)
                    {
                        foreach (var boosterView in _view.boosterButtons)
                        {
                            if (boosterView == null)
                            {
                                continue;
                            }

                            _boosterBindings[boosterView.boosterType] = new BoosterButtonBinding
                            {
                                button = boosterView.button,
                                image = boosterView.image,
                                countText = boosterView.countText
                            };
                        }
                    }
                }

                private Color GetBoosterPreviewHintColor()
                {
                    return ResolveConfiguredColor(
                        App != null && App.UiSprites != null ? App.UiSprites.gameplayBoosterPreviewHintColor : default,
                        BoosterPreviewHintColor);
                }

                private Color GetBoosterPreviewTargetColor()
                {
                    return ResolveConfiguredColor(
                        App != null && App.UiSprites != null ? App.UiSprites.gameplayBoosterPreviewTargetColor : default,
                        BoosterPreviewTargetColor);
                }

                private Color GetActiveBoosterButtonColor()
                {
                    return ResolveConfiguredColor(
                        App != null && App.UiSprites != null ? App.UiSprites.gameplayBoosterActiveButtonColor : default,
                        ActiveBoosterButtonColor);
                }

                private static Color ResolveConfiguredColor(Color configuredColor, Color fallbackColor)
                {
                    return configuredColor.a > 0.0001f ? configuredColor : fallbackColor;
                }
    }
}
