using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ResourceMapper;
using UnityEngine;
using UnityEngine.Windows;
using Vault;
using File = System.IO.File;
using Object = UnityEngine.Object;

namespace DBH.SaveSystem {
    public class ResourceLoader {
        private const string MappingFileName = "mappingFile";
        private static Dictionary<string, List<ResourceDto>> resourceDtos;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void UpdateResources() {
            resourceDtos = Resources.Load<TextAsset>(MappingFileName)
                .text
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(s => s.IsNotEmpty())
                .ToDictionary(s => s.Split(";")[0], s => JsonConvert.DeserializeObject<List<ResourceDto>>(s.Split(";")[1]));
        }


        public static T LoadAssetWithId<T>(string id) where T : Object {
            return Resources.Load<T>(RemoveFileEnding(resourceDtos[id].OrderBy(dto => dto.Count).First().Path));
        }

        public static List<T> LoadAllWithId<T>(List<string> guidList) where T : Object {
            return guidList
                .Select(s => resourceDtos[s].OrderBy(dto => dto.Count).First().Path)
                .Select(RemoveFileEnding)
                .Select(Resources.Load<T>)
                .ToList();
        }

        public static IEnumerable<T> LoadAll<T>() where T : Object {
            return resourceDtos
                .Select(pair => pair.Value.OrderBy(dto => dto.Count).First())
                .Select(dto => dto.Path)
                .Select(RemoveFileEnding)
                .Select(Resources.Load<T>)
                .Where(o => o != null);
        }

        public static string Id(ScriptableObject scriptableObject) {
            var foundIds = resourceDtos
                .Where(pair => pair.Value.OrderBy(dto => dto.Count).First().Path.EndsWith(scriptableObject.name + ".asset"))
                .Select(pair => pair.Key)
                .ToList();
            if (foundIds.Count == 1) {
                return foundIds[0];
            }

            foreach (var s in foundIds) {
                var loadAssetWithId = LoadAssetWithId<ScriptableObject>(s);
                if (loadAssetWithId.GetType() == scriptableObject.GetType()) {
                    return s;
                }
            }

            return null;
        }

        private static string RemoveFileEnding(string original) {
            return original.Substring(0, original.LastIndexOf(".", StringComparison.Ordinal));
        }
    }
}