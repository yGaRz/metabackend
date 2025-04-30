using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.UserLocation;

public class PlayerLocationDTO
{
    [ValidNotEmptyString]
    public string LevelName { get; set; } = null!;

    public double XCoordinate { get; set; }

    public double YCoordinate { get; set; }

    public double ZCoordinate { get; set; }
    [ValidNotEmptyString(IsOptional = true)]
    public string? Tag { get; set; }
}