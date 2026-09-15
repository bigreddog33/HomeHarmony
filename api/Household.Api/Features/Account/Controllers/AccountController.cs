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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var isValid = await accountService.LoginAsync(request, cancellationToken);

        return isValid ? Ok() : Unauthorized();
    }

    [AllowAnonymous]
    [HttpPost("createuser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateUser(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var isSuccess = await accountService.CreateUserAsync(request, cancellationToken);

        return isSuccess ? Ok() : Unauthorized();
    }

    [AllowAnonymous]
    [HttpPost("resendconfirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResendConfirmation(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        //Here will be the same function used for both sending and resending an email confirmation
        //since they both do the same thing, but from different locations
        var isSuccess = await accountService.SendConfirmationAsync(request, cancellationToken);

        return isSuccess ? Ok() : Unauthorized();
    }
}
