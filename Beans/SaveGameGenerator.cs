using System.Collections.Generic;
using System.Linq;
using DBH.Attributes;
using DBH.Injection;
using DBH.SaveSystem.Attributes;
using DBH.SaveSystem.dto;
using UnityEngine;
using Vault;

namespace DBH.SaveSystem {
    [Bean]
    public class SaveGameGenerator {
        public void Update(SaveGame saveGame, string stateName, List<string> phases, string currentScene) {
            var sceneSave = saveGame.SceneSaves.Find(save => save.Scenename.Equals(currentScene));
            if (sceneSave != null) {
                sceneSave.GameObjectPositionSaves = GetAllGameObjectPositionSaves();
                sceneSave.ComponentProperties = GetAllComponentSaveAbleProperties();
            } else {
                var save = new SceneSave(currentScene) {
                    GameObjectPositionSaves = GetAllGameObjectPositionSaves(),
                    ComponentProperties = GetAllComponentSaveAbleProperties()
                };
                saveGame.SceneSaves.Add(save);
            }

            saveGame.SceneName = currentScene;
            saveGame.ScriptableObjectSaves = GetAllScriptableSaveAbleProperties();

            saveGame.StateName = stateName;
            saveGame.Phases = phases;
        }

        public SaveGame Create(int order, string stateName, string sceneName) {
            return new SaveGame(order, stateName, sceneName);
        }

        private List<GameObjectPositionSave> GetAllGameObjectPositionSaves() {
            var allSavablePosition = GameObject.FindGameObjectsWithTag("SaveAblePosition");

            return allSavablePosition
                .Select(gameObject => new GameObjectPositionSave(gameObject.transform.position, gameObject.name))
                .ToList();
        }

        private List<ComponentPropertySave> GetAllComponentSaveAbleProperties() {
            var allGameObjects = Object.FindObjectsOfType<GameObject>();
            var saveAbles = allGameObjects.SelectMany(o => o.GetComponentsInChildren<ISaveable>()).Distinct();
            var componentPropertySaves = new List<ComponentPropertySave>();
            foreach (var saveAble in saveAbles) {
                var fieldsWithAttribute = Injector.GetFieldsWithAttribute<PlayerSaved>(saveAble, true);
                if (fieldsWithAttribute.IsEmpty()) continue;

                var componentPropertySave = new ComponentPropertySave(saveAble.Identifier);
                foreach (var fieldInfo in fieldsWithAttribute) {
                    var playerSaveAble = Injector.GetAttribute<PlayerSaved>(fieldInfo);
                    componentPropertySave.AddSceneProperties(playerSaveAble.Name, fieldInfo.GetValue(saveAble));
                }

                componentPropertySaves.Add(componentPropertySave);
            }

            return componentPropertySaves;
        }


        private List<SObjectPropertySave> GetAllScriptableSaveAbleProperties() {
            var foundSavableObjects = ResourceLoader.LoadAll<SaveAbleScriptableObject>();

            var componentPropertySaves = new List<SObjectPropertySave>();
            foreach (var scriptableObject in foundSavableObjects) {
                var fieldsWithAttribute = Injector.GetFieldsWithAttribute<PlayerSaved>(scriptableObject);
                if (fieldsWithAttribute.IsEmpty()) continue;

                var componentPropertySave = new SObjectPropertySave(scriptableObject.Identifier, scriptableObject);
                componentPropertySave.Type = scriptableObject.GetType();
                foreach (var fieldInfo in fieldsWithAttribute) {
                    var value = fieldInfo.GetValue(scriptableObject);
                    if (IsDefault(value)) continue;
                    var playerSaveAble = Injector.GetAttribute<PlayerSaved>(fieldInfo);
                    componentPropertySave.AddSceneProperties(playerSaveAble.Name,
                        value);
                }

                if (!componentPropertySave.sceneProperties.IsEmpty()) {
                    componentPropertySaves.Add(componentPropertySave);
                }
            }

            return componentPropertySaves;
        }


        private bool IsDefault(object toTest) {
            return toTest switch {
                string => toTest.Equals(""),
                bool => toTest.Equals(false),
                int => toTest.Equals(0),
                _ => false
            };
        }
    }
}