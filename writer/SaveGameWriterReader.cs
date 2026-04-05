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
using Newtonsoft.Json.Linq;
using UnityEngine;
using Vault;

namespace DBH.SaveSystem.writer {
    [Bean]
    public class SaveGameWriterReader {
        private readonly List<IVersionUpdate> versionUpdates;
        private static readonly string saveGamePath = Application.persistentDataPath + "/" + "saves";

        public SaveGameWriterReader(List<IVersionUpdate> versionUpdates) {
            this.versionUpdates = versionUpdates;
        }

        public async Task<SaveGame> WriteSaveGame(SaveGame saveGame) {
            var fixedUnityTypeContractResolver = new FixedUnityTypeContractResolver();
            var jsonSerializerSettings = new JsonSerializerSettings {
                Converters = { new ScriptableObjectJsonConverter(), new DecimalJsonConverter() },
                ContractResolver = fixedUnityTypeContractResolver,
                TypeNameHandling = TypeNameHandling.All
            };
            saveGame.VersionSaved = Application.version;
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

            return encryptedSaveFiles
                .Select(File.ReadAllText)
                .Select(RunVersionChanges)
                .ToList();
        }

        private SaveGame RunVersionChanges(string saveGameRaw) {
            var jsonSerializerSettings = new JsonSerializerSettings {
                Converters = { new ScriptableObjectJsonConverter(), new DecimalJsonConverter() },
                ContractResolver = new FixedUnityTypeContractResolver()
            };
            var versionSaved = ExtractVersionSaved(saveGameRaw);
            var updates = versionUpdates.OrderBy(update => update.VersionToUpdate)
                .Where(update => update.VersionToUpdate > new SemVer(versionSaved))
                .ToList();

            var oldRaw = saveGameRaw;
            foreach (var versionUpdate in updates) {
                var jObject = JObject.Parse(oldRaw);
                var customTypeObjects = ExtractCustomTypeObjects(jObject);
                var builder = new SaveGameUpdateBuilder(customTypeObjects);
                versionUpdate.UpdateCustomTypes(builder);
                oldRaw = jObject.ToString(Formatting.None);
            }

            return JsonConvert.DeserializeObject<SaveGame>(oldRaw, jsonSerializerSettings);
        }

        private static List<JObject> ExtractCustomTypeObjects(JObject root) {
            var result = new List<JObject>();
            CollectCustomTypeObjects(root, result);
            return result;
        }

        private static void CollectCustomTypeObjects(JToken token, List<JObject> result) {
            if (token is JObject obj) {
                var typeName = obj["$type"]?.ToString();
                if (typeName != null && !IsSystemType(typeName)) {
                    result.Add(obj);
                }

                foreach (var property in obj.Properties()) {
                    CollectCustomTypeObjects(property.Value, result);
                }
            } else if (token is JArray array) {
                foreach (var item in array) {
                    CollectCustomTypeObjects(item, result);
                }
            }
        }

        private static bool IsSystemType(string assemblyQualifiedType) {
            return assemblyQualifiedType.StartsWith("System.") ||
                   assemblyQualifiedType.StartsWith("UnityEngine.") ||
                   assemblyQualifiedType.StartsWith("UnityEditor.") ||
                   assemblyQualifiedType.StartsWith("DBH.");
        }

        private string ExtractVersionSaved(string saveGame) {
            try {
                var jsonObject = JObject.Parse(saveGame);
                return jsonObject["versionSaved"]?.ToString();
            }
            catch {
                return null;
            }
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