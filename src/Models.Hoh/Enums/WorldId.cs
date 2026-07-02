using ProtoBuf;

namespace Ingweland.Fog.Models.Hoh.Enums;

[ProtoContract]
public enum WorldId
{
    Undefined = 0,
    Starter,
    TeslaStormBlue,
    TeslaStormGreen,
    TeslaStormPurple,
    TeslaStormRed,
    TeslaStormYellow,
    SiegeOfOrleans,
    SpartasLastStand,
    FallOfTroy,
    AncientEgyptDungeon,
    ScyllaDungeon,
}
