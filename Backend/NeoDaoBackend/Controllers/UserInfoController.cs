using Microsoft.AspNetCore.Mvc;
using NeoDaoBackend.Filters;
using NeoDaoBackend.Models.UserData;
using NeoDaoBackend.Service;
using NeoDaoBackend.Validation;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Controllers;

[ApiController]
[Route("api/public")]
public class UserInfoController : BaseController
{
    private readonly UserDataService _userDataService;

    public UserInfoController(UserDataService userInfoService, IValidationStorage validationStorage) : base(validationStorage)
    {
        _userDataService = userInfoService;
    }

    [ServerAuthorized]
    [HttpDelete("deleteUserData")]
    public async Task<IActionResult> DeleteUserData([FromQuery, ValidGuid] Guid userId, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userDataService.DeleteUser(userId, token), ct);
    }

    [ServerAuthorized]
    [HttpPut("updateUserName")]
    public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameRequest request, CancellationToken ct)
    {
        return await HandleRequestAsync(async token => await _userDataService.UpdateUserName(request, token), ct);
    }
}