using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace datacontract
{
    [DataContract]
    public class MyObject
    {
        [DataMember]
        public string escaping;

        [DataMember(Order = 3)]
        public double aDouble;

        [DataMember]
        public long nonInteroperableLong;

        public long interoperableLong
        {
            get { return long.Parse(interoperableLongAsString); }
            set { interoperableLongAsString = value.ToString(); }
        }

        [DataMember(Name = "interoperableLong")]
        private string interoperableLongAsString;

        // Holds the optional HMAC signature
        [DataMember(EmitDefaultValue = false)]
        public string hmac = null;
    }
}
