using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace BasPriveLib
{
    public class JSON : Serializer
    {
        public override T Deserialize<T>(Stream data)
        {
            using (StreamReader sr = new StreamReader(data))
            {
                string serial = sr.ReadToEnd();

                return JsonConvert.DeserializeObject<T>(serial, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                });
            }

        }
        public override void Serialize<T>(T obj, Stream stream)
        {
            using (StreamWriter sr = new StreamWriter(stream))
            {
                string serial = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                });

                sr.WriteLine(serial);
            }
        }

        public T Deserialize<T>(byte[] data)
        {

            string serial = Encoding.ASCII.GetString(data);

            return JsonConvert.DeserializeObject<T>(serial, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });



        }
        public byte[] Serialize<T>(T obj)
        {

            string serial = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                Formatting = Formatting.Indented
            });

            return Encoding.ASCII.GetBytes(serial);
        }


        public object Deserialize(byte[] data)
        {

            string serial = Encoding.ASCII.GetString(data);

            return JsonConvert.DeserializeObject(serial, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });



        }
    }
}

