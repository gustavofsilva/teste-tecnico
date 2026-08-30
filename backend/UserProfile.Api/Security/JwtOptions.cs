namespace UserProfile.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public required string Key { get; init; }
    public string Issuer { get; init; } = "UserProfile.Api";
    public string Audience { get; init; } = "UserProfile.Web";
    public int ExpirationMinutes { get; init; } = 60;
}
