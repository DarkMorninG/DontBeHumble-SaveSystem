using System;

namespace DBH.SaveSystem.Attributes {
    [AttributeUsage(AttributeTargets.Field)]
    public class PlayerSaved : Attribute {
        private readonly string name;

        public PlayerSaved(string name) {
            this.name = name;
        }

        public string Name => name;
    }
}