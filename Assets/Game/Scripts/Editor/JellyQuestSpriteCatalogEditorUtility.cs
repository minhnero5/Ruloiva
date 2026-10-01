using System.IO;
using UnityEditor;
using UnityEngine;

namespace JellyQuestUnity.Editor
{
    internal static class JellyQuestSpriteCatalogEditorUtility
    {
        internal const string CatalogAssetPath = "Assets/Imported/JellyQuest/Resources/JellyQuest/Data/LegacyUiSpriteCatalog.asset";
        internal const string CatalogResourcePath = "JellyQuest/Data/LegacyUiSpriteCatalog";

        internal static LegacyUiSpriteCatalog LoadCatalog()
        {
            return AssetDatabase.LoadAssetAtPath<LegacyUiSpriteCatalog>(CatalogAssetPath);
        }

        internal static LegacyUiSpriteCatalog LoadOrCreateCatalog()
        {
            var catalog = LoadCatalog();
            if (catalog != null)
            {
                return catalog;
            }

            var directory = Path.GetDirectoryName(CatalogAssetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            catalog = ScriptableObject.CreateInstance<LegacyUiSpriteCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return catalog;
        }

        internal static void SelectCatalogAsset(LegacyUiSpriteCatalog catalog)
        {
            if (catalog == null)
            {
                return;
            }

            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
        }
    }
}
