using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace JellyQuestUnity.EditorTests
{
    public sealed class JellyQuestSaveServiceTests
    {
        [Test]
        public void LoadOrCreate_CreatesDefaultProfileForMissingSave()
        {
            var savePath = CreateTempSavePath();

            try
            {
                var service = new SaveService(savePath);
                service.LoadOrCreate();

                Assert.That(File.Exists(service.SavePath), Is.True);
                Assert.That(service.Profile, Is.Not.Null);
                Assert.That(service.Profile.highestUnlockedLevel, Is.EqualTo(1));
                Assert.That(service.Profile.coins, Is.EqualTo(0));
                Assert.That(service.Profile.starsByLevel, Has.Length.EqualTo(50));
            }
            finally
            {
                DeleteTempSavePath(savePath);
            }
        }

        [Test]
        public void LoadOrCreate_ClampsInvalidValuesFromExistingSave()
        {
            var savePath = CreateTempSavePath();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                File.WriteAllText(savePath, "{\"highestUnlockedLevel\":999,\"coins\":-25,\"boosters\":{\"single\":-1,\"column\":-2,\"row\":-3,\"cross\":-4,\"area\":-5}}");

                var service = new SaveService(savePath);
                service.LoadOrCreate();

                Assert.That(service.Profile.highestUnlockedLevel, Is.EqualTo(50));
                Assert.That(service.Profile.coins, Is.EqualTo(0));
                Assert.That(service.GetBoosterCount(BoosterType.Single), Is.EqualTo(0));
                Assert.That(service.GetBoosterCount(BoosterType.Column), Is.EqualTo(0));
                Assert.That(service.GetBoosterCount(BoosterType.Row), Is.EqualTo(0));
                Assert.That(service.GetBoosterCount(BoosterType.Cross), Is.EqualTo(0));
                Assert.That(service.GetBoosterCount(BoosterType.Area), Is.EqualTo(0));
            }
            finally
            {
                DeleteTempSavePath(savePath);
            }
        }

        [Test]
        public void SetHighestUnlockedLevel_PersistsWithinBounds()
        {
            var savePath = CreateTempSavePath();

            try
            {
                var service = new SaveService(savePath);
                service.LoadOrCreate();
                service.SetHighestUnlockedLevel(999);

                var reloadedService = new SaveService(savePath);
                reloadedService.LoadOrCreate();

                Assert.That(reloadedService.Profile.highestUnlockedLevel, Is.EqualTo(50));
            }
            finally
            {
                DeleteTempSavePath(savePath);
            }
        }

        [Test]
        public void CoinsMutators_PersistAndDoNotGoNegative()
        {
            var savePath = CreateTempSavePath();

            try
            {
                var service = new SaveService(savePath);
                service.LoadOrCreate();
                service.SetCoins(12);
                service.AddCoins(5);

                var reloadedService = new SaveService(savePath);
                reloadedService.LoadOrCreate();

                Assert.That(reloadedService.Profile.coins, Is.EqualTo(17));

                reloadedService.AddCoins(-40);
                reloadedService.SetCoins(-3);

                var clampedService = new SaveService(savePath);
                clampedService.LoadOrCreate();

                Assert.That(clampedService.Profile.coins, Is.EqualTo(0));
            }
            finally
            {
                DeleteTempSavePath(savePath);
            }
        }

        [Test]
        public void CoinsChanged_RaisesOnLoadOrCreateAndCoinMutations()
        {
            var savePath = CreateTempSavePath();

            try
            {
                var service = new SaveService(savePath);
                var observedCoins = new List<int>();
                service.CoinsChanged += observedCoins.Add;

                service.LoadOrCreate();
                service.SetCoins(12);
                service.AddCoins(5);

                Assert.That(service.TrySpendCoins(7), Is.True);
                Assert.That(observedCoins, Is.EqualTo(new[] { 0, 12, 17, 10 }));
            }
            finally
            {
                DeleteTempSavePath(savePath);
            }
        }

        [Test]
        public void CoinsChanged_DoesNotRaiseWhenCoinValueDoesNotChangeOrSpendFails()
        {
            var savePath = CreateTempSavePath();

            try
            {
                var service = new SaveService(savePath);
                service.LoadOrCreate();

                var observedCoins = new List<int>();
                service.CoinsChanged += observedCoins.Add;

                service.SetCoins(0);
                service.AddCoins(0);
                service.SetCoins(-3);

                Assert.That(service.TrySpendCoins(1), Is.False);
                Assert.That(observedCoins, Is.Empty);
            }
            finally
            {
                DeleteTempSavePath(savePath);
            }
        }

        [Test]
        public void BoosterMutators_PersistForEveryBoosterType()
        {
            var savePath = CreateTempSavePath();

            try
            {
                var service = new SaveService(savePath);
                service.LoadOrCreate();

                foreach (BoosterType boosterType in Enum.GetValues(typeof(BoosterType)))
                {
                    service.SetBoosterCount(boosterType, 2);
                    service.AddBooster(boosterType, 3);
                }

                var reloadedService = new SaveService(savePath);
                reloadedService.LoadOrCreate();

                foreach (BoosterType boosterType in Enum.GetValues(typeof(BoosterType)))
                {
                    Assert.That(reloadedService.GetBoosterCount(boosterType), Is.EqualTo(5), boosterType.ToString());
                    reloadedService.AddBooster(boosterType, -10);
                }

                var clampedService = new SaveService(savePath);
                clampedService.LoadOrCreate();

                foreach (BoosterType boosterType in Enum.GetValues(typeof(BoosterType)))
                {
                    Assert.That(clampedService.GetBoosterCount(boosterType), Is.EqualTo(0), boosterType.ToString());
                }
            }
            finally
            {
                DeleteTempSavePath(savePath);
            }
        }

        private static string CreateTempSavePath()
        {
            return Path.Combine(Path.GetTempPath(), "JellyQuestSaveTests", Guid.NewGuid().ToString("N"), "save.json");
        }

        private static void DeleteTempSavePath(string savePath)
        {
            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
