namespace Domain.Entities;

public class AdvertisingPlatformEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Locations { get; set; } = [];
}