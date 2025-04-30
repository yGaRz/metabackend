using AutoMapper;
using NeoDaoBackend.Models.Auth;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.Streams;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using Newtonsoft.Json;
using Stream = NeoDaoBackend.Models.db.Stream;

namespace NeoDaoBackend.Service;

public class StreamingService : AbstractWebSocketService<StreamEndpointKind>
{
    private readonly IStreamRepository _streamRepository;
    private readonly ChatService _chatService;
    private readonly IMapper _mapper;

    public StreamingService(
        ILogger<StreamingService> logger,
        IStreamRepository streamRepository,
        IMapper mapper,
        IValidationStorage validationStorage,
        JsonSerializerSettings jsonSerializerSettings,
        ChatService chatService,
        NotificationService notificationService) :
        base(EndpointCategory.Streaming, validationStorage, logger, jsonSerializerSettings, notificationService)
    {
        _streamRepository = streamRepository;
        _mapper = mapper;
        _chatService = chatService;
    }

    protected override async Task<IOutputMessageData?> ProcessWebSocketMessage(NeoDaoUser user, StreamEndpointKind endpointKind, string? data, CancellationToken ct)
    {
        IOutputMessageData? outputMessage = null;
        switch (endpointKind)
        {
            case StreamEndpointKind.GetStream:
                outputMessage = await GetStream(user.UserId!.Value, data!, ct);
                break;
        }
        return outputMessage;
    }

    #region Action

    private async Task<IOutputMessageData?> GetStream(Guid userId, string data, CancellationToken ct)
    {
        StreamRequest request = JsonConvert.DeserializeObject<StreamRequest>(data!)!;
        bool valid = await ValidateStreamById(request, ct);
        if (!valid)
        {
            return null!;
        }
        Stream? stream = await _streamRepository.GetStreamById(request.Id, ct);
        var streamDTO = _mapper.Map<StreamResponse>(stream);
        //Это костыль который создает чат для стрима. Его надо будет заменить на корретный эндпоинт для добавления стрима в БД и создания его на бэкенде
        if (!_chatService.IsChatExists($"stream:{request.Id}"))
        {
            await _chatService.CreateChat($"stream:{request.Id}", ct);
        }
        return streamDTO;
    }

    public async Task<PagedDtoResponse<Stream>> GetStreams(PaginationModel paginationRequest, CancellationToken ct)
    {
        return await _streamRepository.GetStreams(paginationRequest, ct);
    }
    
    public async Task<Guid> CreateStream(CreateStreamRequest streamData, CancellationToken ct)
    {
        Stream stream = new Stream
        {
            Url = streamData.Url,
            StartTime = streamData.StartTime,
            EndTime = streamData.EndTime
        };
        return await _streamRepository.CreateStream(stream, ct);
    }

    public async Task<bool> UpdateStream(UpdateStreamRequest streamData, CancellationToken ct)
    {
        bool isValid = await ValidateStreamById(streamData.StreamId, ct);
        if (!isValid)
        {
            return false;
        }
        
        Stream stream = (await _streamRepository.GetStreamById(streamData.StreamId, ct))!;
        stream.Url = streamData.Url;
        stream.StartTime = streamData.StartTime;
        stream.EndTime = streamData.EndTime;
        await _streamRepository.UpdateStream(stream, ct);
        return true;
    }

    public async Task<bool> DeleteStream(Guid streamId, CancellationToken ct)
    {
        bool isValid = await ValidateStreamById(streamId, ct);
        if (!isValid)
        {
            return false;
        }

        Stream stream = (await _streamRepository.GetStreamById(streamId, ct))!;
        await _streamRepository.DeleteStream(stream, ct);
        return true;
    }
    
    // Method only for Jobs
    public async Task UpdateStreamTimesAsync(CancellationToken ct)
    {
        var lastStream = await _streamRepository.GetLastStream(ct);

        if (lastStream != null)            
        {
            if (lastStream.EndTime.HasValue && lastStream.EndTime.Value < DateTimeOffset.UtcNow)
            {
                _logger.LogInformation("Updating stream times for stream {StreamId}", lastStream.StreamId);
                await _streamRepository.UpdateLastStream(lastStream, ct);
            }
        }
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateStreamById(StreamRequest? request, CancellationToken ct)
    {
        bool isValid = ValidationUtils.BasicWsValidation(request, _validationStorage);
        if (!isValid)
        {
            return false;
        }
        if (!await _streamRepository.StreamExists(request!.Id, ct))
        {
            ValidationUtils.AddUnknownStreamError(_validationStorage, request.Id);
        }
        return _validationStorage.IsValid;
    }
    
    private async Task<bool> ValidateStreamById(Guid streamId, CancellationToken ct)
    {
        if (!await _streamRepository.StreamExists(streamId, ct))
        {
            ValidationUtils.AddUnknownStreamError(_validationStorage, streamId);
        }
        return _validationStorage.IsValid;
    }

    #endregion

   
}