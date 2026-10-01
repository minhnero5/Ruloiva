using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
        public sealed class SaveService
        {
            private const int DefaultLevelCount = 50;

            private readonly string _savePath;

            public SaveProfile Profile { get; private set; }

            public string SavePath => _savePath;
            public event Action<int> CoinsChanged;

            public SaveService(string savePath = null)
            {
                _savePath = string.IsNullOrEmpty(savePath)
                    ? Path.Combine(Application.persistentDataPath, "jelly_quest_save.json")
                    : savePath;
            }

            public void LoadOrCreate()
            {
                var previousCoins = Profile != null ? (int?)Profile.coins : null;
                Profile = null;
                if (File.Exists(_savePath))
                {
                    try
                    {
                        Profile = JsonUtility.FromJson<SaveProfile>(File.ReadAllText(_savePath));
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning("Failed to read Jelly Quest save: " + exception.Message);
                    }
                }

                if (Profile == null)
                {
                    Profile = new SaveProfile();
                }

                if (Profile.starsByLevel == null || Profile.starsByLevel.Length != DefaultLevelCount)
                {
                    Profile.starsByLevel = ResizeStars(Profile.starsByLevel, DefaultLevelCount);
                }

                if (Profile.boosters == null)
                {
                    Profile.boosters = new BoosterInventory();
                }

                NormalizeProfile();
                Save();
                NotifyCoinsChangedIfNeeded(previousCoins);
            }

            public void Save()
            {
                var directory = Path.GetDirectoryName(_savePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_savePath, JsonUtility.ToJson(Profile, true));
            }

            public int GetStars(int levelNumber)
            {
                var index = Mathf.Clamp(levelNumber - 1, 0, Profile.starsByLevel.Length - 1);
                return Profile.starsByLevel[index];
            }

            public void SetStars(int levelNumber, int stars)
            {
                var index = Mathf.Clamp(levelNumber - 1, 0, Profile.starsByLevel.Length - 1);
                Profile.starsByLevel[index] = Mathf.Max(Profile.starsByLevel[index], stars);
                Save();
            }

            public void UnlockNextLevel(int completedLevelNumber)
            {
                SetHighestUnlockedLevel(Mathf.Max(Profile.highestUnlockedLevel, completedLevelNumber + 1));
            }

            public void SetHighestUnlockedLevel(int level)
            {
                EnsureProfileInitialized();
                Profile.highestUnlockedLevel = Mathf.Clamp(level, 1, GetMaxLevel());
                Save();
            }

            public int GetBoosterCount(BoosterType type)
            {
                return Profile.boosters.GetCount(type);
            }

            public bool TryConsumeBooster(BoosterType type)
            {
                var success = Profile.boosters.TryConsume(type);
                if (success)
                {
                    Save();
                }

                return success;
            }

            public void AddBooster(BoosterType type, int amount)
            {
                EnsureProfileInitialized();
                Profile.boosters.Add(type, amount);
                Save();
            }

            public void SetBoosterCount(BoosterType type, int amount)
            {
                EnsureProfileInitialized();
                Profile.boosters.SetCount(type, amount);
                Save();
            }

            public bool TrySpendCoins(int amount)
            {
                EnsureProfileInitialized();
                var previousCoins = Profile.coins;
                if (Profile.coins < amount)
                {
                    return false;
                }

                Profile.coins -= amount;
                Save();
                NotifyCoinsChangedIfNeeded(previousCoins);
                return true;
            }

            public void AddCoins(int amount)
            {
                EnsureProfileInitialized();
                var previousCoins = Profile.coins;
                Profile.coins = Mathf.Max(0, Profile.coins + amount);
                Save();
                NotifyCoinsChangedIfNeeded(previousCoins);
            }

            public void SetCoins(int amount)
            {
                EnsureProfileInitialized();
                var previousCoins = Profile.coins;
                Profile.coins = Mathf.Max(0, amount);
                Save();
                NotifyCoinsChangedIfNeeded(previousCoins);
            }

            public void SetMusicEnabled(bool enabled)
            {
                Profile.musicEnabled = enabled;
                Save();
            }

            private void EnsureProfileInitialized()
            {
                if (Profile == null)
                {
                    Profile = new SaveProfile();
                }

                if (Profile.starsByLevel == null || Profile.starsByLevel.Length != DefaultLevelCount)
                {
                    Profile.starsByLevel = ResizeStars(Profile.starsByLevel, DefaultLevelCount);
                }

                if (Profile.boosters == null)
                {
                    Profile.boosters = new BoosterInventory();
                }

                NormalizeProfile();
            }

            private void NormalizeProfile()
            {
                Profile.highestUnlockedLevel = Mathf.Clamp(Profile.highestUnlockedLevel <= 0 ? 1 : Profile.highestUnlockedLevel, 1, GetMaxLevel());
                Profile.coins = Mathf.Max(0, Profile.coins);
                Profile.boosters.ClampToNonNegative();
            }

            private int GetMaxLevel()
            {
                return Profile != null && Profile.starsByLevel != null && Profile.starsByLevel.Length > 0
                    ? Profile.starsByLevel.Length
                    : DefaultLevelCount;
            }

            private void NotifyCoinsChangedIfNeeded(int? previousCoins)
            {
                if (!previousCoins.HasValue || previousCoins.Value != Profile.coins)
                {
                    CoinsChanged?.Invoke(Profile.coins);
                }
            }

            private static int[] ResizeStars(int[] existingStars, int targetLength)
            {
                var resizedStars = new int[targetLength];
                if (existingStars == null)
                {
                    return resizedStars;
                }

                Array.Copy(existingStars, resizedStars, Mathf.Min(existingStars.Length, resizedStars.Length));
                return resizedStars;
            }
        }
}
