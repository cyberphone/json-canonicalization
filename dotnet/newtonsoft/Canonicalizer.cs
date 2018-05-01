using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace newtonsoft
{
    public class Canonicalizer : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName, StringComparer.Ordinal).ToList();
        }
    }
}
