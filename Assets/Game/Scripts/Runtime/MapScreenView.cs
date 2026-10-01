using UnityEngine;
using UnityEngine.UI;

namespace JellyQuestUnity
{
    public sealed class MapScreenView : MonoBehaviour
    {
        public Text headerText;
        public Button homeButton;
        public ScrollRect scrollRect;
        public RectTransform viewport;
        public RectTransform content;
        public Image map3Image;
        public Image map2Image;
        public Image map1Image;
        public RectTransform buttonsContainer;
    }
}
