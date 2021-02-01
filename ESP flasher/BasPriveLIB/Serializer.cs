using System;
using System.IO;

namespace BasPriveLib
{
    public abstract class Serializer
    {
        public abstract void Serialize<T>(T obj, Stream stream);
        public abstract T Deserialize<T>(Stream stream);

        public void Serialize<T>(T obj, string filename)
        {
            using (Stream s = File.Open(filename, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                Serialize<T>(obj, s);
            }
        }

        public T Deserialize<T>(string filename)
        {
            using (Stream s = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                return Deserialize<T>(s);
            }
        }


    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializerVersionAttribute : Attribute
    {
        public string Version { get; set; } = "";
    }
}

