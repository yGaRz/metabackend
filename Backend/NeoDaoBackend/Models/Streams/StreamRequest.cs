using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Streams;

public class StreamRequest : IInputMessageData
{
    public Guid Id { get; set; }
}