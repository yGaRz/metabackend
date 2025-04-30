using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.Streams;

public class StreamResponse : IOutputMessageData
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }    
}