using Ingweland.Fog.Models.Hoh.Enums;
using ProtoBuf;

namespace Ingweland.Fog.Dtos.Hoh;

[ProtoContract]
public class WoaTierDto
{
    public static readonly WoaTierDto Default = new() {Tier = WoaTier.Undefined, Name = string.Empty};

    [ProtoMember(2)]
    public required string Name { get; init; }

    [ProtoMember(1)]
    public required WoaTier Tier { get; init; }
}
