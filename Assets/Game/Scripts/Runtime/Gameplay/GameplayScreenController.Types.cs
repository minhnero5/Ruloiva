using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
    public sealed partial class GameplayScreenController
    {
                private sealed class CellVisual
                {
                    public SpriteRenderer gridRenderer;
                    public SpriteRenderer iceRenderer;
                    public SpriteRenderer boosterPreviewRenderer;
                }

                private sealed class PieceVisual
                {
                    public int runtimeId;
                    public Vector2Int coordinate;
                    public GameObject gameObject;
                    public Transform transform;
                    public Vector3 baseLocalScale;
                    public SpriteRenderer renderer;
                    public SpriteRenderer[] lockBars;
                    public TextMeshPro label;
                    public MeshRenderer labelRenderer;
                }

                private sealed class DestroyEffectVisual
                {
                    public GameObject gameObject;
                    public SpriteRenderer renderer;
                }

                private sealed class ScorePopupVisual
                {
                    public GameObject root;
                    public Transform transform;
                    public Vector3 baseLocalScale;
                    public TextMeshPro mainLabel;
                    public MeshRenderer mainRenderer;
                    public TextMeshPro shadowLabel;
                    public MeshRenderer shadowRenderer;
                }

                private struct PieceMoveAnimation
                {
                    public PieceVisual visual;
                    public Vector3 from;
                    public Vector3 to;
                }

                private struct WinStarRevealStep
                {
                    public Image image;
                    public Vector2 anchoredPosition;
                    public float startAngle;
                }

                private sealed class BoosterButtonBinding
                {
                    public Button button;
                    public Image image;
                    public Text countText;
                }
    }
}
