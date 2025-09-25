using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class SObjectPropertySave {
        [SerializeField]
        private string identifier;

        [FormerlySerializedAs("guid")]
        [SerializeField]
        private SaveAbleScriptableObject objectRef;

        [SerializeField]
        private Type type;

        [SerializeField]
        public List<SceneProperty> sceneProperties = new List<SceneProperty>();

        public Type Type {
            get => type;
            set => type = value;
        }

        public string Identifier {
            get => identifier;
            set => identifier = value;
        }

        public SaveAbleScriptableObject ObjectRef {
            get => objectRef;
            set => objectRef = value;
        }

        public SObjectPropertySave(string identifier, SaveAbleScriptableObject objectRef) {
            this.identifier = identifier;
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