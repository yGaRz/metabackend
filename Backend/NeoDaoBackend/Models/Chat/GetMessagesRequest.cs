using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Chat;

public class GetMessagesRequest
{
    [ValidNotEmptyString]
    public string ChannelId { get; set; } = null!;
    public PaginationModel Pagination { get; set; } = null!;
}

