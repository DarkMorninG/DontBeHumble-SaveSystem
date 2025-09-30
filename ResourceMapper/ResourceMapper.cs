using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ResourceMapper {
    public class ResourceMapper : AssetPostprocessor, IPreprocessBuildWithReport {
        private const string MappingFileName = "mappingFile.txt";

        public int callbackOrder { get; }

        static void OnPostprocessAllAssets(string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths) {
            var loadResourceMap = CreateOrLoadResourceMap();

            var newAssets = importedAssets.Where(s => !s.EndsWith(MappingFileName))
                .Where(s => s.StartsWith("Assets/Resources/"))
                .Where(s => loadResourceMap.Where(pair => pair.Value.Count == 1)
                    .All(pair => !pair.Value.Select(dto => dto.Path).Contains(RemoveResourcesPath(s))))
                .Select(RemoveResourcesPath)
                .Select(s => new ResourceDto(s))
                .Select(dto => new List<ResourceDto> { dto })
                .Select(JsonConvert.SerializeObject)
                .Select(s => Guid.NewGuid() + ";" + s)
                .ToList();
            File.AppendAllLines(Application.dataPath + "/Resources/" + MappingFileName, newAssets);

            for (var i = 0; i < movedAssets.Length; i++) {
                var updatedResourceMap = loadResourceMap
                    .Where(pair => pair.Value.Select(dto => dto.Path).Contains(RemoveResourcesPath(movedFromAssetPaths[i])))
                    .Select(pair => {
                        var resourceDto = pair.Value.OrderBy(dto => dto.Count).First().CreateNewVersion(RemoveResourcesPath(movedAssets[i]));
                        pair.Value.Add(resourceDto);
                        return pair;
                    })
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var keyValuePair in updatedResourceMap) {
                    loadResourceMap[keyValuePair.Key] = keyValuePair.Value;
                }

                File.WriteAllLines(Application.dataPath + "/Resources/" + MappingFileName,
                    loadResourceMap.Select(pair => pair.Key + ";" + JsonConvert.SerializeObject(pair.Value)));
            }
        }

        public void OnPreprocessBuild(BuildReport report) {
            // CreateOrLoadResourceMap();
        }

        private static Dictionary<string, List<ResourceDto>> CreateOrLoadResourceMap() {
            var mappingFileName = Application.dataPath + "/Resources/" + MappingFileName;
            if (File.Exists(mappingFileName)) {
                return File.ReadAllLines(mappingFileName)
                    .ToDictionary(s => s.Split(";")[0], s => JsonConvert.DeserializeObject<List<ResourceDto>>(s.Split(";")[1]));
            }

            var allFileNames = Directory
                .EnumerateFiles(Application.dataPath + "/Resources/", "*.*", SearchOption.AllDirectories)
                .Where(s => !s.EndsWith(".meta"))
                .Select(RemoveResourcesPath)
                .Select(s => new ResourceDto(s))
                .ToDictionary(dto => Guid.NewGuid().ToString(), dto => new List<ResourceDto> { dto });

            File.WriteAllLines(mappingFileName, allFileNames.Select(pair => pair.Key + ";" + JsonConvert.SerializeObject(pair.Value)));
            return allFileNames;
        }

        private static string RemoveResourcesPath(string s) {
            return s.Substring(s.IndexOf("/Resources/", StringComparison.Ordinal) + "/Resources/".Length);
        }

        private static void UpdateResourceMap(string mappingFileName) {
        }
    }
}