using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters;

namespace DBH.SaveSystem.json {
    public class FixedUnityTypeContractResolver : UnityTypeContractResolver {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            if (!jsonProperty.Ignored && member.GetCustomAttribute<JsonIgnoreAttribute>() != null) {
                jsonProperty.Ignored = true;
            }

            if (member.MemberType == MemberTypes.Property) {
                jsonProperty.Ignored = true;
            }

            return jsonProperty;
        }
    }
}