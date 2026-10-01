using System;
using UnityEngine;

namespace JellyQuestUnity
{
    internal static class TemplateResources
    {
        internal const string LevelButtonTemplatePath = "JellyQuest/Templates/LevelButtonTemplate";
        internal const string GridTileTemplatePath = "JellyQuest/Templates/GridTileTemplate";
        internal const string GameplayPieceTemplatePath = "JellyQuest/Templates/GameplayPieceTemplate";

        internal static LevelButtonView LoadLevelButtonTemplate()
        {
            var prefab = LoadRequiredGameObject(LevelButtonTemplatePath);
            var view = prefab.GetComponent<LevelButtonView>();
            if (view == null
                || view.rectTransform == null
                || view.button == null
                || view.frameImage == null
                || view.labelText == null
                || view.starImages == null
                || view.starImages.Length != 3
                || Array.Exists(view.starImages, image => image == null))
            {
                throw new InvalidOperationException(
                    "Template at Resources/" + LevelButtonTemplatePath + ".prefab must contain a LevelButtonView with rectTransform, button, frameImage, labelText, and three assigned starImages.");
            }

            return view;
        }

        internal static GameObject LoadGridTileTemplate()
        {
            var prefab = LoadRequiredGameObject(GridTileTemplatePath);
            if (prefab.GetComponent<SpriteRenderer>() == null)
            {
                throw new InvalidOperationException(
                    "Template at Resources/" + GridTileTemplatePath + ".prefab must contain a root SpriteRenderer.");
            }

            return prefab;
        }

        internal static GameObject LoadGameplayPieceTemplate()
        {
            var prefab = LoadRequiredGameObject(GameplayPieceTemplatePath);
            var view = prefab.GetComponent<GameplayPieceView>();
            if (view == null
                || view.rootRenderer == null
                || view.label == null
                || view.labelRenderer == null
                || view.lockBars == null
                || view.lockBars.Length != 3
                || Array.Exists(view.lockBars, renderer => renderer == null))
            {
                throw new InvalidOperationException(
                    "Template at Resources/" + GameplayPieceTemplatePath + ".prefab must contain a GameplayPieceView with rootRenderer, label, labelRenderer, and three assigned lockBars.");
            }

            return prefab;
        }

        internal static GameObject LoadRequiredGameObject(string resourcePath)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    "Missing required template prefab at Resources/" + resourcePath + ".prefab");
            }

            return prefab;
        }
    }
}
