using Ingweland.Fog.Models.Hoh.Enums;
using ProtoBuf;

namespace Ingweland.Fog.Dtos.Hoh;

[ProtoContract]
public class EliteArenaTierDto
{
    [ProtoMember(1)]
    public required EliteArenaTier Tier { get; init; }
    [ProtoMember(2)]
    public required string Name { get; init; }
}
