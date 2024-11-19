using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UniRealtime
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AudioFormat
    {
        [EnumMember(Value = "pcm16")] PCM16,

        [EnumMember(Value = "g711_ulaw")] G711Ulaw,

        [EnumMember(Value = "g711_alaw")] G711Alaw
    }
}
