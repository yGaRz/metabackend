using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.UserTransport;

public class UseTransportRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidNotEmptyString]
    public string TransportId { get; set; } = null!;
    [ValidNotNegativeInteger]
    public int Price { get; set; }
}