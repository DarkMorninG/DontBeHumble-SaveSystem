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
            var resourceMap = CreateOrLoadResourceMap();
            var movedAssetsChanged = UpdateMovedAssets(resourceMap, movedAssets, movedFromAssetPaths);
            var importedAssetsChanged = AddImportedAssets(resourceMap, importedAssets);

            ValidateCurrentPaths(resourceMap);

            if (movedAssetsChanged || importedAssetsChanged) WriteResourceMap(resourceMap);
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

        private static string RemoveResourcesPath(string path) {
            const string resourcesSegment = "/Resources/";

            var normalizedPath = path.Replace('\\', '/');
            var resourcesIndex = normalizedPath.IndexOf(
                resourcesSegment,
                StringComparison.OrdinalIgnoreCase);

            if (resourcesIndex < 0)
                throw new ArgumentException($"Path is not inside a Resources folder: {path}");

            return normalizedPath.Substring(resourcesIndex + resourcesSegment.Length);
        }

        private static bool UpdateMovedAssets(Dictionary<string, List<ResourceDto>> resourceMap,
            string[] movedAssets, string[] movedFromAssetPaths) {
            var mapChanged = false;

            for (var i = 0; i < movedAssets.Length; i++) {
                var oldPath = RemoveResourcesPath(movedFromAssetPaths[i]);
                var newPath = RemoveResourcesPath(movedAssets[i]);
                mapChanged |= UpdateMovedAsset(resourceMap, oldPath, newPath);
            }

            return mapChanged;
        }

        private static bool UpdateMovedAsset(Dictionary<string, List<ResourceDto>> resourceMap,
            string oldPath, string newPath) {
            var matchingEntries = resourceMap
                .Where(pair => IsCurrentPath(pair, oldPath))
                .ToList();

            if (matchingEntries.Count > 1) {
                Debug.LogError($"Resource path '{oldPath}' belongs to multiple mapping IDs. " +
                               "The move cannot be applied unambiguously.");
                return false;
            }

            if (matchingEntries.Count == 0) return false;

            var entry = matchingEntries[0];
            var current = Current(entry.Value);
            if (PathsEqual(current.Path, newPath)) return false;

            if (IsCurrentPathMapped(resourceMap, newPath, entry.Key)) {
                Debug.LogError($"Cannot update resource mapping '{entry.Key}' from '{oldPath}' to " +
                               $"'{newPath}': the destination is already mapped.");
                return false;
            }

            entry.Value.Add(current.CreateNewVersion(newPath));
            return true;
        }

        private static bool AddImportedAssets(Dictionary<string, List<ResourceDto>> resourceMap,
            IEnumerable<string> importedAssets) {
            var mapChanged = false;
            var importedResourcePaths = importedAssets
                .Where(s => !s.EndsWith(MappingFileName, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
                .Where(s => (File.GetAttributes(s) & FileAttributes.Directory) != FileAttributes.Directory)
                .Select(RemoveResourcesPath)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var path in importedResourcePaths) {
                if (IsCurrentPathMapped(resourceMap, path)) continue;

                resourceMap.Add(Guid.NewGuid().ToString(), new List<ResourceDto> { new ResourceDto(path) });
                mapChanged = true;
            }

            return mapChanged;
        }

        private static bool IsCurrentPathMapped(Dictionary<string, List<ResourceDto>> resourceMap,
            string path, string excludedId = null) {
            return resourceMap.Any(pair =>
                pair.Key != excludedId &&
                IsCurrentPath(pair, path));
        }

        private static bool IsCurrentPath(KeyValuePair<string, List<ResourceDto>> entry, string path) {
            return entry.Value.Count > 0 && PathsEqual(Current(entry.Value).Path, path);
        }

        private static bool PathsEqual(string left, string right) {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static ResourceDto Current(IEnumerable<ResourceDto> versions) {
            return versions.OrderBy(dto => dto.Count).Last();
        }

        private static void WriteResourceMap(Dictionary<string, List<ResourceDto>> resourceMap) {
            File.WriteAllLines(Application.dataPath + "/Resources/" + MappingFileName,
                resourceMap.Select(pair => pair.Key + ";" + JsonConvert.SerializeObject(pair.Value)));
        }

        private static void ValidateCurrentPaths(Dictionary<string, List<ResourceDto>> resourceMap) {
            var duplicatePaths = resourceMap
                .Where(pair => pair.Value.Count > 0)
                .GroupBy(pair => Current(pair.Value).Path, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1);

            foreach (var duplicate in duplicatePaths) {
                Debug.LogError($"Resource path '{duplicate.Key}' belongs to multiple mapping IDs: " +
                               string.Join(", ", duplicate.Select(pair => pair.Key)));
            }
        }
    }
}
