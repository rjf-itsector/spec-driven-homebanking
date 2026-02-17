using System.Net.Http.Json;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;

namespace HomeBanking.API.Tests;

public static class TestHelpers
{
    public static async Task<HttpClient> GetAuthenticatedClientAsync(TestWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("demo@bank.com", "Demo123!"));
        loginResponse.EnsureSuccessStatusCode();
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result!.Token);
        return client;
    }
}
