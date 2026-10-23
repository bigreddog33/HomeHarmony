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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await accountService.LoginAsync(request,cancellationToken);

        if (result.Status == LoginStatus.Succeeded && request.Mode == LoginMode.Token)
            return new EmptyResult();

        var response = new LoginResponse(result.Status, result.LockoutEnd);

        if (result.Status == LoginStatus.Succeeded)
            return Ok(response);

        if (result.Status == LoginStatus.InvalidCredentials)
            return Unauthorized(response);

        if (result.Status == LoginStatus.EmailNotConfirmed || result.Status == LoginStatus.NotAllowed)
            return StatusCode(StatusCodes.Status403Forbidden, response);

        if (result.Status == LoginStatus.LockedOut)
            return StatusCode(StatusCodes.Status423Locked, response);

        if (result.Status == LoginStatus.RequiresTwoFactor)
            return Unauthorized(response);

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await accountService.LogoutAsync(cancellationToken);

        return NoContent();
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
                    result.Status,
                    EmailConfirmed: result.EmailConfirmed));

        if (result.Status == CreateUserStatus.ValidationFailed) return BadRequest(
                new CreateUserResponse(
                    result.Status,
                    result.IdentityResult!.Errors));

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [AllowAnonymous]
    [HttpPost("resendconfirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendConfirmation(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        //Here will be the same function used for both sending and resending an email confirmation
        //since they both do the same thing, but from different locations
        var result = await accountService.SendConfirmationAsync(request, cancellationToken);

        if (result == SendConfirmationStatus.Sent || result == SendConfirmationStatus.AlreadyConfirmed)
            return StatusCode(StatusCodes.Status200OK);

        if (result == SendConfirmationStatus.UserNotFound)
            return StatusCode(StatusCodes.Status404NotFound);

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> Refresh(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await accountService.RefreshAsync(request,cancellationToken);

        if (result == RefreshStatus.Succeeded)
            return new EmptyResult();

        if (result == RefreshStatus.InvalidRefreshToken)
            return Unauthorized(new RefreshResponse(result));

        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
