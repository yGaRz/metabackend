using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.UserRelations;

public class RelationWSResponse : IOutputMessageData
{
    public Guid UserId { get; set; }
}
