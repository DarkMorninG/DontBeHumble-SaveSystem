using System;
using System.Collections.Generic;
using UnityEngine;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class ComponentPropertySave {
        [SerializeField]
        private string identifier;
        [SerializeField]
        private List<SceneProperty> sceneProperties = new List<SceneProperty>();

        public string Identifier {
            get => identifier;
            set => identifier = value;
        }

        public List<SceneProperty> SceneProperties {
            get => sceneProperties;
            set => sceneProperties = value;
        }

        public ComponentPropertySave(string identifier) {
            this.identifier = identifier;
        }

        public void AddSceneProperties(string propertyName, object value) {
            sceneProperties.Add(new SceneProperty(propertyName, value));
        }

        [Serializable]
        public class SceneProperty {
            public readonly string PropertyName;
            public readonly object value;

            public SceneProperty(string propertyName, object value) {
                PropertyName = propertyName;
                this.value = value;
            }
        }
    }
}