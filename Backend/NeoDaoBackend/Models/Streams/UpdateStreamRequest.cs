using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Streams;

public class UpdateStreamRequest: CreateStreamRequest
{
    [ValidGuid]
    public Guid StreamId { get; set; }
}