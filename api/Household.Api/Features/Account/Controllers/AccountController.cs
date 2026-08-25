using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Features.Account.Controllers;

[ApiController]
[Route("api/account")]
public sealed class AccountController(IAccountService accountService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await accountService.LoginAsync(request, cancellationToken);

        return response is null ? Unauthorized() : Ok(response);
    }
}
