using UnityEngine;
using UnityEngine.UI;

namespace JellyQuestUnity
{
    public sealed class LevelButtonView : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Button button;
        public Image frameImage;
        public Text labelText;
        public Image[] starImages = new Image[3];
    }
}
