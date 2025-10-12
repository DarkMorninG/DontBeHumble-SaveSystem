using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DBH.Attributes;
using DBH.Injection;
using DBH.SaveSystem.Attributes;
using DBH.SaveSystem.dto;
using DBH.SaveSystem.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.UnityConverters;
using Sirenix.Utilities;
using UnityEngine;
using Vault;
using Object = UnityEngine.Object;

namespace DBH.SaveSystem.Beans {
    [Bean]
    public class SaveGameLoader {
        public void LoadSaveGame(SaveGame saveGame) {
            if (saveGame.ActiveSceneSave() == null) return;
            var allGameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var savables = GetComponentFromGameObjects<ISaveable>(allGameObjects);
            UpdateProperties(saveGame, savables);
            UpdateScriptableObjects(saveGame.ScriptableObjectSaves);
            var saveListener = GetComponentFromGameObjects<ISavingListener>(allGameObjects);
            savables.ForEach(saveAble => saveAble.AfterSaveGameLoad());
            saveListener.ForEach(listener => listener.AfterSaveGameLoad());

            var saveAblePositionGameobjects = GameObject.FindGameObjectsWithTag("SaveAblePosition");
            UpdatePositions(saveGame.ActiveSceneSave(), saveAblePositionGameobjects.ToList());
        }


        private void UpdateProperties(SaveGame saveGame, List<ISaveable> saveAbles) {
            var sceneSave = saveGame.ActiveSceneSave();
            foreach (var saveAble in saveAbles) {
                if (!sceneSave.ComponentProperties.Select(save => save.Identifier)
                    .Contains(saveAble.Identifier)) {
                    continue;
                }

                var fieldsWithAttribute = Injector.GetFieldsWithAttribute<PlayerSaved>(saveAble);
                if (fieldsWithAttribute.IsEmpty()) continue;
                var componentPropertySave =
                    sceneSave.ComponentProperties.Find(save => save.Identifier == saveAble.Identifier);
                foreach (var sceneProperty in componentPropertySave.SceneProperties) {
                    foreach (var fieldInfo in from fieldInfo in fieldsWithAttribute
                        let playerSaved = Injector.GetAttribute<PlayerSaved>(fieldInfo)
                        where playerSaved.Name.Equals(sceneProperty.PropertyName)
                        select fieldInfo) {
                        if (IsScriptablObject(fieldInfo)) {
                            if (sceneProperty.value is string value) {
                                UpdateWithScriptableObject(value, fieldInfo, saveAble);
                            } else {
                                fieldInfo.SetValue(saveAble, sceneProperty.value);
                            }
                        } else if (sceneProperty.value is JArray jArray) {
                            if (IsScriptableList(fieldInfo)) {
                                UpdateListWithScriptableObjects(jArray, fieldInfo, saveAble);
                            } else {
                                UpdateListWithRaw(jArray, fieldInfo, saveAble);
                            }
                        } else {
                            if (sceneProperty.value is JObject toJobject) {
                                var jsonSerializer = CreateDefaultJsonSerializer();
                                var convertedToFieldType = toJobject.ToObject(fieldInfo.FieldType, jsonSerializer);
                                fieldInfo.SetValue(saveAble, convertedToFieldType);
                            } else {
                                fieldInfo.SetValue(saveAble, sceneProperty.value);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateScriptableObjects(List<SObjectPropertySave> sObjectPropertySaves) {
            foreach (var sObjectPropertySave in sObjectPropertySaves) {
                var loadedAsset = LoadedAsset(sObjectPropertySave);
                var sceneProperties = sObjectPropertySave.sceneProperties;
                var fieldsWithAttribute = Injector.GetFieldsWithAttribute<PlayerSaved>(loadedAsset);
                foreach (var fieldInfo in fieldsWithAttribute) {
                    var playerSaved = Injector.GetAttribute<PlayerSaved>(fieldInfo);
                    var sceneProperty =
                        sceneProperties.Find(property => property.PropertyName.Equals(playerSaved.Name));
                    if (sceneProperty == null) continue;
                    if (IsScriptablObject(fieldInfo)) {
                        if (sceneProperty.value is string value) {
                            UpdateWithScriptableObject(value, fieldInfo, loadedAsset);
                        } else {
                            fieldInfo.SetValue(loadedAsset, sceneProperty.value);
                        }
                    } else if (sceneProperty.value is JArray jArray) {
                        if (IsScriptableList(fieldInfo)) {
                            UpdateListWithScriptableObjects(jArray, fieldInfo, loadedAsset);
                        } else {
                            UpdateListWithRaw(jArray, fieldInfo, loadedAsset);
                        }
                    } else {
                        if (sceneProperty.value is JObject jObject) {
                            var jsonSerializer = CreateDefaultJsonSerializer();
                            var convertedToFieldType = jObject.ToObject(fieldInfo.FieldType, jsonSerializer);
                            fieldInfo.SetValue(loadedAsset, convertedToFieldType);
                        } else {
                            fieldInfo.SetValue(loadedAsset,
                                sceneProperty.value is double
                                    ? Convert.ToSingle(sceneProperty.value)
                                    : sceneProperty.value);
                        }
                    }
                }
            }
        }

        private static bool IsScriptablObject(FieldInfo fieldInfo) {
            return fieldInfo.FieldType.GetBaseTypes().Contains(typeof(ScriptableObject));
        }

        private static SaveAbleScriptableObject LoadedAsset(SObjectPropertySave sObjectPropertySave) {
            return sObjectPropertySave.ObjectRef;
        }

        private static void UpdateWithScriptableObject(string guid,
            FieldInfo fieldInfo,
            object toUpdate) {
            var childLoadedAsset = ResourceLoader.LoadAssetWithId<Object>(guid);
            fieldInfo.SetValue(toUpdate, childLoadedAsset);
        }

        private static JsonSerializer CreateDefaultJsonSerializer() {
            var jsonSerializerSettings = new JsonSerializerSettings {
                Converters = { new ScriptableObjectJsonConverter(), new DecimalJsonConverter() },
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new FixedUnityTypeContractResolver()
            };
            return JsonSerializer.CreateDefault(jsonSerializerSettings);
        }

        private static void UpdateListWithRaw(JArray jArray, FieldInfo fieldInfo, object toUpdate) {
            var collection = fieldInfo.GetValue(toUpdate) as IList;
            collection.Clear();
            var list = jArray.ToList();
            var jsonSerializer = CreateDefaultJsonSerializer();
            foreach (var loadedSObject in list) {
                collection.Add(loadedSObject.ToObject(fieldInfo.FieldType.GetGenericArguments()[0], jsonSerializer));
            }
        }

        private static void UpdateListWithScriptableObjects(JArray jArray,
            FieldInfo fieldInfo,
            object toUpdate) {
            var guidList = jArray.ToList().ConvertAll(input => (string)input);
            var loadedSObjects = ResourceLoader.LoadAllWithId<ScriptableObject>(guidList);
            var collection = fieldInfo.GetValue(toUpdate) as IList;
            collection.Clear();
            foreach (var loadedSObject in loadedSObjects) {
                collection.Add(loadedSObject);
            }
        }

        private static void UpdatePositions(SceneSave sceneSave, List<GameObject> saveAblePositionGameobjects) {
            foreach (var saveGameGameObjectPositionSave in sceneSave.GameObjectPositionSaves) {
                var transformToBeChanged = saveAblePositionGameobjects.Find(o =>
                    o.name.Equals(saveGameGameObjectPositionSave.GameObjectName));
                if (transformToBeChanged == null) continue;
                transformToBeChanged.transform.position = saveGameGameObjectPositionSave.Position;
            }
        }

        private List<T> GetComponentFromGameObjects<T>(IEnumerable<GameObject> gameObjectsInScene) {
            var components = new List<T>();
            foreach (var rootGameObject in gameObjectsInScene) {
                foreach (var component in rootGameObject.GetComponents<T>()) {
                    components.AddNotNull(component);
                }
            }

            return components;
        }

        private bool IsScriptableList(FieldInfo fieldInfo) {
            var isList = fieldInfo.FieldType.IsGenericType &&
                         fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>);
            return isList &&
                   fieldInfo.FieldType.GetGenericArguments()[0]
                       .GetBaseTypes()
                       .Contains(typeof(ScriptableObject));
        }
    }
}