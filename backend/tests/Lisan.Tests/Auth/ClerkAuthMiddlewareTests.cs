using System.Net;
using FluentAssertions;

namespace Lisan.Tests.Auth;

public class ClerkAuthMiddlewareTests : IClassFixture<LisanApiFactory>
{
    private readonly HttpClient _client;

    public ClerkAuthMiddlewareTests(LisanApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsNonUnauthorized_WithNoToken()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_Returns401_WithNoToken()
    {
        var response = await _client.GetAsync("/api/ping");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_Returns401_WithInvalidBearerToken()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "forged.token.value");

        var response = await _client.GetAsync("/api/ping");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_Returns200_WithValidToken()
    {
        _client.DefaultRequestHeaders.Add(TestAuthHandler.AuthHeaderName, "true");

        var response = await _client.GetAsync("/api/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
