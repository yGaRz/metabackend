namespace NeoDaoBackend.Models.Common;

public class PagedDtoResponse<T>
{
    public int Total { get; set; }
    public List<T> Data { get; set; }

    public PagedDtoResponse(List<T> data, int totalRecords)
    {
        Data = data;
        Total = totalRecords;
    }
}