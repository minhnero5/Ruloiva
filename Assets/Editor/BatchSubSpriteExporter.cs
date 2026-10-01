using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BatchSubSpriteExporter : EditorWindow
{
    [MenuItem("Tools/Sprite/Batch Export Sub-Sprites (Preview)")]
    public static void Open() => GetWindow<BatchSubSpriteExporter>("Sub-Sprite Exporter");

    [Serializable]
    private class SpriteEntry
    {
        public Sprite sprite;
        public bool export = true;
    }

    [Serializable]
    private class SourceItem
    {
        public string assetPath;      // texture asset path
        public Texture2D texture;
        public string displayName;
        public bool foldout = true;
        public List<SpriteEntry> entries = new();
    }

    private readonly List<SourceItem> sources = new();
    private Vector2 scroll;

    // ---------- GLOBAL SETTINGS ----------
    private bool overwriteIfExists = false;
    private bool deleteOriginalTextures = false;

    private bool useCustomOutFolder = true;
    private DefaultAsset outFolderObj;

    // Naming: base + index
    private string baseName = "sprite";
    private string separator = "_";
    private int startIndex = 1;
    private int padding = 2;

    // Preview UI
    private int previewColumns = 6;
    private bool showPlannedName = true;

    private const float ThumbSize = 72f;
    private const float Padding = 8f;

    private void OnGUI()
    {
        DrawTopBar();
        EditorGUILayout.Space(8);

        DrawDropArea();
        EditorGUILayout.Space(8);

        DrawGlobalSettings();
        EditorGUILayout.Space(10);

        DrawSourceList();
        EditorGUILayout.Space(10);

        DrawBottomButtons();
    }

    private void DrawTopBar()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Batch Export Sub-Sprites (Preview)", EditorStyles.boldLabel);

            if (GUILayout.Button("Clear", GUILayout.Width(80)))
                sources.Clear();
        }

        EditorGUILayout.HelpBox(
            "Kéo thả nhiều Texture/Sprite vào vùng bên dưới.\n" +
            "Tool sẽ hiển thị sub-sprite để bạn tick chọn, sau đó export ra PNG theo setting GLOBAL.",
            MessageType.Info);
    }

    private void DrawDropArea()
    {
        Rect dropArea = GUILayoutUtility.GetRect(0, 70, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Textures / Sprites Here", new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        });

        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition)) return;

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var obj in DragAndDrop.objectReferences)
                        TryAddSource(obj);
                }
                evt.Use();
                break;
        }
    }

    private void DrawGlobalSettings()
    {
        EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);

        overwriteIfExists = EditorGUILayout.ToggleLeft("Overwrite if exists (tắt = auto unique name)", overwriteIfExists);
        deleteOriginalTextures = EditorGUILayout.ToggleLeft("Delete original textures after export", deleteOriginalTextures);

        EditorGUILayout.Space(6);

        useCustomOutFolder = EditorGUILayout.ToggleLeft("Export to selected folder", useCustomOutFolder);

        using (new EditorGUI.DisabledScope(!useCustomOutFolder))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                outFolderObj = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outFolderObj, typeof(DefaultAsset), false);

                if (GUILayout.Button("Pick Folder...", GUILayout.Width(110)))
                {
                    string picked = PickProjectFolder("Pick Output Folder (under Assets)");
                    if (!string.IsNullOrEmpty(picked))
                        outFolderObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(picked);
                }
            }

            if (outFolderObj != null)
            {
                string p = AssetDatabase.GetAssetPath(outFolderObj);
                if (!AssetDatabase.IsValidFolder(p))
                    EditorGUILayout.HelpBox("Folder không hợp lệ (phải là folder trong Assets).", MessageType.Warning);
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Naming (Global)", EditorStyles.boldLabel);

        baseName = EditorGUILayout.TextField("Base Name", baseName);
        separator = EditorGUILayout.TextField("Separator", string.IsNullOrEmpty(separator) ? "_" : separator);

        using (new EditorGUILayout.HorizontalScope())
        {
            startIndex = EditorGUILayout.IntField("Start Index", startIndex, GUILayout.Width(220));
            padding = EditorGUILayout.IntField("Padding", padding, GUILayout.Width(170));
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        previewColumns = Mathf.Clamp(EditorGUILayout.IntField("Columns", previewColumns), 2, 12);
        showPlannedName = EditorGUILayout.ToggleLeft("Show planned output name under preview", showPlannedName);

        EditorGUILayout.Space(6);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Select All"))
                foreach (var src in sources) foreach (var e in src.entries) e.export = true;

            if (GUILayout.Button("Deselect All"))
                foreach (var src in sources) foreach (var e in src.entries) e.export = false;
        }
    }

    private void DrawSourceList()
    {
        if (sources.Count == 0)
        {
            EditorGUILayout.HelpBox("Chưa có source. Kéo thả texture/sprite vào vùng Drag & Drop.", MessageType.None);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        // planned index preview (global, only for selected)
        int plannedIndex = 0;

        for (int i = 0; i < sources.Count; i++)
        {
            var src = sources[i];

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    src.foldout = EditorGUILayout.Foldout(src.foldout, $"{src.displayName}  ({src.entries.Count} sprites)", true);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        sources.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                if (!src.foldout) continue;

                EditorGUILayout.LabelField("Asset Path:", src.assetPath);
                EditorGUILayout.Space(6);

                DrawSpriteGrid(src, ref plannedIndex);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSpriteGrid(SourceItem src, ref int plannedIndex)
    {
        int cols = Mathf.Max(2, previewColumns);
        float cellW = ThumbSize + Padding;
        float cellH = ThumbSize + (showPlannedName ? 56f : 40f);

        int count = src.entries.Count;
        int rows = Mathf.CeilToInt(count / (float)cols);

        for (int r = 0; r < rows; r++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int c = 0; c < cols; c++)
                {
                    int idx = r * cols + c;
                    if (idx >= count)
                    {
                        GUILayout.FlexibleSpace();
                        continue;
                    }

                    var entry = src.entries[idx];

                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellW), GUILayout.Height(cellH)))
                    {
                        entry.export = EditorGUILayout.Toggle(entry.export, GUILayout.Width(16));

                        Texture2D preview = AssetPreview.GetAssetPreview(entry.sprite);
                        if (preview == null)
                        {
                            preview = AssetPreview.GetMiniThumbnail(entry.sprite) as Texture2D;
                            Repaint();
                        }

                        Rect thumbRect = GUILayoutUtility.GetRect(ThumbSize, ThumbSize, GUILayout.ExpandWidth(false));
                        if (preview != null)
                            GUI.DrawTexture(thumbRect, preview, ScaleMode.ScaleToFit);

                        string name = entry.sprite != null ? entry.sprite.name : "(null)";
                        EditorGUILayout.LabelField(Shorten(name, 18), EditorStyles.miniLabel);

                        if (showPlannedName && entry.sprite != null)
                        {
                            if (entry.export)
                            {
                                string planned = BuildOutputFileName(baseName, plannedIndex);
                                plannedIndex++;
                                EditorGUILayout.LabelField(Shorten(planned + ".png", 22), EditorStyles.miniLabel);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("", EditorStyles.miniLabel);
                            }
                        }
                    }
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Select all in this texture"))
                foreach (var e in src.entries) e.export = true;

            if (GUILayout.Button("Deselect all in this texture"))
                foreach (var e in src.entries) e.export = false;
        }
    }

    private void DrawBottomButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(sources.Count == 0))
            {
                if (GUILayout.Button("Export Selected", GUILayout.Height(32), GUILayout.Width(180)))
                    ExportSelected();
            }
        }
    }

    private void TryAddSource(UnityEngine.Object obj)
    {
        if (obj == null) return;

        string path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) return;

        // If user drops a Sprite, resolve its texture path
        if (obj is Sprite sp)
        {
            string texPath = AssetDatabase.GetAssetPath(sp.texture);
            if (!string.IsNullOrEmpty(texPath)) path = texPath;
        }

        // avoid duplicates
        if (sources.Any(s => s.assetPath == path)) return;

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return;

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        // Ensure readable + sprite type
        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (!importer.isReadable) { importer.isReadable = true; changed = true; }
        if (changed) importer.SaveAndReimport();

        // Load sub-sprites (Multiple) OR single sprite
        var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        if (subs.Length == 0)
        {
            var single = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (single != null) subs = new[] { single };
        }

        var item = new SourceItem
        {
            assetPath = path,
            texture = tex,
            displayName = Path.GetFileName(path),
            foldout = true,
            entries = subs.Select(s => new SpriteEntry { sprite = s, export = true }).ToList()
        };

        sources.Add(item);
    }

    private void ExportSelected()
    {
        string outFolder = GetGlobalOutputFolderOrFallback();
        if (string.IsNullOrEmpty(outFolder) || !AssetDatabase.IsValidFolder(outFolder))
        {
            Debug.LogError("Output folder không hợp lệ. Hãy chọn folder trong Assets (Global Settings).");
            return;
        }

        // build export list (global order)
        var exportList = new List<(SourceItem src, Sprite sprite)>();
        foreach (var src in sources)
        {
            foreach (var e in src.entries)
            {
                if (e.export && e.sprite != null)
                    exportList.Add((src, e.sprite));
            }
        }

        if (exportList.Count == 0)
        {
            Debug.LogWarning("Không có sprite nào được chọn để export.");
            return;
        }

        // We might delete originals, so keep unique texture paths
        var texturesToDelete = new HashSet<string>();

        AssetDatabase.StartAssetEditing();
        try
        {
            int globalIdx = 0;

            // Cache pixels per source texture to avoid repeated GetPixels32()
            var pixelCache = new Dictionary<string, (Color32[] pixels, int width)>();

            foreach (var (src, sprite) in exportList)
            {
                if (src.texture == null) continue;

                if (!pixelCache.TryGetValue(src.assetPath, out var cache))
                {
                    cache = (src.texture.GetPixels32(), src.texture.width);
                    pixelCache[src.assetPath] = cache;
                }

                Rect r = sprite.textureRect;
                var rect = new RectInt(
                    Mathf.RoundToInt(r.x),
                    Mathf.RoundToInt(r.y),
                    Mathf.RoundToInt(r.width),
                    Mathf.RoundToInt(r.height)
                );

                if (rect.width <= 0 || rect.height <= 0) continue;

                string fileName = BuildOutputFileName(baseName, globalIdx);
                string outPath = $"{outFolder}/{fileName}.png";

                if (!overwriteIfExists)
                    outPath = AssetDatabase.GenerateUniqueAssetPath(outPath);

                WriteRectPng(cache.pixels, cache.width, rect, outPath);

                AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
                MakePngSprite(outPath);

                globalIdx++;

                if (deleteOriginalTextures)
                    texturesToDelete.Add(src.assetPath);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        if (deleteOriginalTextures)
        {
            foreach (var p in texturesToDelete)
                AssetDatabase.DeleteAsset(p);

            AssetDatabase.Refresh();
        }

        Debug.Log($"✅ Export done. Exported {exportList.Count} PNG(s) to: {outFolder}");
    }

    // ---------- Global output folder resolver ----------
    private string GetGlobalOutputFolderOrFallback()
    {
        if (useCustomOutFolder && outFolderObj != null)
        {
            string p = AssetDatabase.GetAssetPath(outFolderObj);
            if (AssetDatabase.IsValidFolder(p))
                return p;
        }

        // fallback: use first source folder
        if (sources.Count > 0)
        {
            string srcFolder = Path.GetDirectoryName(sources[0].assetPath)?.Replace("\\", "/") ?? "Assets";
            if (AssetDatabase.IsValidFolder(srcFolder))
                return srcFolder;
        }

        return "Assets";
    }

    // ---------- Naming (Base + global index) ----------
    private string BuildOutputFileName(string baseNameInput, int runningIndex)
    {
        string b = string.IsNullOrWhiteSpace(baseNameInput) ? "sprite" : baseNameInput.Trim();
        b = SanitizeFileName(b);

        string sep = string.IsNullOrEmpty(separator) ? "_" : separator;
        int idx = startIndex + runningIndex;

        string idxStr = idx.ToString().PadLeft(Mathf.Max(0, padding), '0');
        string name = $"{b}{sep}{idxStr}";

        name = SanitizeFileName(name);
        if (string.IsNullOrWhiteSpace(name)) name = "sprite";
        return name;
    }

    // ---------- Folder picker ----------
    private static string PickProjectFolder(string title)
    {
        string abs = EditorUtility.OpenFolderPanel(title, Application.dataPath, "");
        if (string.IsNullOrEmpty(abs)) return null;

        abs = abs.Replace("\\", "/");
        string project = Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/");

        if (!abs.StartsWith(project))
        {
            EditorUtility.DisplayDialog("Invalid Folder",
                "Please select a folder inside this Unity project (preferably under Assets).", "OK");
            return null;
        }

        string rel = abs.Substring(project.Length + 1);
        if (!rel.StartsWith("Assets"))
        {
            EditorUtility.DisplayDialog("Invalid Folder",
                "Please select a folder under Assets/ so Unity can import the exported PNGs.", "OK");
            return null;
        }

        if (!AssetDatabase.IsValidFolder(rel))
        {
            EditorUtility.DisplayDialog("Invalid Folder",
                "That folder doesn't exist as a valid Unity folder. Create it inside Assets first.", "OK");
            return null;
        }

        return rel;
    }

    // ---------- PNG writing ----------
    private static void WriteRectPng(Color32[] srcPixels, int srcW, RectInt rect, string outAssetPath)
    {
        var dst = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
        var outPixels = new Color32[rect.width * rect.height];

        for (int y = 0; y < rect.height; y++)
        {
            for (int x = 0; x < rect.width; x++)
            {
                int sx = rect.x + x;
                int sy = rect.y + y;
                outPixels[x + y * rect.width] = srcPixels[sx + sy * srcW];
            }
        }

        dst.SetPixels32(outPixels);
        dst.Apply(false, false);

        byte[] png = dst.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(dst);

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), outAssetPath);
        File.WriteAllBytes(fullPath, png);
    }

    private static void MakePngSprite(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }

        if (changed) importer.SaveAndReimport();
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }

    private static string Shorten(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
        return s.Substring(0, max - 1) + "…";
    }
}
