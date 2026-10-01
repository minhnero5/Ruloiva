using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
        public abstract class GameScreenControllerBase : MonoBehaviour, IGameScreenController
        {
            protected const float ReferenceWidth = 640f;
            protected const float ReferenceHeight = 960f;

            protected JellyQuestApp App;
            protected GameObject WorldRoot;

            public virtual void Initialize(JellyQuestApp app)
            {
                App = app;
                WorldRoot = new GameObject(GetType().Name + "WorldRoot");
                WorldRoot.transform.SetParent(transform, false);
            }

            public virtual void DisposeScreen()
            {
                if (WorldRoot != null)
                {
                    Destroy(WorldRoot);
                }
            }

            public virtual void RefreshFromSave()
            {
            }

            protected T RequireSceneView<T>() where T : Component
            {
                var activeScene = SceneManager.GetActiveScene();
                foreach (var candidate in Resources.FindObjectsOfTypeAll<T>())
                {
                    if (candidate != null && candidate.gameObject.scene == activeScene)
                    {
                        return candidate;
                    }
                }

                throw new InvalidOperationException("Missing scene view: " + typeof(T).Name);
            }

            protected static void BindButton(Button button, Action action)
            {
                if (button == null)
                {
                    return;
                }

                button.onClick.RemoveAllListeners();
                if (action != null)
                {
                    button.onClick.AddListener(() => action());
                }
            }

            protected static void SetTopLeftRect(RectTransform rectTransform, Vector2 pixelPosition, Vector2 size)
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(pixelPosition.x, -pixelPosition.y);
                rectTransform.sizeDelta = size;
            }

            protected Vector3 PixelToWorld(float x, float y, int sortingOrder = 0)
            {
                return new Vector3((x - (ReferenceWidth * 0.5f)) / 100f, ((ReferenceHeight * 0.5f) - y) / 100f, sortingOrder * 0.01f);
            }

            protected SpriteRenderer CreateWorldSprite(Sprite sprite, Vector3 position, int sortingOrder, Vector3 scale)
            {
                var gameObject = new GameObject(sprite != null ? sprite.name : "WorldSprite", typeof(SpriteRenderer));
                gameObject.transform.SetParent(WorldRoot.transform, false);
                gameObject.transform.localPosition = position;
                gameObject.transform.localScale = scale;
                var renderer = gameObject.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;
                return renderer;
            }

            protected SpriteRenderer CreateWorldSprite(Sprite sprite, Vector3 position, int sortingOrder)
            {
                return CreateWorldSprite(sprite, position, sortingOrder, Vector3.one);
            }
        }
}
