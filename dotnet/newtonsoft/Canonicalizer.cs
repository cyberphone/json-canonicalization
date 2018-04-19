using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace newtonsoft
{
    public class Canonicalizer : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            SortedList<string, JsonProperty> newList = new SortedList<string, JsonProperty>(StringComparer.Ordinal);
            foreach (JsonProperty jsonProperty in base.CreateProperties(type, memberSerialization))
            {
                newList.Add(jsonProperty.PropertyName, jsonProperty);
            }
            return newList.Values;
        }
    }
}
