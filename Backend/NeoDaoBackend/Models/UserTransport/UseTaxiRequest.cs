using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.UserTransport;

public class UseTaxiRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidNotNegativeInteger]
    public int Price { get; set; }
}