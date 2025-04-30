using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.Streams;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class StreamController: BaseController
{
    private readonly StreamingService _streamingService;
    public StreamController(IValidationStorage validationStorage, StreamingService streamingService) : base(validationStorage)
    {
        _streamingService = streamingService;
    }

    [ServerAuthorized]
    [HttpGet("getStreams")]
    public async Task<IActionResult> GetStreams([FromQuery] PaginationModel paginationRequest, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _streamingService.GetStreams(paginationRequest, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("createStream")]
    public async Task<IActionResult> CreateStream([FromBody] CreateStreamRequest streamData, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _streamingService.CreateStream(streamData, token), ct);
    }

    [ServerAuthorized]
    [HttpPut("updateStream")]
    public async Task<IActionResult> UpdateStream([FromBody] UpdateStreamRequest streamData, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _streamingService.UpdateStream(streamData, token), ct);
    }
    
    [ServerAuthorized]
    [HttpDelete("deleteStream/{id}")]
    public async Task<IActionResult> DeleteStream([ValidGuid] Guid id, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _streamingService.DeleteStream(id, token), ct);
    }
}