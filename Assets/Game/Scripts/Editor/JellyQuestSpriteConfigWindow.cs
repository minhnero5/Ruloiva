using UnityEditor;
using UnityEngine;
using SpriteArrayDefinition = JellyQuestUnity.Editor.JellyQuestSpriteConfigLayout.SpriteArrayDefinition;
using SpriteFieldDefinition = JellyQuestUnity.Editor.JellyQuestSpriteConfigLayout.SpriteFieldDefinition;
using TabDefinition = JellyQuestUnity.Editor.JellyQuestSpriteConfigLayout.TabDefinition;

namespace JellyQuestUnity.Editor
{
    public sealed class JellyQuestSpriteConfigWindow : EditorWindow
    {
        private const float ThumbnailSize = 56f;
        private const float PreviewColumnWidth = 136f;
        private const float ArrayItemWidth = 184f;
        private const int GridColumns = 4;

        private static readonly TabDefinition[] Tabs = JellyQuestSpriteConfigLayout.VisibleTabs;

        private LegacyUiSpriteCatalog _catalog;
        private SerializedObject _serializedCatalog;
        private Vector2 _scrollPosition;
        private int _selectedTabIndex;
        private GUIStyle _metadataLabelStyle;

        [MenuItem("Tools/Jelly Quest/Sprite Config")]
        public static void Open()
        {
            var window = GetWindow<JellyQuestSpriteConfigWindow>("Sprite Config");
            window.minSize = new Vector2(760f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            EnsureCatalogLoaded();
        }

        private void OnGUI()
        {
            EnsureCatalogLoaded();
            DrawToolbar();

            if (_catalog == null || _serializedCatalog == null)
            {
                EditorGUILayout.HelpBox("Unable to load the LegacyUiSpriteCatalog asset.", MessageType.Error);
                return;
            }

            _serializedCatalog.Update();
            _selectedTabIndex = GUILayout.Toolbar(Mathf.Clamp(_selectedTabIndex, 0, Tabs.Length - 1), GetTabNames());
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawTab(Tabs[Mathf.Clamp(_selectedTabIndex, 0, Tabs.Length - 1)]);
            EditorGUILayout.EndScrollView();

            if (_serializedCatalog.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_catalog);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Legacy UI Sprite Catalog", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_catalog == null))
                {
                    if (GUILayout.Button("Select Catalog Asset", EditorStyles.toolbarButton))
                    {
                        JellyQuestSpriteCatalogEditorUtility.SelectCatalogAsset(_catalog);
                    }

                    if (GUILayout.Button("Refresh Build Scenes", EditorStyles.toolbarButton))
                    {
                        SaveCatalogChanges();
                        JellyQuestSpriteScenePreview.RefreshBuildScenes(_catalog);
                    }
                }
            }
        }

        private void DrawTab(TabDefinition tab)
        {
            EditorGUILayout.Space(8f);
            foreach (var field in tab.Fields)
            {
                DrawField(field);
            }

            for (var index = 0; index < tab.ArrayGroups.Length; index++)
            {
                EditorGUILayout.Space(10f);
                DrawSpriteArray(tab.ArrayGroups[index]);
            }
        }

        private void DrawField(SpriteFieldDefinition definition)
        {
            var property = _serializedCatalog.FindProperty(definition.PropertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox("Missing property: " + definition.PropertyName, MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(280f)))
                {
                    EditorGUILayout.LabelField(definition.Label, EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(property, GUIContent.none);
                }

                DrawFieldPreview(property);
            }
        }

        private void DrawSpriteArray(SpriteArrayDefinition definition)
        {
            var arrayProperty = _serializedCatalog.FindProperty(definition.PropertyName);
            if (arrayProperty == null || !arrayProperty.isArray)
            {
                EditorGUILayout.HelpBox("Missing sprite array: " + definition.PropertyName, MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(definition.Label, EditorStyles.boldLabel);
            var visibleIndices = definition.VisibleIndices;
            var visibleItemCount = visibleIndices != null ? visibleIndices.Length : arrayProperty.arraySize;
            for (var index = 0; index < visibleItemCount; index += GridColumns)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var column = 0; column < GridColumns; column++)
                    {
                        var itemIndex = index + column;
                        if (itemIndex >= visibleItemCount)
                        {
                            GUILayout.FlexibleSpace();
                            continue;
                        }

                        var actualIndex = visibleIndices != null ? visibleIndices[itemIndex] : itemIndex;
                        if (actualIndex < 0 || actualIndex >= arrayProperty.arraySize)
                        {
                            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(ArrayItemWidth)))
                            {
                                EditorGUILayout.LabelField("Missing Element " + actualIndex, EditorStyles.boldLabel);
                            }

                            continue;
                        }

                        var label = itemIndex < definition.Labels.Length ? definition.Labels[itemIndex] : "Element " + actualIndex;
                        DrawSpriteArrayItem(arrayProperty.GetArrayElementAtIndex(actualIndex), label);
                    }
                }
            }
        }

        private void DrawSpriteArrayItem(SerializedProperty property, string label)
        {
            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(ArrayItemWidth)))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(property, GUIContent.none);
                DrawPreview(property.objectReferenceValue as Sprite);
            }
        }

        private void DrawPreview(Sprite sprite)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(PreviewColumnWidth)))
            {
                var previewTexture = GetPreviewTexture(sprite);
                var previewRowRect = GUILayoutUtility.GetRect(PreviewColumnWidth, ThumbnailSize, GUILayout.Width(PreviewColumnWidth), GUILayout.Height(ThumbnailSize));
                var previewRect = new Rect(
                    previewRowRect.x + ((previewRowRect.width - ThumbnailSize) * 0.5f),
                    previewRowRect.y,
                    ThumbnailSize,
                    ThumbnailSize);
                if (previewTexture != null)
                {
                    GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    EditorGUI.DrawRect(previewRect, new Color(0.16f, 0.16f, 0.16f, 1f));
                    GUI.Label(previewRect, "No Sprite", EditorStyles.centeredGreyMiniLabel);
                }

                DrawSpriteMetadata(sprite);

                using (new EditorGUI.DisabledScope(sprite == null))
                {
                    if (GUILayout.Button("Ping"))
                    {
                        EditorGUIUtility.PingObject(sprite);
                    }
                }
            }
        }

        private void DrawFieldPreview(SerializedProperty property)
        {
            if (property != null && property.propertyType == SerializedPropertyType.Color)
            {
                DrawColorPreview(property.colorValue);
                return;
            }

            DrawPreview(property != null ? property.objectReferenceValue as Sprite : null);
        }

        private void DrawColorPreview(Color color)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(PreviewColumnWidth)))
            {
                var previewRowRect = GUILayoutUtility.GetRect(PreviewColumnWidth, ThumbnailSize, GUILayout.Width(PreviewColumnWidth), GUILayout.Height(ThumbnailSize));
                var previewRect = new Rect(
                    previewRowRect.x + ((previewRowRect.width - ThumbnailSize) * 0.5f),
                    previewRowRect.y,
                    ThumbnailSize,
                    ThumbnailSize);
                EditorGUI.DrawRect(previewRect, new Color(0.16f, 0.16f, 0.16f, 1f));
                var swatchRect = new Rect(previewRect.x + 4f, previewRect.y + 4f, previewRect.width - 8f, previewRect.height - 8f);
                EditorGUI.DrawRect(swatchRect, color);
                GUI.Label(GUILayoutUtility.GetRect(PreviewColumnWidth, 16f, GUILayout.Width(PreviewColumnWidth)), "Color", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void DrawSpriteMetadata(Sprite sprite)
        {
            if (!JellyQuestSpritePreviewMetadataUtility.TryCreate(sprite, out var metadata))
            {
                return;
            }

            GUILayout.Label(metadata.OriginalSizeLabel, GetMetadataLabelStyle(), GUILayout.Width(PreviewColumnWidth));
            GUILayout.Label(metadata.RatioLabel, GetMetadataLabelStyle(), GUILayout.Width(PreviewColumnWidth));
        }

        private void EnsureCatalogLoaded()
        {
            if (_catalog != null && _serializedCatalog != null && _serializedCatalog.targetObject == _catalog)
            {
                return;
            }

            _catalog = JellyQuestSpriteCatalogEditorUtility.LoadOrCreateCatalog();
            _serializedCatalog = _catalog != null ? new SerializedObject(_catalog) : null;
        }

        private void SaveCatalogChanges()
        {
            if (_serializedCatalog == null)
            {
                return;
            }

            _serializedCatalog.ApplyModifiedProperties();
            if (_catalog != null)
            {
                EditorUtility.SetDirty(_catalog);
                AssetDatabase.SaveAssets();
            }
        }

        private static Texture GetPreviewTexture(Sprite sprite)
        {
            if (sprite == null)
            {
                return null;
            }

            return AssetPreview.GetAssetPreview(sprite) ?? AssetPreview.GetMiniThumbnail(sprite);
        }

        private GUIStyle GetMetadataLabelStyle()
        {
            if (_metadataLabelStyle == null)
            {
                _metadataLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            return _metadataLabelStyle;
        }

        private static string[] GetTabNames()
        {
            var tabNames = new string[Tabs.Length];
            for (var index = 0; index < Tabs.Length; index++)
            {
                tabNames[index] = Tabs[index].Name;
            }

            return tabNames;
        }

    }
}
