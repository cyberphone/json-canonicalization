using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace datacontract
{
    [DataContract]
    public class MyObject : IEquatable<MyObject>
    {
        [DataMember]
        public string escaping;

        [DataMember]
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

        public bool Equals(MyObject other)
        {
            return escaping.Equals(other.escaping) &&
                   aDouble == other.aDouble &&
                   nonInteroperableLong == other.nonInteroperableLong &&
                   interoperableLong == other.interoperableLong;
        }
    }
}
