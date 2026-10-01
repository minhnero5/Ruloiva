using System;
using NUnit.Framework;
using UnityEngine;

namespace JellyQuestUnity.EditorTests
{
    public sealed class JellyQuestTemplateResourceTests
    {
        [Test]
        public void LevelButtonTemplate_LoadsAndHasAssignedViewReferences()
        {
            var template = TemplateResources.LoadLevelButtonTemplate();

            Assert.That(template, Is.Not.Null);
            Assert.That(template.rectTransform, Is.Not.Null);
            Assert.That(template.button, Is.Not.Null);
            Assert.That(template.frameImage, Is.Not.Null);
            Assert.That(template.labelText, Is.Not.Null);
            Assert.That(template.button.colors.disabledColor, Is.EqualTo(Color.white));
            Assert.That(template.starImages, Is.Not.Null);
            Assert.That(template.starImages, Has.Length.EqualTo(3));
            Assert.That(template.starImages, Has.All.Not.Null);
        }

        [Test]
        public void GridTileTemplate_LoadsAndHasRootSpriteRenderer()
        {
            var template = TemplateResources.LoadGridTileTemplate();

            Assert.That(template, Is.Not.Null);
            Assert.That(template.GetComponent<SpriteRenderer>(), Is.Not.Null);
        }

        [Test]
        public void GameplayPieceTemplate_LoadsAndHasPieceVisualComponents()
        {
            var template = TemplateResources.LoadGameplayPieceTemplate();
            var view = template.GetComponent<GameplayPieceView>();

            Assert.That(template, Is.Not.Null);
            Assert.That(view, Is.Not.Null);
            Assert.That(view.rootRenderer, Is.Not.Null);
            Assert.That(view.label, Is.Not.Null);
            Assert.That(view.labelRenderer, Is.Not.Null);
            Assert.That(view.lockBars, Is.Not.Null);
            Assert.That(view.lockBars, Has.Length.EqualTo(3));
            Assert.That(view.lockBars, Has.All.Not.Null);
        }

        [Test]
        public void LoadRequiredGameObject_ThrowsForMissingPath()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => TemplateResources.LoadRequiredGameObject("JellyQuest/Templates/DefinitelyMissingTemplate"));

            Assert.That(exception.Message, Does.Contain("DefinitelyMissingTemplate"));
        }
    }
}
