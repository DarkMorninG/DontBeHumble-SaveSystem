using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class SObjectPropertySave {
        [FormerlySerializedAs("guid")]
        [SerializeField]
        private SaveAbleScriptableObject objectRef;

        [SerializeField]
        public List<SceneProperty> sceneProperties = new List<SceneProperty>();

        public SaveAbleScriptableObject ObjectRef => objectRef;

        public SObjectPropertySave(SaveAbleScriptableObject objectRef) {
            this.objectRef = objectRef;
        }

        public void AddSceneProperties(string propertyName, object value) {
            sceneProperties.Add(new SceneProperty(propertyName, value));
        }

        [Serializable]
        public class SceneProperty {
            [SerializeField]
            public string PropertyName;

            [SerializeField]
            public object value;

            public SceneProperty(string propertyName, object value) {
                PropertyName = propertyName;
                this.value = value;
            }
        }
    }
}