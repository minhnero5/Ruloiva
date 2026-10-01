using UnityEngine;
using UnityEngine.UI;

namespace JellyQuestUnity
{
    public sealed class GameplayScreenView : MonoBehaviour
    {
        public Text movesText;
        public Text scoreText;
        public Text timerText;
        public Text coinsText;
        public Text levelText;
        public Image legacyMovesPanelImage;
        public Image legacyObjectivePanelImage;
        public Image legacyObjectiveStripImage;
        public Image legacyTimerBarBackgroundImage;
        public RectTransform legacyTimerBarFillRect;
        public Image legacyTimerBarFillImage;
        public Image legacyTimerClockImage;
        public Button musicButton;
        public Image musicButtonImage;
        public Button pauseButton;
        public RectTransform objectiveContainer;
        public ObjectiveRowView objectiveRowTemplate;
        public BoosterButtonView[] boosterButtons;
        public GameObject pauseOverlay;
        public Button pauseResumeButton;
        public Button pauseReplayButton;
        public Button pauseMenuButton;
        public GameObject shopOverlay;
        public Text shopPriceText;
        public Text shopCoinsText;
        public Text shopOwnedText;
        public Image shopIconImage;
        public Button shopBuyButton;
        public Button shopCloseButton;
        public GameObject winOverlay;
        public Text winScoreText;
        public Text winStarsText;
        public RectTransform winStarsContainer;
        public Image winLeftStarImage;
        public Image winCenterStarImage;
        public Image winRightStarImage;
        public Button winNextButton;
        public Button winMapButton;
        public Button winReplayButton;
        public GameObject loseOverlay;
        public Text loseInfoText;
        public Button loseReplayButton;
        public Button loseMapButton;
        public GameObject tutorialOverlay;
        public Text tutorialText;
        public Button tutorialOkButton;
        public CanvasGroup toastCanvasGroup;
        public Text toastText;
    }
}
