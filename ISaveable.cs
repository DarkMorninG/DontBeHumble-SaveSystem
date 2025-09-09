using System;

namespace DBH.SaveSystem {
    public interface ISaveable {

        void AfterSaveGameLoad() {
        }

        string Identifier { get; }

        Type GetType();
    }
}