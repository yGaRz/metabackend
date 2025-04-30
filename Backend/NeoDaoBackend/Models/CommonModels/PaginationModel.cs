using NeoDaoBackend.Validation.Attributes;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.Common;

public partial class PaginationModel
{
    [ValidNotNegativeInteger]
    public int Offset { get; set; }
    [ValidCount]
    public int Count { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
