using NeoDaoBackend.Models.Common;

namespace NeoDaoBackend.Models.UserRelations.Search;

public class UserRelationsSearchRequest
{
    public SearchFilter Filter { get; set; } = null!;
    public PaginationModel Pagination { get; set; } = null!;
    public List<SortingField>? SortFields { get; set; }
}
