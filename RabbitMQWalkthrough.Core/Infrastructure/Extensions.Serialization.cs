using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RabbitMQWalkthrough.Core.Infrastructure
{
    public static partial class Extensions
    {
        public static string Serialize<T>(this T objectToSerialize) => JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented);
        
        public static T Deserialize<T>(this string jsonText) => JsonConvert.DeserializeObject<T>(jsonText);

        public static byte[] ToByteArray(this string text) => System.Text.Encoding.UTF8.GetBytes(text);

        public static string ToUTF8String(this byte[] bytes) => System.Text.Encoding.UTF8.GetString(bytes);

        public static ReadOnlyMemory<byte> ToReadOnlyMemory(this byte[] bytes) => new ReadOnlyMemory<byte>(bytes);

        

    }
}
