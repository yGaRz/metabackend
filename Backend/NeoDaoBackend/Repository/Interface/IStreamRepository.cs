using NeoDaoBackend.Models.Common;
using Stream = NeoDaoBackend.Models.db.Stream;

namespace NeoDaoBackend.Repository.Interface;

public interface IStreamRepository
{
    Task<Stream?> GetStreamById(Guid streamId, CancellationToken ct);
    Task<bool> StreamExists(Guid userId, CancellationToken ct);
    Task<Guid> CreateStream(Stream stream, CancellationToken ct);
    Task UpdateStream(Stream stream, CancellationToken ct);
    Task DeleteStream(Stream stream, CancellationToken ct);
    Task<Stream?> GetLastStream(CancellationToken ct);
    Task UpdateLastStream(Stream stream, CancellationToken ct);
    Task<PagedDtoResponse<Stream>> GetStreams(PaginationModel paginationModel, CancellationToken ct);
}