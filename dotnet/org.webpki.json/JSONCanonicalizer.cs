using System;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;

// Ultra simple canonicalizer for .NET
// Needs an improvment for "Number" to cope with the full spec... 

namespace Org.Webpki.Json
{
    public class JSONCanonicalizer
    {
        JSONReader jsonReader;
        StringBuilder buffer;

        public JSONCanonicalizer(TextReader reader)
        {
            jsonReader = new JSONReader(reader);
        }

        public JSONCanonicalizer(string jsonData)
            : this(new StringReader(jsonData))
        {
        }

        public JSONCanonicalizer(byte[] jsonData)
            : this(new UTF8Encoding().GetString(jsonData))
        {

        }

        private void Escape(char c)
        {
            buffer.Append('\\').Append(c);
        }

        private void SerializeString(string value)
        {
            buffer.Append('"');
            foreach (char c in value)
            {
                switch(c)
                {
                    case '\n':
                        Escape('n');
                        break;
                    case '\b':
                        Escape('b');
                        break;

                    case '\f':
                        Escape('f');
                        break;

                    case '\r':
                        Escape('r');
                        break;

                    case '\t':
                        Escape('t');
                        break;

                    case '"':
                    case '\\':
                        Escape(c);
                        break;

                    default:
                        if (c < ' ')
                        {
                            buffer.Append("\\u").Append(((int)c).ToString("x04"));
                        }
                        else
                        {
                            buffer.Append(c);
                        }
                        break;
                }
            }
            buffer.Append('"');
        }

        private void Serialize(object o)
        {
            if (o is OrderedDictionary)
            {
                SortedDictionary<string, object> dict =
                    new SortedDictionary<string, object>(StringComparer.Ordinal);
                foreach (DictionaryEntry directoryEntry in (OrderedDictionary)o)
                {
                    dict.Add((string)directoryEntry.Key, directoryEntry.Value);
                }
                buffer.Append('{');
                bool next = false;
                foreach (var directoryEntry in dict)
                {
                    if (next)
                    {
                        buffer.Append(',');
                    }
                    next = true;
                    SerializeString(directoryEntry.Key);
                    buffer.Append(':');
                    Serialize(directoryEntry.Value);
                }
                buffer.Append('}');
            }
            else if (o is List<object>)
            {
                buffer.Append('[');
                bool next = false;
                foreach (object value in (List<object>)o)
                {
                    if (next)
                    {
                        buffer.Append(',');
                    }
                    next = true;
                    Serialize(value);
                }
                buffer.Append(']');
            }
            else if (o == null)
            {
                buffer.Append("null");
            }
            else if (o is String)
            {
                SerializeString((string)o);
            }
            else if (o is Boolean)
            {
                buffer.Append(o.ToString().ToLowerInvariant());
            }
            else if (o is Double)
            {
                buffer.Append(o.ToString().ToLowerInvariant());
            }
        }

        public string GetEncodedString()
        {
            buffer = new StringBuilder();
            Serialize(jsonReader.Read());
            return buffer.ToString();
        }

        public byte[] GetEncodedUTF8()
        {
            return new UTF8Encoding().GetBytes(GetEncodedString());
        }
    }
}
