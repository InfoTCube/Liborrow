using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.DTOs.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.IntegrationTests.Helpers;

public static class IntegrationTestHelpers
{
    public static HttpClient CreateClient(this WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public static async Task<UserDto> RegisterUserAsync(HttpClient client, 
        string username, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto
            {
                UserName = username,
                Email = email,
                Password = password
            });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public static HttpClient CreateAuthenticatedClient(this WebApplicationFactory<Program> factory, 
        UserDto user)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", user.Token);
        return client;
    }
}