using Newtonsoft.Json;

namespace NeoDaoBackend.Models.UserRelations.Search;

public partial class SearchFilter
{
    public string? Text { get; set; }
    public bool? IsOnline { get; set; }
    public bool isWithFriendRelations { get; set; }
}