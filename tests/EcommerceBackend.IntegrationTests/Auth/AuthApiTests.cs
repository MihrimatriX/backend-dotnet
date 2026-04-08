using System.Net;
using System.Net.Http.Json;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.IntegrationTests.Support;
using Xunit;

namespace EcommerceBackend.IntegrationTests.Auth;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class AuthApiTests
{
    private readonly HttpClient _client;

    public AuthApiTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Login_with_seeded_user_returns_bearer_token()
    {
        var request = new LoginRequestDto
        {
            Email = "user1@example.com",
            Password = "user123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request, TestJson.Options);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<BaseResponseDto<AuthResponseDto>>(TestJson.Options);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success, envelope.Message);
        Assert.NotNull(envelope.Data);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Data.Token));
        Assert.True(envelope.Data.UserId > 0);
    }

    [Fact]
    public async Task Login_with_bad_password_returns_unauthorized()
    {
        var request = new LoginRequestDto
        {
            Email = "user1@example.com",
            Password = "wrong-password"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request, TestJson.Options);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_new_user_then_login_succeeds()
    {
        var email = $"it-{Guid.NewGuid():N}@example.com";
        var register = new RegisterRequestDto
        {
            Email = email,
            Password = "Test123!",
            FirstName = "Integration",
            LastName = "Test"
        };

        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", register, TestJson.Options);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);
        var regEnvelope = await regResponse.Content.ReadFromJsonAsync<BaseResponseDto<AuthResponseDto>>(TestJson.Options);
        Assert.True(regEnvelope?.Success == true);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = email,
            Password = "Test123!"
        }, TestJson.Options);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginEnvelope = await loginResponse.Content.ReadFromJsonAsync<BaseResponseDto<AuthResponseDto>>(TestJson.Options);
        Assert.True(loginEnvelope?.Success == true);
        Assert.False(string.IsNullOrEmpty(loginEnvelope?.Data?.Token));
    }
}
