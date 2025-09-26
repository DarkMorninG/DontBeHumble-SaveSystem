using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters;
using UnityEngine;

namespace DBH.SaveSystem.json {
    public class FixedUnityTypeContractResolver : UnityTypeContractResolver {
        
        protected override List<MemberInfo> GetSerializableMembers(Type objectType) {
            var members = base.GetSerializableMembers(objectType);

            // Include inherited private/protected fields & properties marked with [SerializeField]
            var current = objectType.BaseType;
            while (current != null && current != typeof(object)) {
                // Fields
                var fields = current.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var f in fields) {
                    if (f.GetCustomAttribute<SerializeField>() != null && !members.Contains(f)) {
                        members.Add(f);
                    }
                }
                // Properties (rarely used with [SerializeField], but supported)
                var props = current.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var p in props) {
                    if (p.GetCustomAttribute<SerializeField>() != null && !members.Contains(p)) {
                        members.Add(p);
                    }
                }
                current = current.BaseType;
            }

            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            // Respect explicit [JsonIgnore] on members
            if (!jsonProperty.Ignored && member.GetCustomAttribute<JsonIgnoreAttribute>() != null) {
                jsonProperty.Ignored = true;
            }

            // Only serialize fields by default; ignore properties unless enabled by UnityTypeContractResolver rules
            if (member.MemberType == MemberTypes.Property) {
                jsonProperty.Ignored = true;
            }

            return jsonProperty;
        }
    }
}