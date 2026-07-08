namespace Ingweland.Fog.Application.Server.Settings;

public class DefaultDomainRestrictionSettings
{
    public const string CONFIGURATION_PROPERTY_NAME = "DefaultDomainRestriction";
    public bool Enabled { get; set; } = true;
    public List<string> AllowedIPs { get; set; } = [];
}
