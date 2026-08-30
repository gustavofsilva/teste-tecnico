using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using UserProfile.Api.Data;
using UserProfile.Api.Domain;
using UserProfile.Api.Endpoints;
using UserProfile.Api.Security;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Configuração JWT ausente.");
if (Encoding.UTF8.GetByteCount(jwt.Key) < 32) throw new InvalidOperationException("Jwt:Key deve ter ao menos 32 bytes.");
if (jwt.ExpirationMinutes <= 0) throw new InvalidOperationException("Jwt:ExpirationMinutes deve ser maior que zero.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var databaseConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection ausente.");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(NormalizePostgreSqlConnection(databaseConnection), postgres =>
            postgres.EnableRetryOnFailure(maxRetryCount: 3));
        return;
    }

    if (!databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("DatabaseProvider deve ser Sqlite ou PostgreSql.");
    options.UseSqlite(databaseConnection);
});
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer, ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p.WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapUserEndpoints();
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
    if (databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        await database.MigrateAsync();
    else
        await database.EnsureCreatedAsync();
}
app.Run();

static string NormalizePostgreSqlConnection(string connection)
{
    if (!Uri.TryCreate(connection, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        return connection;

    var credentials = uri.UserInfo.Split(':', 2);
    if (credentials.Length != 2)
        throw new InvalidOperationException("A URL PostgreSQL deve conter usuário e senha.");

    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = SslMode.Require,
        Pooling = true
    }.ConnectionString;
}

public partial class Program;
