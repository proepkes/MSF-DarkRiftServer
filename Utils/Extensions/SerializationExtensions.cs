using System.Collections.Generic;
using System.IO;
using System.Linq;
using DarkRift;
using Utils.Conversion;
using Utils.IO;

namespace Utils.Extensions
{
    /// <summary>
    ///     Contains functions to help easily serialize / deserialize some common types
    /// </summary>
    public static class SerializationExtensions
    {
        public static byte[] ToBytes(this Dictionary<string, string> dictionary)
        {
            byte[] b;
            using (var ms = new MemoryStream())
            {
                using (var writer = new EndianBinaryWriter(EndianBitConverter.Big, ms))
                {
                    dictionary.ToWriter(writer);
                }

                b = ms.ToArray();
            }

            return b;
        }

        public static void ToWriter(this Dictionary<string, string> dictionary, EndianBinaryWriter writer)
        {
            if (dictionary == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(dictionary.Count);

            foreach (var item in dictionary)
            {
                writer.Write(item.Key);
                writer.Write(item.Value);
            }
        }

        public static Dictionary<string, string> FromReader(this Dictionary<string, string> dictionary,
            EndianBinaryReader reader)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                if (dictionary.ContainsKey(key))
                    dictionary[key] = value;
                else
                    dictionary.Add(key, value);
            }

            return dictionary;
        }

        public static Dictionary<string, string> FromBytes(this Dictionary<string, string> dictionary, byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new EndianBinaryReader(EndianBitConverter.Big, ms))
                {
                    dictionary.FromReader(reader);
                }
            }

            return dictionary;
        }
    }
}