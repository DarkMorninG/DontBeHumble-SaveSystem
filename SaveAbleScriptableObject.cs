using UnityEngine;

namespace DBH.SaveSystem {
    public abstract class SaveAbleScriptableObject : ScriptableObject {
        public virtual void AfterSaveGameLoad() {
        }

        public virtual string Identifier => name;
    }
}