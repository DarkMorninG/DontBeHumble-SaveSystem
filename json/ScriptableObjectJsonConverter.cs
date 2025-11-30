using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DBH.SaveSystem.json {
    public class ScriptableObjectJsonConverter : JsonConverter<ScriptableObject> {
        public override void WriteJson(JsonWriter writer, ScriptableObject value, JsonSerializer serializer) {
            writer.WriteValue(GetAssetGuidByObject(value));
        }

        public override ScriptableObject ReadJson(JsonReader reader,
            Type objectType,
            ScriptableObject existingValue,
            bool hasExistingValue,
            JsonSerializer serializer) {
            var guid = (string)reader.Value;
            return LoadedAsset(guid);
        }


        private static string GetAssetGuidByObject(ScriptableObject scriptableObject) {
            return ResourceLoader.Id(scriptableObject);
        }

        private static ScriptableObject LoadedAsset(string guid) {
            return ResourceLoader.LoadAssetWithPath<ScriptableObject>(guid);
        }
    }
}