using ProtoBuf;

namespace Ingweland.Fog.Models.Hoh.Enums;

[ProtoContract]
public enum UnitType
{
    Undefined = 0,
    Cavalry,
    HeavyInfantry,
    Infantry,
    Ranged,
    Siege,
    Spawner,
    Gate,
    Ally,
    Ghost,
    AllyNaya,
    AllyZhengYiSao,
}
