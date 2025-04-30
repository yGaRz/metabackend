using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.UserTransport;

public class AddTransportToUserRequest
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidNotEmptyString]
    public string TransportId { get; set; } = null!;    
}
