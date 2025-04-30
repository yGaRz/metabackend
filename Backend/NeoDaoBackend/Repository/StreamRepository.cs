using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Repository.Interface;
using Stream = NeoDaoBackend.Models.db.Stream;

namespace NeoDaoBackend.Repository;

public class StreamRepository : IStreamRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public StreamRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Stream?> GetStreamById(Guid streamId, CancellationToken ct)
    {
        return await _dbContext.Streams
            .Where(e => e.StreamId == streamId)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<bool> StreamExists(Guid userId, CancellationToken ct)
    {
        var stream = await GetStreamById(userId, ct);
        return stream != null;
    }

    public async Task<Guid> CreateStream(Stream stream, CancellationToken ct)
    {
        Guid streamId = Guid.NewGuid();
        stream.StreamId = streamId;
        _dbContext.Streams.Add(stream);
        await _dbContext.SaveChangesAsync(ct);
        return streamId;
    }

    public async Task UpdateStream(Stream stream, CancellationToken ct)
    {
        _dbContext.Streams.Update(stream);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteStream(Stream stream, CancellationToken ct)
    {
        _dbContext.Streams.Remove(stream);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task<Stream?> GetLastStream(CancellationToken ct)
    {
        return await _dbContext.Streams
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpdateLastStream(Stream stream, CancellationToken ct)
    {
        await _dbContext.Streams
            .Where(s => s.StreamId == stream.StreamId)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.StartTime, s => s.StartTime.AddMinutes(30)) // Сдвигаем StartTime на 30 минут
                    .SetProperty(s => s.EndTime, s => s.EndTime!.Value.AddMinutes(30)),   // Сдвигаем на 30 минут относительно текущего EndTime
                ct);
    }

    public async Task<PagedDtoResponse<Stream>> GetStreams(PaginationModel pagination, CancellationToken ct)
    {
        var streams = await _dbContext.Streams
            .OrderByDescending(s => s.StartTime)
            .Skip(pagination.Offset)
            .Take(pagination.Count)
            .ToListAsync(ct);

        var totalRecords = await _dbContext.Streams.CountAsync(ct);

        return new PagedDtoResponse<Stream>(streams, totalRecords);
    }
}