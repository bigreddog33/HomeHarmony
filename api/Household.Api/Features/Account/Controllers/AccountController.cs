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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await accountService.CreateUserAsync(request, cancellationToken);

        if (result.Status == CreateUserStatus.Created) return StatusCode(
                StatusCodes.Status201Created,
                new CreateUserResponse(
                    result.Status));

        if (result.Status == CreateUserStatus.CreatedConfirmationFailed) return StatusCode(
                StatusCodes.Status201Created,
                new CreateUserResponse(
                    result.Status));

        if (result.Status == CreateUserStatus.AlreadyExists) return Conflict(
                new CreateUserResponse(
                    result.Status));

        if (result.Status == CreateUserStatus.ValidationFailed) return BadRequest(
                new CreateUserResponse(
                    result.Status,
                    result.IdentityResult!.Errors));

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [AllowAnonymous]
    [HttpPost("resendconfirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ResendConfirmation(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        //Here will be the same function used for both sending and resending an email confirmation
        //since they both do the same thing, but from different locations
        var result = await accountService.SendConfirmationAsync(request, cancellationToken);

        if (result==SendConfirmationStatus.Sent) return StatusCode(StatusCodes.Status200OK);
        
        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
