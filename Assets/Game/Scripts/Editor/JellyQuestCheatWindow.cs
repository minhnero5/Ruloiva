using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JellyQuestUnity.Editor
{
    public sealed class JellyQuestCheatWindow : EditorWindow
    {
        private const float MinimumWidth = 520f;
        private const float MinimumHeight = 420f;
        private const int FallbackLevelCount = 50;

        private readonly Dictionary<BoosterType, int> _boosterInputs = new Dictionary<BoosterType, int>();

        private SaveService _editorSave;
        private int _maxLevel = FallbackLevelCount;
        private int _unlockLevelInput = 1;
        private int _coinsInput = 100;

        [MenuItem("Tools/Jelly Quest/Cheat Window")]
        public static void Open()
        {
            var window = GetWindow<JellyQuestCheatWindow>("Cheat Window");
            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        private SaveService ActiveSave
        {
            get
            {
                if (EditorApplication.isPlaying && JellyQuestApp.Instance != null && JellyQuestApp.Instance.Save != null)
                {
                    return JellyQuestApp.Instance.Save;
                }

                return _editorSave;
            }
        }

        private void OnEnable()
        {
            minSize = new Vector2(MinimumWidth, MinimumHeight);
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            LoadCatalogMetadata();
            ReloadSave();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void OnGUI()
        {
            EnsureSaveLoaded();
            DrawToolbar();

            var save = ActiveSave;
            if (save == null || save.Profile == null)
            {
                EditorGUILayout.HelpBox("Unable to load the Jelly Quest save file.", MessageType.Error);
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(
                IsUsingRuntimeSave()
                    ? "Play Mode: editing the live runtime save and refreshing the active screen."
                    : "Edit Mode: editing the persisted save file on disk.",
                MessageType.Info);

            DrawProgressSection(save);
            DrawCoinsSection(save);
            DrawBoostersSection(save);
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var savePath = ActiveSave != null ? ActiveSave.SavePath : "(missing)";
                GUILayout.Label("Save: " + savePath, EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Reload Save", EditorStyles.toolbarButton))
                {
                    ReloadSave();
                }

                if (GUILayout.Button("Reveal Save", EditorStyles.toolbarButton))
                {
                    RevealSave();
                }
            }
        }

        private void DrawProgressSection(SaveService save)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Current unlocked level", save.Profile.highestUnlockedLevel.ToString());

                _unlockLevelInput = Mathf.Clamp(EditorGUILayout.IntField("Target level", _unlockLevelInput), 1, _maxLevel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Set Unlock Level"))
                    {
                        ApplySaveMutation(service => service.SetHighestUnlockedLevel(_unlockLevelInput));
                    }

                    if (GUILayout.Button("Unlock All"))
                    {
                        ApplySaveMutation(service => service.SetHighestUnlockedLevel(_maxLevel));
                    }
                }
            }
        }

        private void DrawCoinsSection(SaveService save)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Coins", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Current coins", save.Profile.coins.ToString());

                _coinsInput = EditorGUILayout.IntField("Amount", _coinsInput);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add Coins"))
                    {
                        ApplySaveMutation(service => service.AddCoins(_coinsInput));
                    }

                    if (GUILayout.Button("Set Coins"))
                    {
                        ApplySaveMutation(service => service.SetCoins(_coinsInput));
                    }
                }
            }
        }

        private void DrawBoostersSection(SaveService save)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Boosters", EditorStyles.boldLabel);

                foreach (BoosterType boosterType in Enum.GetValues(typeof(BoosterType)))
                {
                    DrawBoosterRow(save, boosterType);
                }
            }
        }

        private void DrawBoosterRow(SaveService save, BoosterType boosterType)
        {
            if (!_boosterInputs.ContainsKey(boosterType))
            {
                _boosterInputs[boosterType] = 1;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(boosterType.ToString(), GUILayout.Width(90f));
                EditorGUILayout.LabelField("Current: " + save.GetBoosterCount(boosterType), GUILayout.Width(90f));
                _boosterInputs[boosterType] = EditorGUILayout.IntField(_boosterInputs[boosterType]);

                if (GUILayout.Button("Add", GUILayout.Width(70f)))
                {
                    var amount = _boosterInputs[boosterType];
                    ApplySaveMutation(service => service.AddBooster(boosterType, amount));
                }

                if (GUILayout.Button("Set", GUILayout.Width(70f)))
                {
                    var amount = _boosterInputs[boosterType];
                    ApplySaveMutation(service => service.SetBoosterCount(boosterType, amount));
                }
            }
        }

        private void ApplySaveMutation(Action<SaveService> mutation)
        {
            var save = ActiveSave;
            if (save == null)
            {
                return;
            }

            mutation(save);

            if (IsUsingRuntimeSave())
            {
                JellyQuestApp.Instance.RefreshActiveScreenFromSave();
            }
            else
            {
                ReloadStandaloneSave(false);
            }

            Repaint();
        }

        private void ReloadSave(bool resetInputs = true)
        {
            LoadCatalogMetadata();

            if (IsUsingRuntimeSave())
            {
                JellyQuestApp.Instance.Save.LoadOrCreate();
                JellyQuestApp.Instance.RefreshActiveScreenFromSave();
                if (resetInputs)
                {
                    SyncInputsFromSave(JellyQuestApp.Instance.Save);
                }

                Repaint();
                return;
            }

            ReloadStandaloneSave(resetInputs);
        }

        private void ReloadStandaloneSave(bool resetInputs = true)
        {
            _editorSave = new SaveService();
            _editorSave.LoadOrCreate();
            if (resetInputs)
            {
                SyncInputsFromSave(_editorSave);
            }

            Repaint();
        }

        private void RevealSave()
        {
            EnsureSaveLoaded();
            var savePath = ActiveSave != null ? ActiveSave.SavePath : null;
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            if (File.Exists(savePath))
            {
                EditorUtility.RevealInFinder(savePath);
                return;
            }

            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory))
            {
                EditorUtility.RevealInFinder(directory);
            }
        }

        private void EnsureSaveLoaded()
        {
            if (ActiveSave == null)
            {
                ReloadSave();
            }
        }

        private void SyncInputsFromSave(SaveService save)
        {
            if (save == null || save.Profile == null)
            {
                return;
            }

            _unlockLevelInput = Mathf.Clamp(save.Profile.highestUnlockedLevel, 1, _maxLevel);
            _coinsInput = save.Profile.coins;

            foreach (BoosterType boosterType in Enum.GetValues(typeof(BoosterType)))
            {
                _boosterInputs[boosterType] = save.GetBoosterCount(boosterType);
            }
        }

        private void LoadCatalogMetadata()
        {
            try
            {
                var catalogService = new JellyQuestCatalogService();
                catalogService.Load();
                if (catalogService.Levels != null && catalogService.Levels.Length > 0)
                {
                    _maxLevel = catalogService.Levels.Length;
                    return;
                }
            }
            catch (Exception)
            {
            }

            _maxLevel = ActiveSave != null && ActiveSave.Profile != null && ActiveSave.Profile.starsByLevel != null && ActiveSave.Profile.starsByLevel.Length > 0
                ? ActiveSave.Profile.starsByLevel.Length
                : FallbackLevelCount;
        }

        private bool IsUsingRuntimeSave()
        {
            return EditorApplication.isPlaying && JellyQuestApp.Instance != null && JellyQuestApp.Instance.Save != null;
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    ReloadSave();
                    break;
            }
        }
    }
}
