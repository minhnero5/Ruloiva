using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JellyQuestUnity.Editor
{
    internal static class JellyQuestSpriteScenePreview
    {
        internal static bool RefreshActiveScene(LegacyUiSpriteCatalog catalog)
        {
            return RefreshBuildScenes(catalog);
        }

        internal static bool RefreshBuildScenes(LegacyUiSpriteCatalog catalog)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Sprite preview only runs in edit mode.");
                return false;
            }

            if (catalog == null)
            {
                Debug.LogWarning("Missing LegacyUiSpriteCatalog asset.");
                return false;
            }

            var buildScenes = EditorBuildSettings.scenes;
            if (buildScenes == null || buildScenes.Length == 0)
            {
                Debug.LogWarning("No scenes in Build Settings to refresh.");
                return false;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            var originalSetup = EditorSceneManager.GetSceneManagerSetup();
            var sprites = new LegacySpriteLibrary();
            var refreshedSceneCount = 0;
            var supportedSceneCount = 0;

            try
            {
                for (var index = 0; index < buildScenes.Length; index++)
                {
                    var buildScene = buildScenes[index];
                    if (buildScene == null || !buildScene.enabled || string.IsNullOrEmpty(buildScene.path))
                    {
                        continue;
                    }

                    var scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                    if (!TryApplyToScene(scene, catalog, sprites))
                    {
                        continue;
                    }

                    supportedSceneCount += 1;
                    if (EditorSceneManager.SaveScene(scene))
                    {
                        refreshedSceneCount += 1;
                    }
                    else
                    {
                        Debug.LogWarning("Failed to save refreshed scene: " + buildScene.path);
                    }
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
            }

            if (supportedSceneCount == 0)
            {
                Debug.LogWarning("No enabled Build Settings scenes contain a supported Jelly Quest screen view.");
                return false;
            }

            if (refreshedSceneCount > 0)
            {
                Debug.Log("Refreshed sprite preview in " + refreshedSceneCount + " Build Settings scene(s).");
            }

            return refreshedSceneCount > 0;
        }

        internal static void ApplyToBoot(BootScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites = null)
        {
            LegacyUiSpriteSceneApplier.Apply(view, catalog, sprites);
        }

        internal static void ApplyToMenu(MenuScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites = null, bool musicEnabled = true)
        {
            LegacyUiSpriteSceneApplier.Apply(view, catalog, sprites, musicEnabled);
        }

        internal static void ApplyToMap(MapScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites = null)
        {
            LegacyUiSpriteSceneApplier.Apply(view, catalog, sprites);
        }

        internal static void ApplyToGameplay(GameplayScreenView view, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites = null, bool musicEnabled = true)
        {
            LegacyUiSpriteSceneApplier.Apply(view, catalog, sprites, musicEnabled);
        }

        private static bool TryApplyToScene(Scene scene, LegacyUiSpriteCatalog catalog, LegacySpriteLibrary sprites)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return false;
            }

            if (TryFindSceneComponent(scene, out GameplayScreenView gameplayView))
            {
                ApplyToGameplay(gameplayView, catalog, sprites);
                MarkSceneDirty(scene, gameplayView);
                return true;
            }

            if (TryFindSceneComponent(scene, out MenuScreenView menuView))
            {
                ApplyToMenu(menuView, catalog, sprites, true);
                MarkSceneDirty(scene, menuView);
                return true;
            }

            if (TryFindSceneComponent(scene, out MapScreenView mapView))
            {
                ApplyToMap(mapView, catalog, sprites);
                MarkSceneDirty(scene, mapView);
                return true;
            }

            if (TryFindSceneComponent(scene, out BootScreenView bootView))
            {
                ApplyToBoot(bootView, catalog, sprites);
                MarkSceneDirty(scene, bootView);
                return true;
            }

            return false;
        }

        private static bool TryFindSceneComponent<T>(Scene scene, out T component) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return true;
                }
            }

            component = null;
            return false;
        }

        private static void MarkSceneDirty(Scene scene, Component component)
        {
            if (component != null)
            {
                EditorUtility.SetDirty(component);
                EditorUtility.SetDirty(component.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
