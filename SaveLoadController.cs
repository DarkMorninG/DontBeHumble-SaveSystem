using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetterCoroutine.AwaitRuntime;
using DBH.Attributes;
using DBH.Base;
using DBH.SaveSystem.Beans;
using DBH.SaveSystem.dto;
using DBH.SaveSystem.writer;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vault;
using Vault.BetterCoroutine;

namespace DBH.SaveSystem {
    [Controller]
    public class SaveLoadController : DBHMono {
        [Grab]
        private GameplayTimer _gameplayTimer;

        [Grab]
        private SaveGameGenerator _saveGameGenerator;

        [Grab]
        private SaveGameLoader _saveGameLoader;

        [Grab]
        private SaveGameWriterReader _saveGameWriterReader;

        [SerializeField]
        private bool enableAutoSaving = true;

        [SerializeField]
        private float autoSavingInterval = 10;

        [SerializeField]
        private SaveGame currentSaveGame;

        [ShowInInspector]
        private readonly List<SaveGame> saveGames = new();

        private Func<string> stateReference = () => "";
        private Func<List<string>> phaseReference = () => new List<string>();
        public List<SaveGame> SaveGames => saveGames;

        public bool HasSaveGameLoaded => currentSaveGame != null;

        public delegate void BeforeSaveGameUpdate();

        public delegate void SaveGamesUpdated(List<SaveGame> saveGames);

        public event BeforeSaveGameUpdate OnBeforeSaveGameUpdate;

        public event SaveGamesUpdated OnSaveGameUpdated;
        private IAwaitRuntime autoSaving;

        [ContextMenu("Load Save Games")]
        private void Start() {
            UpdateSaveGameList();
        }

        [ContextMenu("Create SaveFile")]
        public async Task<SaveGame> CreateNewSaveFile() {
            var highestOrder = saveGames.Select(game => game.Order)
                .OrderBy(i => i)
                .LastOptional()
                .OrElse(0);
            var saveGame = _saveGameGenerator.Create(highestOrder + 1, stateReference(), SceneManager.GetActiveScene().name);
            var version = Application.version;
            await Awaitable.BackgroundThreadAsync();
            var writeSaveGame = await _saveGameWriterReader.WriteSaveGame(saveGame, version);
            saveGames.ReplaceOrAdd(writeSaveGame);
            currentSaveGame = saveGame;
            return saveGame;
        }

        [ContextMenu("Update SaveGame")]
        public async void UpdateSaveFile() {
            // currentSaveGame.SaveGameIcon = _screenShotter.LastScreenshot;
            OnBeforeSaveGameUpdate?.Invoke();
            currentSaveGame.LastModified = DateTime.Now;
            currentSaveGame.PlayTime += _gameplayTimer.SecondsPlayedSinceStart();
            _saveGameGenerator.Update(currentSaveGame,
                stateReference(),
                phaseReference(),
                SceneManager.GetActiveScene().name);
            var version = Application.version;
            await Awaitable.BackgroundThreadAsync();
            var writeSaveGame = await _saveGameWriterReader.WriteSaveGame(currentSaveGame, version);
            saveGames.ReplaceOrAdd(writeSaveGame);
        }

        [ContextMenu("Update SaveGame")]
        public async Task UpdateSaveFile(string sceneName) {
            // currentSaveGame.SaveGameIcon = _screenShotter.LastScreenshot;
            OnBeforeSaveGameUpdate?.Invoke();
            currentSaveGame.LastModified = DateTime.Now;
            currentSaveGame.PlayTime += _gameplayTimer.SecondsPlayedSinceStart();
            _saveGameGenerator.Update(currentSaveGame,
                stateReference(),
                phaseReference(),
                sceneName);
            var version = Application.version;
            await Awaitable.BackgroundThreadAsync();
            var writeSaveGame = await _saveGameWriterReader.WriteSaveGame(currentSaveGame, version);
            saveGames.ReplaceOrAdd(writeSaveGame);
        }

        public void StateReference(Func<string> stateReference) {
            this.stateReference = stateReference;
        }

        public void PhaseReference(Func<List<string>> phaseReference) {
            this.phaseReference = phaseReference;
        }

        public void LoadSaveGame(SaveGame saveGame) {
            _gameplayTimer.StartGameplayTimer();
            currentSaveGame = saveGame;
            _saveGameLoader.LoadSaveGame(currentSaveGame);
            if (enableAutoSaving) {
                autoSaving = IAwaitRuntime.EverySecondsDo(() => {
                        if (currentSaveGame != null) {
                            UpdateSaveFile();
                        }
                    },
                    () => autoSavingInterval);
            }
        }

        private void OnDisable() {
            autoSaving?.Stop();
        }

        [ContextMenu("Create And Update")]
        public async Task CreateSaveGame() {
            saveGames.ReplaceOrAdd(await CreateNewSaveFile());
        }

        public void OverWriteSaveGame(int saveGameOrder) {
            currentSaveGame.Order = saveGameOrder;
            UpdateSaveFile();
        }


        [ContextMenu("Load first SaveGame")]
        private void LoadFirstSavegame() {
            currentSaveGame = saveGames[0];
            LoadSaveGame(currentSaveGame);
        }

        [ContextMenu("Load current SaveGame")]
        private void LoadCurrentSaveGame() {
            LoadSaveGame(currentSaveGame);
        }

        [ContextMenu("Load last SaveGame From Disk")]
        private void LoadLastSaveGameFromDisk() {
            var saveGame = _saveGameWriterReader.LoadAllSaveGames().Last();
            LoadSaveGame(saveGame);
        }

        private void UpdateSaveGameList() {
            saveGames.Clear();
            saveGames.AddRange(_saveGameWriterReader.LoadAllSaveGames());
            OnSaveGameUpdated?.Invoke(saveGames);
        }
    }
}