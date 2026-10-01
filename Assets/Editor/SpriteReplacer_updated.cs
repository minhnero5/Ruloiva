using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;

public class SpriteReplacer : EditorWindow
{
    private List<Sprite> oldSprites = new List<Sprite>();
    private List<Sprite> newSprites = new List<Sprite>();
    private DefaultAsset targetFolder;
    private Vector2 scroll;
    private bool includeBuildScenes = true; // ✅ Tự động lấy scene trong Build Settings
    private bool sortDroppedSpritesByName = true; // ✅ Tùy chọn sort để match ổn định

    [MenuItem("Tools/Multi Sprite Replacer (Auto Build Scenes)")]
    public static void ShowWindow()
    {
        GetWindow<SpriteReplacer>("Sprite Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("🔁 Multi Sprite Replacer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        includeBuildScenes = EditorGUILayout.Toggle("📜 Replace in Build Settings Scenes", includeBuildScenes);
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("📁 Target Folder (optional)", targetFolder, typeof(DefaultAsset), false);
        sortDroppedSpritesByName = EditorGUILayout.Toggle("🔤 Sort dropped sprites by name", sortDroppedSpritesByName);

        EditorGUILayout.Space(10);
        GUILayout.Label("🖼 Sprite Pairs (Old → New)", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Kéo OLD sprites vào ô 1 và NEW sprites vào ô 2. Tool sẽ tự động ghép theo index (row 0 → row 0, ...).\n" +
            "Tip: bật 'Sort dropped sprites by name' nếu bạn muốn order ổn định theo tên sprite.",
            MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        var oldDrop = GUILayoutUtility.GetRect(0, 55, GUILayout.ExpandWidth(true));
        GUI.Box(oldDrop, "Drop OLD sprites here", EditorStyles.helpBox);
        HandleSpriteDragAndDrop(oldDrop, DropTarget.Old);

        var newDrop = GUILayoutUtility.GetRect(0, 55, GUILayout.ExpandWidth(true));
        GUI.Box(newDrop, "Drop NEW sprites here", EditorStyles.helpBox);
        HandleSpriteDragAndDrop(newDrop, DropTarget.New);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = newSprites.Count > 1;
        if (GUILayout.Button("🔀 Shuffle NEW sprites", GUILayout.Height(22)))
        {
            Undo.RecordObject(this, "Shuffle NEW sprites");
            ShuffleList(newSprites);
            Repaint();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(250));
        int rowCount = Mathf.Max(oldSprites.Count, newSprites.Count);
        EnsureListSize(oldSprites, rowCount);
        EnsureListSize(newSprites, rowCount);

        for (int i = 0; i < rowCount; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{i}", GUILayout.Width(22));

            DrawSpritePreview(oldSprites[i]);
            oldSprites[i] = (Sprite)EditorGUILayout.ObjectField(oldSprites[i], typeof(Sprite), false, GUILayout.Width(120));

            GUILayout.Label("→", GUILayout.Width(20));

            DrawSpritePreview(newSprites[i]);
            newSprites[i] = (Sprite)EditorGUILayout.ObjectField(newSprites[i], typeof(Sprite), false, GUILayout.Width(120));

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                oldSprites.RemoveAt(i);
                newSprites.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndScrollView();

        if ((oldSprites.Count > 0 || newSprites.Count > 0) && GUILayout.Button("🗑 Clear Sprite List"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Clear all sprite pairs?", "Yes", "No"))
            {
                oldSprites.Clear();
                newSprites.Clear();
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("🚀 Replace Sprites Now"))
        {
            if (!ValidateInputs()) return;

            int total = 0;

            if (includeBuildScenes)
                total += ReplaceInBuildScenes();

            if (targetFolder != null)
                total += ReplaceInFolder();

            EditorUtility.DisplayDialog("✅ Done", $"Replaced {total} sprite reference(s).", "OK");
        }
    }

    #region --- Drag & UI ---

    private enum DropTarget { Old, New }

    private void HandleSpriteDragAndDrop(Rect dropArea, DropTarget target)
    {
        Event evt = Event.current;
        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                var sprites = ExtractSpritesFromDraggedObjects(DragAndDrop.objectReferences, sortDroppedSpritesByName);

                if (sprites.Count > 0)
                {
                    if (target == DropTarget.Old)
                        AddSpritesToOldList(sprites);
                    else
                        AddSpritesToNewList(sprites);

                    // Keep both lists aligned by index
                    int max = Mathf.Max(oldSprites.Count, newSprites.Count);
                    EnsureListSize(oldSprites, max);
                    EnsureListSize(newSprites, max);
                }

                evt.Use();
            }
        }
    }

    private static List<Sprite> ExtractSpritesFromDraggedObjects(UnityEngine.Object[] draggedObjects, bool sortByName)
    {
        var result = new List<Sprite>();

        foreach (UnityEngine.Object draggedObj in draggedObjects)
        {
            if (draggedObj is Sprite sprite)
            {
                result.Add(sprite);
                continue;
            }

            if (draggedObj is Texture2D texture)
            {
                string path = AssetDatabase.GetAssetPath(texture);
                UnityEngine.Object[] subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                foreach (var s in subs)
                {
                    if (s is Sprite sp)
                        result.Add(sp);
                }
            }

            // Allow dragging folders too
            if (draggedObj is DefaultAsset)
            {
                string folderPath = AssetDatabase.GetAssetPath(draggedObj);
                if (AssetDatabase.IsValidFolder(folderPath))
                {
                    string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
                    foreach (var guid in guids)
                    {
                        string spritePath = AssetDatabase.GUIDToAssetPath(guid);
                        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        if (sp != null) result.Add(sp);
                    }
                }
            }
        }

        // Clean duplicates but preserve order
        var seen = new HashSet<string>();
        var unique = new List<Sprite>();
        foreach (var sp in result)
        {
            if (sp == null) continue;
            string key = AssetDatabase.GetAssetPath(sp) + "::" + sp.name;
            if (seen.Add(key)) unique.Add(sp);
        }

        if (sortByName)
            unique.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        return unique;
    }

    private void AddSpritesToOldList(List<Sprite> sprites)
    {
        foreach (var sp in sprites)
        {
            if (sp == null) continue;
            if (oldSprites.Contains(sp)) continue;

            // Fill empty slots first (when user dropped NEW first)
            int emptyIndex = oldSprites.FindIndex(x => x == null);
            if (emptyIndex >= 0)
                oldSprites[emptyIndex] = sp;
            else
                oldSprites.Add(sp);

            if (newSprites.Count < oldSprites.Count)
                newSprites.Add(null);
        }
    }

    private void AddSpritesToNewList(List<Sprite> sprites)
    {
        foreach (var sp in sprites)
        {
            if (sp == null) continue;
            if (newSprites.Contains(sp)) continue;

            // Fill empty slots first (most common: OLD list already filled, NEW are null)
            int emptyIndex = newSprites.FindIndex(x => x == null);
            if (emptyIndex >= 0)
                newSprites[emptyIndex] = sp;
            else
                newSprites.Add(sp);

            if (oldSprites.Count < newSprites.Count)
                oldSprites.Add(null);
        }
    }

    private static void EnsureListSize(List<Sprite> list, int size)
    {
        while (list.Count < size) list.Add(null);
        if (list.Count > size) list.RemoveRange(size, list.Count - size);
    }

    private void DrawSpritePreview(Sprite sprite)
    {
        if (sprite != null)
        {
            Texture2D tex = AssetPreview.GetAssetPreview(sprite) ?? AssetPreview.GetMiniThumbnail(sprite);
            GUILayout.Box(tex, GUILayout.Width(40), GUILayout.Height(40));
        }
        else
            GUILayout.Box("", GUILayout.Width(40), GUILayout.Height(40));
    }

    private static void ShuffleList<T>(List<T> list)
    {
        if (list == null || list.Count <= 1) return;

        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    #endregion

    #region --- Validation ---

    private bool ValidateInputs()
    {
        int max = Mathf.Max(oldSprites.Count, newSprites.Count);
        EnsureListSize(oldSprites, max);
        EnsureListSize(newSprites, max);

        if (max == 0)
        {
            EditorUtility.DisplayDialog("Error", "No sprite pairs to replace.", "OK");
            return false;
        }

        for (int i = 0; i < max; i++)
        {
            if (oldSprites[i] == null || newSprites[i] == null)
            {
                EditorUtility.DisplayDialog("Error", $"Sprite pair at row {i + 1} is incomplete.", "OK");
                return false;
            }
        }
        return true;
    }

    #endregion

    #region --- Replace Logic ---

    private int ReplaceInFolder()
    {
        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });
        int replaceCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset == null) continue;

                bool modified = false;
                SerializedObject so = new SerializedObject(asset);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        for (int i = 0; i < oldSprites.Count && i < newSprites.Count; i++)
                        {
                            if (prop.objectReferenceValue == oldSprites[i])
                            {
                                prop.objectReferenceValue = newSprites[i];
                                modified = true;
                                replaceCount++;
                                break;
                            }
                        }
                    }
                }

                if (modified)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(asset);
                }
            }
        }

        AssetDatabase.SaveAssets();
        return replaceCount;
    }

    private int ReplaceInBuildScenes()
    {
        int total = 0;
        var buildScenes = EditorBuildSettings.scenes;

        foreach (var s in buildScenes)
        {
            if (!s.enabled) continue;
            var path = s.path;
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int replaced = ReplaceInSceneObjects();
            total += replaced;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"✅ Scene '{Path.GetFileNameWithoutExtension(path)}': replaced {replaced} sprite(s)");
        }

        return total;
    }

    private int ReplaceInSceneObjects()
    {
        int replaced = 0;
        foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>(true))
        {
            // SpriteRenderer
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                for (int i = 0; i < oldSprites.Count && i < newSprites.Count; i++)
                    if (sr.sprite == oldSprites[i])
                    {
                        sr.sprite = newSprites[i];
                        replaced++;
                        EditorUtility.SetDirty(sr);
                    }
            }

            // UI.Image
            var img = obj.GetComponent<Image>();
            if (img != null)
            {
                for (int i = 0; i < oldSprites.Count && i < newSprites.Count; i++)
                    if (img.sprite == oldSprites[i])
                    {
                        img.sprite = newSprites[i];
                        replaced++;
                        EditorUtility.SetDirty(img);
                    }
            }

            // ✅ Sprite trong script custom
            var comps = obj.GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                SerializedObject so = new SerializedObject(c);
                SerializedProperty prop = so.GetIterator();
                bool modified = false;

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        for (int i = 0; i < oldSprites.Count && i < newSprites.Count; i++)
                        {
                            if (prop.objectReferenceValue == oldSprites[i])
                            {
                                prop.objectReferenceValue = newSprites[i];
                                modified = true;
                                replaced++;
                                break;
                            }
                        }
                    }
                }

                if (modified)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(c);
                }
            }
        }
        return replaced;
    }

    #endregion
}