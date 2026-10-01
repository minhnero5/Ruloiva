using UnityEngine;
using UnityEngine.UI;

namespace JellyQuestUnity
{
    public sealed class MenuScreenView : MonoBehaviour
    {
        public Image backgroundImage;
        public Image logoImage;
        public Button startButton;
        public Button musicButton;
        public Image musicButtonImage;
        public Button creditsButton;
        public GameObject creditsOverlay;
        public Text creditsTitleText;
        public Text creditsBodyText;
        public Button creditsCloseButton;
        public Button shopOpenButton;
        public Button shopCloseButton;
        public IAPButtonView shopItemTemplate;
        public Image shopItemCoinIcon;
    }
}
