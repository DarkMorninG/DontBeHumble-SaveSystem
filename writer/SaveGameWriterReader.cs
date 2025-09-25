using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBH.Attributes;
using DBH.SaveSystem.dto;
using DBH.SaveSystem.json;
using Newtonsoft.Json;
using UnityEngine;
using Vault;

namespace DBH.SaveSystem.writer {
    [Bean]
    public class SaveGameWriterReader {
        private static readonly string saveGamePath = Application.persistentDataPath + "/" + "saves";

        public async Task<SaveGame> WriteSaveGame(SaveGame saveGame) {
            var fixedUnityTypeContractResolver = new FixedUnityTypeContractResolver();
            var jsonSerializerSettings = new JsonSerializerSettings {
                Converters = { new ScriptableObjectJsonConverter(), new DecimalJsonConverter() },
                ContractResolver = fixedUnityTypeContractResolver,
                TypeNameHandling = TypeNameHandling.All
            };
            var saveGameString = JsonConvert.SerializeObject(saveGame, Formatting.Indented, jsonSerializerSettings);
            await File.WriteAllTextAsync(SaveGameFile(saveGame.Order), saveGameString);
            return saveGame;
        }

        public List<SaveGame> LoadAllSaveGames() {
            if (!Directory.Exists(saveGamePath)) {
                Directory.CreateDirectory(saveGamePath);
            }

            var encryptedSaveFiles = Directory.GetFiles(saveGamePath);
            if (encryptedSaveFiles.IsNullOrEmpty()) return new List<SaveGame>();

            var jsonSerializerSettings = new JsonSerializerSettings {
                Converters = { new ScriptableObjectJsonConverter(), new DecimalJsonConverter() },
                ContractResolver = new FixedUnityTypeContractResolver()
            };
            return encryptedSaveFiles
                .Select(File.ReadAllText)
                .Select(s => JsonConvert.DeserializeObject<SaveGame>(s, jsonSerializerSettings))
                .ToList();
        }

        private static string SaveGameFile(int order) {
            return saveGamePath + "/" + order + ".save";
        }

        private static string SaveGameBackUpFile(int order) {
            return saveGamePath + "/" + order + ".backup";
        }

        public string Base64Encode(string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}