using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JellyQuestUnity
{
    public sealed class GameplayPieceView : MonoBehaviour
    {
        public SpriteRenderer rootRenderer;
        public TextMeshPro label;
        public MeshRenderer labelRenderer;
        public SpriteRenderer[] lockBars = new SpriteRenderer[3];
    }
}
