using System;
using System.IO;
using System.Linq;
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
            CreateResourceMap();
        }

        public void OnPreprocessBuild(BuildReport report) {
            CreateResourceMap();
        }

        private static void CreateResourceMap() {
            var allFileNames = Directory
                .EnumerateFiles(Application.dataPath + "/Resources/", "*.*", SearchOption.AllDirectories)
                .Where(s => !s.EndsWith(".meta"))
                .Select(s => s.Substring(s.IndexOf("/Resources/", StringComparison.Ordinal) + "/Resources/".Length))
                .ToList();
            var mappingFileName = Application.dataPath + "/Resources/" + MappingFileName;
            if (File.Exists(mappingFileName)) {
                File.Delete(mappingFileName);
            }
            File.WriteAllLines(mappingFileName, allFileNames);
        }
    }
}