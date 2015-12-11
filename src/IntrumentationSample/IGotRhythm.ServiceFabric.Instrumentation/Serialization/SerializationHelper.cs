using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace IGotRhythm.ServiceFabric.Instrumentation.Serialization
{
    public static class SerializationHelper
    {
        public static byte[] SerializeToBytes<T>(T value)
        {
            return SerializeToBytes(typeof(T), value);
        }

        public static byte[] SerializeToBytes(Type type, object value)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
                {
                    var serializer = new DataContractSerializer(type);
                    serializer.WriteObject(writer, value);
                }
                var result = stream.ToArray();
                return result;
            }
        }

        public static T DeserializeFromBytes<T>(byte[] value)
        {
            return (T)DeserializeFromBytes(typeof(T), value);
        }

        public static object DeserializeFromBytes(Type type, byte[] value)
        {
            using (var stream = new MemoryStream(value))
            {
                using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
                {
                    var serializer = new DataContractSerializer(type);
                    return serializer.ReadObject(reader);
                }
            }
        }
    }
}