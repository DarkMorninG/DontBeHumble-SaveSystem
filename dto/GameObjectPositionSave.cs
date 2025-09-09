using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class GameObjectPositionSave {
        [SerializeField]
        private Vector3 position;
        [FormerlySerializedAs("_gameObjectName")]
        [SerializeField]
        private string gameObjectName;

        public GameObjectPositionSave(Vector3 position, string gameObjectName) {
            this.position = position;
            this.gameObjectName = gameObjectName;
        }

        public Vector3 Position {
            get => position;
            set => position = value;
        }

        public string GameObjectName {
            get => gameObjectName;
            set => gameObjectName = value;
        }
    }
}