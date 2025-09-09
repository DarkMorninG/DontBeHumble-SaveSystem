using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vault;
using Object = UnityEngine.Object;

namespace DBH.SaveSystem {
    public class ResourceLoader {
        private const string MappingFileName = "mappingFile";
        private static List<string> ids;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void UpdateResources() {
            var load = Resources.Load<TextAsset>(MappingFileName);
            ids = load.text
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(s => s.IsNotEmpty())
                .ToList();
        }


        public static T LoadAssetWithId<T>(string id) where T : Object {
            return Resources.Load<T>(RemoveFileEnding(id));
        }

        public static List<T> LoadAllWithId<T>(List<string> guidList) where T : Object {
            return guidList
                .Select(RemoveFileEnding)
                .Select(Resources.Load<T>)
                .ToList();
        }

        public static IEnumerable<T> LoadAll<T>() where T : Object {
            return ids
                .Select(RemoveFileEnding)
                .Select(Resources.Load<T>)
                .Where(o => o != null);
        }

        public static string Id(ScriptableObject scriptableObject) {
            var foundIds = ids.Where(s => s.EndsWith(scriptableObject.name + ".asset"))
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