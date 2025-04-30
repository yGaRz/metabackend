using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.graphQL;

public class TokenBalancesCollectionSegment : IOutputMessageData {
    public List<TokenBalance> Items;
    public CollectionSegmentInfo PageInfo;
    public int TotalCount;

    public override string ToString() {
        return JsonConvert.SerializeObject(this);
    }
}
