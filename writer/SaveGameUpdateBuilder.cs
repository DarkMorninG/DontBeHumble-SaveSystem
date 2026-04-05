using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DBH.SaveSystem.writer {
    public class SaveGameUpdateBuilder {
        private readonly List<JObject> customTypeObjects;

        public SaveGameUpdateBuilder(List<JObject> customTypeObjects) {
            this.customTypeObjects = customTypeObjects;
        }

        public SaveGameUpdateBuilder RemoveProperty(string typeName, string propertyName) {
            foreach (var obj in FindByType(typeName)) {
                obj.Remove(propertyName);
            }
            return this;
        }

        public SaveGameUpdateBuilder RenameProperty(string typeName, string oldPropertyName, string newPropertyName) {
            foreach (var obj in FindByType(typeName)) {
                var property = obj.Property(oldPropertyName);
                if (property != null) {
                    obj[newPropertyName] = property.Value;
                    obj.Remove(oldPropertyName);
                }
            }
            return this;
        }

        public SaveGameUpdateBuilder UpdateTypeReference(string oldTypeName, string newTypeName) {
            foreach (var obj in customTypeObjects) {
                var type = obj["$type"]?.ToString();
                if (type != null && type.Contains(oldTypeName)) {
                    obj["$type"] = type.Replace(oldTypeName, newTypeName);
                }
            }
            return this;
        }

        public SaveGameUpdateBuilder SetPropertyValue(string typeName, string propertyName, JToken value) {
            foreach (var obj in FindByType(typeName)) {
                obj[propertyName] = value;
            }
            return this;
        }

        public SaveGameUpdateBuilder AddProperty(string typeName, string propertyName, JToken defaultValue) {
            foreach (var obj in FindByType(typeName)) {
                if (obj.Property(propertyName) == null) {
                    obj[propertyName] = defaultValue;
                }
            }
            return this;
        }

        public SaveGameUpdateBuilder ConvertType(string typeName, Func<JObject, string> newTypeResolver) {
            foreach (var obj in FindByType(typeName)) {
                var resolvedType = newTypeResolver(obj);
                if (resolvedType != null) {
                    obj["$type"] = obj["$type"].ToString().Replace(typeName, resolvedType);
                }
            }
            return this;
        }

        public SaveGameUpdateBuilder ForEach(string typeName, Action<JObject> action) {
            foreach (var obj in FindByType(typeName)) {
                action(obj);
            }
            return this;
        }

        private IEnumerable<JObject> FindByType(string typeName) {
            return customTypeObjects.Where(o => TypeMatches(o, typeName));
        }

        private static bool TypeMatches(JObject obj, string typeName) {
            var type = obj["$type"]?.ToString();
            return type != null && type.Contains(typeName);
        }
    }
}
