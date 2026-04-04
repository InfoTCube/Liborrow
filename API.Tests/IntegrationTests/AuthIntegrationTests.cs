using System.Net.Http.Json;
using API.DTOs.Auth;
using API.Tests.IntegrationTests.Fixtures;

namespace API.Tests.IntegrationTests;

[Collection("Integration")]
public class AuthIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _factory;

    public AuthIntegrationTests(IntegrationTestFixture factory) => _factory = factory;

    [Fact]
    public async Task Register_Login_Success()
    {
        // Arrange
        var client = _factory.Factory.CreateClient();
        var registerDto = new
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "Password123!"
        };

        var loginDto = new
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        };

        // Act
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        // Assert
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrEmpty(loginResult.Token));
        Assert.Equal("testuser", loginResult.UserName);
        Assert.Equal("testuser@example.com", loginResult.Email);
    }
}