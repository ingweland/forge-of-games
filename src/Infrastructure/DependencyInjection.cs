using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Settings;
using Ingweland.Fog.Infrastructure.Entities;
using Ingweland.Fog.Infrastructure.Repositories;
using Ingweland.Fog.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ingweland.Fog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultSQL");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultSQL' not found.");
        }

        services.AddDbContext<IFogDbContext, FogDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions => { sqlOptions.CommandTimeout(120); });
        });
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddTableStorage();
        services.AddAzureQueues();
        services.AddBlobStorage();

        services.TryAddSingleton<IHohDataProvider, AzureBlobStorageHohDataProvider>();
        services.TryAddSingleton<IHohLocalizationDataProvider, AzureBlobStorageHohLocalizationDataProvider>();
        services.AddScoped<IInGameStartupDataRepository, InGameStartupDataRepository>();
        services.AddScoped<ICommandCenterProfileRepository, CommandCenterProfileRepository>();
        services.AddScoped<IHohCityRepository, HohCityRepository>();
        services.AddScoped<IInGameRawDataTableRepository, InGameRawDataTableRepository>();

        return services;
    }

    private static IServiceCollection AddBlobStorage(this IServiceCollection services)
    {
        services.AddSingleton<IHohCoreDataAzureContainerClient, HohCoreDataAzureContainerClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageSettings>>();
            return new HohCoreDataAzureContainerClient(options.Value);
        });
        services.AddSingleton<IHohRawCoreDataAzureContainerClient, HohRawCoreDataAzureContainerClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageSettings>>();
            return new HohRawCoreDataAzureContainerClient(options.Value);
        });

        services.AddSingleton<IFogSharedDataStorageRepository>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageSettings>>().Value;
            return new FogSharedDataRepository(options.ConnectionString, options.FogSharedDataContainer);
        });

        services.AddSingleton<IInGameRawDataBlobRepository>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageSettings>>().Value;
            return new InGameRawDataBlobRepository(options.ConnectionString, options.InGameRawDataTempContainer);
        });

        services.AddSingleton<ISharedImageStorageRepository>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageSettings>>().Value;
            return new SharedImageStorageRepository(options.ConnectionString, options.FogSharedImagesContainer);
        });

        return services;
    }

    private static IServiceCollection AddTableStorage(this IServiceCollection services)
    {
        services.AddTableStorage<CcProfileTableEntity>(options => options.CommandCenterProfilesTable);
        services.AddTableStorage<InGameStartupDataTableEntity>(options => options.HohStartupDataTable);
        services.AddTableStorage<HohCityTableEntity>(options => options.CityPlannerCitiesTable);
        services.AddTableStorage<InGameRawDataTableEntity>(options => options.InGameRawDataTable);

        return services;
    }

    private static IServiceCollection AddTableStorage<TTableEntity>(
        this IServiceCollection services,
        Func<StorageSettings, string> tableNameSelector)
        where TTableEntity : TableEntityBase
    {
        services.AddSingleton<ITableStorageRepository<TTableEntity>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageSettings>>().Value;

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("Storage connection string is missing.");
            }

            var tableName = tableNameSelector(options);
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new InvalidOperationException($"Table name for {typeof(TTableEntity).Name} is missing.");
            }

            return new TableStorageRepository<TTableEntity>(
                options.ConnectionString,
                tableName);
        });

        return services;
    }

    private static IServiceCollection AddAzureQueues(this IServiceCollection services)
    {
        services.AddSingleton<IQueueRepository<InGameRawDataQueueMessage>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageSettings>>().Value;
            return new QueueRepository<InGameRawDataQueueMessage>(options.ConnectionString,
                options.InGameRawDataProcessingQueue);
        });

        return services;
    }
}
