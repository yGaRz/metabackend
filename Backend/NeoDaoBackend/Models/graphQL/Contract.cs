using NeoDaoBackend.Models.graphQL.Enums;

namespace NeoDaoBackend.Models.graphQL;

public class Contract {
    // A set of fields is incomplete. Add additional fields if needed
    public string Name { get; set; }
    public string Symbol { get; set; }
    public ContractType Type { get; set; }
}
