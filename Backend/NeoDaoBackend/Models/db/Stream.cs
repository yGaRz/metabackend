namespace NeoDaoBackend.Models.db;

public class Stream
{
    public Guid StreamId { get; set; }
    public string Url { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public DateTimeOffset Created { get; set; }
}