using FluentResults;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Ingweland.Fog.InnSdk.Hoh.Constants;
using Ingweland.Fog.InnSdk.Hoh.Errors;
using Ingweland.Fog.InnSdk.Hoh.Net.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.InnSdk.Hoh.Services;

public class WoaService(
    IGameApiClient apiClient,
    ILogger<WoaService> logger) : IWoaService
{
    public Task<Result<byte[]>> GetConquestLogRawDataAsync(GameWorldConfig world, long eventId, long divisionId,
        int pageSize = 50, DateTime? afterCreatedAt = null)
    {
        logger.LogInformation("Fetching WoA conquest log {@Data}",
            new {world.Id, eventId, divisionId, pageSize, afterCreatedAt});

        var payload = new WoAConquestLogRequest
        {
            EventId = eventId,
            DivisionId = divisionId,
            PageSize = pageSize,
        };

        if (afterCreatedAt.HasValue)
        {
            payload.AfterCreatedAt = Timestamp.FromDateTime(afterCreatedAt.Value.ToUniversalTime());
        }

        return Result.Try(
            () => apiClient.SendForProtobufAsync(world, GameEndpoints.WoaConquestLogPath, payload.ToByteArray()),
            e => new NetworkError(
                $"Failed to fetch WoA conquest log for event {eventId}, division {divisionId} in world {world.Id}", e));
    }
}
