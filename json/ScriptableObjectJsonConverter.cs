using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace DBH.SaveSystem.json {
    public class ScriptableObjectJsonConverter : JsonConverter<ScriptableObject> {
        public override async void WriteJson(JsonWriter writer, ScriptableObject value, JsonSerializer serializer) {
            try {
                var assetGuidByObject = await GetAssetGuidByObject(value);
                await writer.WriteValueAsync(assetGuidByObject);
            }
            catch (System.Exception e) {
                Debug.LogException(e);
            }
        }

        public override ScriptableObject ReadJson(JsonReader reader,
            Type objectType,
            ScriptableObject existingValue,
            bool hasExistingValue,
            JsonSerializer serializer) {
            var guid = (string)reader.Value;
            return LoadedAsset(guid);
        }


        private static async Task<string> GetAssetGuidByObject(ScriptableObject scriptableObject) {
            await Awaitable.MainThreadAsync();
            var id = ResourceLoader.Id(scriptableObject);
            await Awaitable.BackgroundThreadAsync();
            return id;
        }

        private static ScriptableObject LoadedAsset(string guid) {
            return ResourceLoader.LoadAssetWithPath<ScriptableObject>(guid);
        }
    }
}