using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Writers;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.Chat;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class ChatController : BaseController
{
    private readonly ChatService _chatService;
    public ChatController(IValidationStorage validationStorage, ChatService chatService) : base(validationStorage)
    {
        _chatService = chatService;
    }

    [ServerAuthorized]
    [HttpPost("sendAdminMessage")]
    public async Task<IActionResult> SendAdminMessage([FromBody] string message, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _chatService.SendAdminMessage(message, token), ct);
    }

    [ServerAuthorized]
    [HttpGet("getGreetingMessage")]
    public async Task<IActionResult> GetGreetingMessage(CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _chatService.GetGreetingMessage(token), ct);
    }

    [ServerAuthorized]
    [HttpPut("updateGreetingMessage")]
    public async Task<IActionResult> UpdateGreetingMessage([FromBody] string message, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _chatService.UpdateGreetingMessage(message, token), ct);
    }

    [ServerAuthorized]
    [HttpPost("sendMetaMessage")]
    public async Task<IActionResult> SendMetaMessage([FromBody] ChatMessageToUserRequest message, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _chatService.SendMetaMessage(message, token), ct);
    }
}
