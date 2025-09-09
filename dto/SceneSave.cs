using System;
using System.Collections.Generic;
using UnityEngine;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class SceneSave {
        [SerializeField]
        private string scenename;
        [SerializeField]
        private List<ComponentPropertySave> componentProperties = new();
        [SerializeField]
        private List<GameObjectPositionSave> gameObjectPositionSaves = new();

        public SceneSave(string scenename) {
            this.scenename = scenename;
        }

        public string Scenename => scenename;

        public List<ComponentPropertySave> ComponentProperties {
            get => componentProperties;
            set => componentProperties = value;
        }

        public List<GameObjectPositionSave> GameObjectPositionSaves {
            get => gameObjectPositionSaves;
            set => gameObjectPositionSaves = value;
        }
    }
}