using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UserProfile.Api.Data;
using Xunit;

namespace UserProfile.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString = $"Data Source=tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    private readonly SqliteConnection _keepAlive;

    public ApiFactory()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "Sqlite",
                ["ConnectionStrings:DefaultConnection"] = _connectionString
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connectionString));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _keepAlive.Dispose();
    }
}

public sealed class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string JwtKey = "development-key-change-me-32-characters-minimum";
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Full_user_journey_succeeds()
    {
        var auth = await Register("Ana Silva", "ANA@example.com");
        Authenticate(auth.Token);
        Assert.Equal("ana@example.com", (await _client.GetFromJsonAsync<UserDto>("/api/profile"))?.Email);

        var update = await _client.PutAsJsonAsync("/api/profile", new
        {
            name = "Ana Souza", email = "ana@example.com", password = "newsecret", confirmPassword = "newsecret"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        Authenticate(null);
        Assert.Equal(HttpStatusCode.OK, (await Login("ana@example.com", "newsecret")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_email_and_invalid_credentials_return_expected_errors()
    {
        const string email = "bruno@example.com";
        await Register("Bruno Lima", email);

        var duplicate = await RegisterResponse("Bruno Lima", email);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("Já existe um usuário com este email.", await ReadMessage(duplicate));

        var invalidLogin = await Login(email, "wrong");
        Assert.Equal(HttpStatusCode.Unauthorized, invalidLogin.StatusCode);
        Assert.Equal("Email ou senha inválidos.", await ReadMessage(invalidLogin));
    }

    [Theory]
    [InlineData("  ", "valid@example.com", "secret1", "secret1", "name")]
    [InlineData("Valid Name", "not-an-email", "secret1", "secret1", "email")]
    [InlineData("Valid Name", "valid@example.com", "short", "short", "password")]
    [InlineData("Valid Name", "valid@example.com", "secret1", "different", "confirmPassword")]
    public async Task Invalid_registration_returns_validation_details(
        string name, string email, string password, string confirmation, string expectedField)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name, email, password, confirmPassword = confirmation
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await ReadErrors(response)).TryGetProperty(expectedField, out _));
    }

    [Theory]
    [InlineData("  ", "edited@example.com", "", "", "name")]
    [InlineData("Edited User", "not-an-email", "", "", "email")]
    [InlineData("Edited User", "edited@example.com", "short", "short", "password")]
    [InlineData("Edited User", "edited@example.com", "newsecret", "different", "confirmPassword")]
    public async Task Invalid_profile_update_returns_validation_details(
        string name, string email, string password, string confirmation, string expectedField)
    {
        var auth = await Register("Profile Owner", $"owner-{Guid.NewGuid():N}@example.com");
        Authenticate(auth.Token);
        var response = await _client.PutAsJsonAsync("/api/profile", new
        {
            name, email, password, confirmPassword = confirmation
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await ReadErrors(response)).TryGetProperty(expectedField, out _));
    }

    [Fact]
    public async Task Empty_password_keeps_the_existing_password()
    {
        var email = $"keep-password-{Guid.NewGuid():N}@example.com";
        var auth = await Register("Password Owner", email, "original-password");
        Authenticate(auth.Token);
        var update = await _client.PutAsJsonAsync("/api/profile", new
        {
            name = "Password Owner Updated", email, password = "", confirmPassword = ""
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        Authenticate(null);
        Assert.Equal(HttpStatusCode.OK, (await Login(email, "original-password")).StatusCode);
    }

    [Fact]
    public async Task Login_normalizes_email_case_and_surrounding_spaces()
    {
        var email = $"normalized-{Guid.NewGuid():N}@example.com";
        await Register("Normalized User", email);
        Assert.Equal(HttpStatusCode.OK, (await Login($"  {email.ToUpperInvariant()}  ", "secret1")).StatusCode);
    }

    [Fact]
    public async Task Profile_requires_a_valid_token()
    {
        Authenticate(null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/profile")).StatusCode);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("tampered")]
    public async Task Profile_rejects_invalid_jwt(string scenario)
    {
        var token = CreateToken(
            scenario == "issuer" ? "Invalid.Issuer" : "UserProfile.Api",
            scenario == "audience" ? "Invalid.Audience" : "UserProfile.Web",
            scenario == "expired" ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(5));
        if (scenario == "tampered") token = token[..^1] + (token[^1] == 'a' ? 'b' : 'a');
        Authenticate(token);

        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/profile")).StatusCode);
    }

    [Fact]
    public async Task Profile_cannot_use_another_users_email()
    {
        var firstEmail = $"first-{Guid.NewGuid():N}@example.com";
        await Register("First User", firstEmail);
        var second = await Register("Second User", $"second-{Guid.NewGuid():N}@example.com");
        Authenticate(second.Token);
        var update = await _client.PutAsJsonAsync("/api/profile", new
        {
            name = "Second User", email = firstEmail.ToUpperInvariant(), password = (string?)null, confirmPassword = (string?)null
        });

        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        Assert.Equal("Já existe um usuário com este email.", await ReadMessage(update));
    }

    [Fact]
    public async Task Concurrent_registration_allows_exactly_one_user_per_email()
    {
        var email = $"concurrent-{Guid.NewGuid():N}@example.com";
        using var firstClient = factory.CreateClient();
        using var secondClient = factory.CreateClient();
        var body = new { name = "Concurrent User", email, password = "secret1", confirmPassword = "secret1" };
        var responses = await Task.WhenAll(
            firstClient.PostAsJsonAsync("/api/auth/register", body),
            secondClient.PostAsJsonAsync("/api/auth/register", body));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Health_check_returns_expected_contract()
    {
        Authenticate(null);
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("healthy", json.RootElement.GetProperty("status").GetString());
    }

    private async Task<Auth> Register(string name, string email, string password = "secret1")
    {
        var response = await RegisterResponse(name, email, password);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Auth>())!;
    }

    private Task<HttpResponseMessage> RegisterResponse(string name, string email, string password = "secret1") =>
        _client.PostAsJsonAsync("/api/auth/register", new { name, email, password, confirmPassword = password });

    private Task<HttpResponseMessage> Login(string email, string password) =>
        _client.PostAsJsonAsync("/api/auth/login", new { email, password });

    private void Authenticate(string? token) =>
        _client.DefaultRequestHeaders.Authorization = token is null ? null : new AuthenticationHeaderValue("Bearer", token);

    private static async Task<string?> ReadMessage(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("message").GetString();
    }

    private static async Task<JsonElement> ReadErrors(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("errors").Clone();
    }

    private static string CreateToken(string issuer, string audience, DateTime expires)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience,
            [new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())],
            notBefore: expires.AddMinutes(-5), expires: expires, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record Auth(string Token, UserDto User);
    private sealed record UserDto(Guid Id, string Name, string Email);
}
