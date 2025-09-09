using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class SaveGame {
        [SerializeField]
        private int order;

        [SerializeField]
        private byte[] saveGameIcon;

        [SerializeField]
        private long playTime;

        [SerializeField]
        private string stateName;

        [SerializeField]
        string sceneName;

        [SerializeField]
        private List<string> phases;


        [FormerlySerializedAs("saveTime")]
        [SerializeField]
        DateTime creationDate;

        [SerializeField]
        private DateTime lastModified;

        [SerializeField]
        private List<SceneSave> sceneSaves = new();

        [SerializeField]
        private List<SObjectPropertySave> scriptableObjectSaves = new();

        public SaveGame(int order, string stateName, string sceneName) {
            this.order = order;
            this.stateName = stateName;
            this.sceneName = sceneName;
            creationDate = DateTime.Now;
            lastModified = DateTime.Now;
        }

        public string StateName {
            get => stateName;
            set => stateName = value;
        }

        public string SceneName {
            get => sceneName;
            set => sceneName = value;
        }

        public DateTime CreationDate {
            get => creationDate;
            set => creationDate = value;
        }

        public Texture2D SaveGameIcon {
            get {
                var texture2D = new Texture2D(1280, 720);
                if (saveGameIcon != null) {
                    texture2D.LoadImage(saveGameIcon);
                }
                return texture2D;
            }
            set {
                if (value != null) {
                    saveGameIcon = value.EncodeToPNG();
                }
            }
        }

        public long PlayTime {
            get => playTime;
            set => playTime = value;
        }

        public int Order {
            get => order;
            set => order = value;
        }

        public DateTime LastModified {
            get => lastModified;
            set => lastModified = value;
        }

        public List<SceneSave> SceneSaves => sceneSaves;

        public List<SObjectPropertySave> ScriptableObjectSaves {
            get => scriptableObjectSaves;
            set => scriptableObjectSaves = value;
        }

        public List<string> Phases {
            get => phases;
            set => phases = value;
        }

        public SceneSave ActiveSceneSave() {
            return sceneSaves.Find(save => save.Scenename.Equals(sceneName));
        }
    }
}