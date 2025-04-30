using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.NFTAuction;

public class ReturnToWaitingRequest
{
    [ValidGuid]
    public Guid LotId { get; set; }
    [ValidPositiveInteger]
    public int InitialPrice { get; set; }
}