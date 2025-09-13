namespace Domain.Entities;

public class AdvertisingPlatformEntity
{
    public string Name { get; set; } = string.Empty;
    public List<string> Locations { get; set; } = [];
}