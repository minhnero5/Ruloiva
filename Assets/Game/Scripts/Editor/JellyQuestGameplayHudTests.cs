using NUnit.Framework;
using UnityEngine;

namespace JellyQuestUnity.EditorTests
{
    public sealed class JellyQuestGameplayHudTests
    {
        [Test]
        public void LegacyTimerFillBaseWidth_UsesSceneRectWidth()
        {
            var gameObject = new GameObject("LegacyTimerBarFill", typeof(RectTransform));

            try
            {
                var rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(400f, 21.844f);

                Assert.That(
                    GameplayScreenController.ResolveLegacyTimerFillBaseWidth(rectTransform, 282f),
                    Is.EqualTo(400f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void LegacyTimerFillBaseWidth_FallsBackWhenSceneWidthIsInvalid()
        {
            var gameObject = new GameObject("LegacyTimerBarFill", typeof(RectTransform));

            try
            {
                var rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(0f, 21.844f);

                Assert.That(
                    GameplayScreenController.ResolveLegacyTimerFillBaseWidth(rectTransform, 282f),
                    Is.EqualTo(282f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void LegacyTimerFillWidth_UsesCachedBaseWidthAcrossRatios()
        {
            Assert.That(GameplayScreenController.CalculateLegacyTimerFillWidth(400f, 80, 80), Is.EqualTo(400f));
            Assert.That(GameplayScreenController.CalculateLegacyTimerFillWidth(400f, 40, 80), Is.EqualTo(200f));
            Assert.That(GameplayScreenController.CalculateLegacyTimerFillWidth(400f, 0, 80), Is.EqualTo(0f));
        }

        [Test]
        public void LegacyTimerFillWidth_RemainsSafeWhenTimerDurationIsZeroOrNegative()
        {
            Assert.That(GameplayScreenController.CalculateLegacyTimerFillWidth(400f, 10, 0), Is.EqualTo(0f));
            Assert.That(GameplayScreenController.CalculateLegacyTimerFillWidth(400f, 10, -5), Is.EqualTo(0f));
        }
    }
}
