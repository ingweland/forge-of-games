using System.Collections.ObjectModel;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Functions.Validators;
using Ingweland.Fog.Infrastructure.Entities;
using Ingweland.Fog.Infrastructure.Enums;
using Ingweland.Fog.Infrastructure.Repositories.Abstractions;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public class InGameDataReceiver(
    ILogger<InGameDataReceiver> logger,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    HohHelperResponseDtoToTablePkConverter hohHelperResponseDtoToTablePkConverter,
    DatabaseWarmUpService databaseWarmUpService,
    HohHelperResponseDtoValidator dtoValidator,
    IQueueRepository<InGameRawDataQueueMessage> queueRepository)
{
    private static readonly ReadOnlySet<InGameDataProcessingServiceType> ImmediateProcessingServiceTypes =
        new(new HashSet<InGameDataProcessingServiceType>
        {
            InGameDataProcessingServiceType.Battle,
            InGameDataProcessingServiceType.WakeupLeaderboards,
            InGameDataProcessingServiceType.WakeupAlliance,
            InGameDataProcessingServiceType.WakeupAllianceWoa,
        });

    [Function("InGameDataReceiver")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hoh/inGameData")]
        HttpRequest req,
        [Microsoft.Azure.Functions.Worker.Http.FromBody]
        HohHelperResponseDto inGameData)
    {
        await databaseWarmUpService.WarmUpDatabaseIfRequiredAsync();

        SetDebugCorsHeaders(req);

        if (!dtoValidator.Validate(inGameData, out var error))
        {
            logger.LogError("Dto validation failed: {error}", error);

            return new BadRequestResult();
        }

        List<( InGameDataProcessingServiceType ProcessingServiceType, string PartitionKey, string RowKey)> keys = [];
        Guid? submissionId = inGameData.SubmissionId != null ? Guid.Parse(inGameData.SubmissionId) : null;
        try
        {
            var now = DateTime.UtcNow;
            logger.LogDebug("Processing raw data at {Time}", now);
            var rawData = new InGameRawData
            {
                RequestBase64Data = inGameData.Base64RequestData,
                Base64Data = inGameData.Base64ResponseData!,
                SubmissionId = submissionId,
                CollectedAt = now,
            };
            var date = DateOnly.FromDateTime(now);
            foreach (var t in hohHelperResponseDtoToTablePkConverter.Get(inGameData, date))
            {
                var rowKey = await inGameRawDataTableRepository.SaveAsync(rawData, t.PartitionKey);
                keys.Add((t.ProcessingServiceType, t.PartitionKey, rowKey));
                logger.LogDebug("Saved raw data for partition key: {PartitionKey}", t.PartitionKey);
            }

            logger.LogDebug("Processing raw data completed");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while processing raw data");
            throw;
        }

        try
        {
            foreach (var tuple in keys)
            {
                await QueueForImmediateProcessingIfRequired(tuple.ProcessingServiceType, tuple.PartitionKey,
                    tuple.RowKey);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while queueing for immediate processing");
        }

        return new NoContentResult();
    }

    private Task QueueForImmediateProcessingIfRequired(InGameDataProcessingServiceType processingServiceType,
        string partitionKey, string rowKey)
    {
        if (ImmediateProcessingServiceTypes.Contains(processingServiceType))
        {
            return queueRepository.SendMessageAsync(new InGameRawDataQueueMessage
            {
                ProcessingServiceType = processingServiceType,
                PartitionKey = partitionKey,
                RowKey = rowKey,
            });
        }

        return Task.CompletedTask;
    }

    private void SetDebugCorsHeaders(HttpRequest req)
    {
#if DEBUG
        // Set CORS headers
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST, OPTIONS");
        req.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
#endif
    }
}
