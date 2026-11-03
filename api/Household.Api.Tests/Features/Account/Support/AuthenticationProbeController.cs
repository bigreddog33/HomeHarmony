using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Support;

// Registered only by IdentityAccountApiFactory; no production endpoint is added.
[ApiController]
[Authorize]
[Route("test/authentication")]
public sealed class AuthenticationProbeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
}
