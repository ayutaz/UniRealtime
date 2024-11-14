using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UniRealtime
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Voice
    {
        [EnumMember(Value = "alloy")] Alloy,
        [EnumMember(Value = "ash")] Ash,
        [EnumMember(Value = "ballad")] Ballad,
        [EnumMember(Value = "coral")] Coral,
        [EnumMember(Value = "echo")] Echo,
        [EnumMember(Value = "sage")] Sage,
        [EnumMember(Value = "shimmer")] Shimmer,
        [EnumMember(Value = "verse")] Verse
    }
}
