using FluentResults;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;

namespace Ingweland.Fog.InnSdk.Hoh.Services.Abstractions;

public interface IWoaService
{
    Task<Result<byte[]>> GetConquestLogRawDataAsync(GameWorldConfig world, long eventId, long divisionId,
        int pageSize = 50, DateTime? afterCreatedAt = null);
}
