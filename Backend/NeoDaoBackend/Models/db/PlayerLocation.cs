namespace NeoDaoBackend.Models.db;

public class PlayerLocation
{
    public Guid UserId { get; set; }

    public string LevelName { get; set; }

    public double XCoordinate { get; set; }

    public double YCoordinate { get; set; }

    public double ZCoordinate { get; set; }

    public string? Tag { get; set; }

    public virtual User User { get; set; }
}