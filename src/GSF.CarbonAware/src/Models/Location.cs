namespace GSF.CarbonAware.Models;

public record Location
{
    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public string? Name { get; init; }

    public static implicit operator Location(global::CarbonAware.Model.Location location) {
        return new Location {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Name = location.Name
        };
    }
}
