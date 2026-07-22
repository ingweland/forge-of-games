namespace Ingweland.Fog.Application.Server.Settings;

public class StorageSettings
{
    public const string CONFIGURATION_PROPERTY_NAME = "StorageSettings";
    public required string CityPlannerCitiesTable { get; set; }
    public required string CommandCenterProfilesTable { get; set; }
    public required string ConnectionString { get; set; }
    public required string FogSharedDataContainer { get; set; }

    public required string HohCoreDataContainer { get; set; }
    public required string HohRawCoreDataContainer { get; set; }
    public required string HohStartupDataTable { get; set; }
    public required string InGameRawDataProcessingQueue { get; set; }
    public required string InGameRawDataTable { get; set; }
    public required string InGameRawDataTempContainer { get; set; }
    public required string FogSharedImagesContainer { get; set; }
}
