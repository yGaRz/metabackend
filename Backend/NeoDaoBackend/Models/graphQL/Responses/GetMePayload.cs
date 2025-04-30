using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.graphQL.Responses;

public class GetMePayload : IOutputMessageData {
    public Guid Id { get; set; }
    public string Address { get; set; }
    public string Name { get; set; }
    public string? ProfilePic { get; set; }
    public int TactileLevel { get; set; }
    public int UniteVerseLevel { get; set; }
}
