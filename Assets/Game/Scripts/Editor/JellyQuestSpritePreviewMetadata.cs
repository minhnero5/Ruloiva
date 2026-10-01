using System.Globalization;
using UnityEngine;

namespace JellyQuestUnity.Editor
{
    internal readonly struct JellyQuestSpritePreviewMetadata
    {
        internal JellyQuestSpritePreviewMetadata(int originalWidth, int originalHeight, int simplifiedWidth, int simplifiedHeight, double decimalRatio)
        {
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            SimplifiedWidth = simplifiedWidth;
            SimplifiedHeight = simplifiedHeight;
            DecimalRatio = decimalRatio;
        }

        internal int OriginalWidth { get; }
        internal int OriginalHeight { get; }
        internal int SimplifiedWidth { get; }
        internal int SimplifiedHeight { get; }
        internal double DecimalRatio { get; }

        internal string OriginalSizeLabel => string.Format(
            CultureInfo.InvariantCulture,
            "Original: {0} x {1} px",
            OriginalWidth,
            OriginalHeight);

        internal string RatioLabel => string.Format(
            CultureInfo.InvariantCulture,
            "Ratio: {0}:{1} ({2:0.###})",
            SimplifiedWidth,
            SimplifiedHeight,
            DecimalRatio);
    }

    internal static class JellyQuestSpritePreviewMetadataUtility
    {
        internal static bool TryCreate(Sprite sprite, out JellyQuestSpritePreviewMetadata metadata)
        {
            if (sprite == null)
            {
                metadata = default;
                return false;
            }

            var spriteRect = sprite.rect;
            var originalWidth = Mathf.RoundToInt(spriteRect.width);
            var originalHeight = Mathf.RoundToInt(spriteRect.height);
            if (originalWidth <= 0 || originalHeight <= 0)
            {
                metadata = default;
                return false;
            }

            var greatestCommonDivisor = Mathf.Max(1, GreatestCommonDivisor(originalWidth, originalHeight));
            metadata = new JellyQuestSpritePreviewMetadata(
                originalWidth,
                originalHeight,
                originalWidth / greatestCommonDivisor,
                originalHeight / greatestCommonDivisor,
                (double)originalWidth / originalHeight);
            return true;
        }

        private static int GreatestCommonDivisor(int left, int right)
        {
            left = Mathf.Abs(left);
            right = Mathf.Abs(right);

            while (right != 0)
            {
                var remainder = left % right;
                left = right;
                right = remainder;
            }

            return left;
        }
    }
}
