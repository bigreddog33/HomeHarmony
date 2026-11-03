using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Net.Http.Headers;

namespace Household.Api.Tests.Features.Account.Support;

internal static class AuthenticationAssertions
{
    public static IList<SetCookieHeaderValue> Cookies(HttpResponseMessage response) =>
        SetCookieHeaderValue.ParseList(response.Headers.TryGetValues("Set-Cookie", out var headers)
            ? headers.ToList() : []);

    public static async Task<JsonElement> FailureAsync(HttpResponseMessage response,
        HttpStatusCode expectedCode, string expectedStatus, string cookieName)
    {
        Assert.Equal(expectedCode, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedStatus, body.GetProperty("status").GetString());
        Assert.False(body.TryGetProperty("accessToken", out _));
        Assert.False(body.TryGetProperty("refreshToken", out _));
        Assert.DoesNotContain(Cookies(response), cookie => cookie.Name.Value == cookieName);
        return body;
    }

    public static async Task AuthenticatedAsync(HttpClient client, string userId, string? accessToken = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, IdentityAccountApiFactory.ProtectedPath);
        if (accessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(userId, body.GetProperty("userId").GetString());
    }

    public static async Task UnauthorizedAsync(HttpClient client, string? accessToken = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, IdentityAccountApiFactory.ProtectedPath);
        if (accessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }
}
