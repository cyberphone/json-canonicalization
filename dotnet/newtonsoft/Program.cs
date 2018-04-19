using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace newtonsoft
{
    public class MyObject
    {
        public string id;

        public long counter;

        public string[] list;

        [JsonProperty("\u20ac")]
        public bool EuroIsGreat;
    }

    class Program
    {
        static void Main(string[] args)
        {
            MyObject myObject = new MyObject
            {
                counter = 3,
                id = "johndoe",
                EuroIsGreat = true,
                list = new string[]{ "yes" ,"no"}
            };
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine(JsonConvert.SerializeObject(myObject, Formatting.None,
                     new JsonSerializerSettings { ContractResolver = new Canonicalizer() }));
        }
    }
}
