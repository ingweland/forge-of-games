using System.Text.Json.Serialization;

namespace Ingweland.Fog.Models.Hoh.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CityId
{
    Undefined = 0,
    Capital,
    China,
    Egypt,
    Mayas_ChichenItza,
    Mayas_SayilPalace,
    Mayas_Tikal,
    Vikings,
    Arabia_CityOfBrass,
    Arabia_NoriasOfHama,
    Arabia_Petra,
    AncientEgyptEvent,
    Ithaka,
}
