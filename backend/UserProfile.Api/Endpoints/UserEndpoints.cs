using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserProfile.Api.Contracts;
using UserProfile.Api.Data;
using UserProfile.Api.Domain;
using UserProfile.Api.Security;

namespace UserProfile.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Authentication");
        auth.MapPost("/register", Register).AddEndpointFilter<ValidationFilter<RegisterRequest>>();
        auth.MapPost("/login", Login).AddEndpointFilter<ValidationFilter<LoginRequest>>();
        var profile = app.MapGroup("/api/profile").RequireAuthorization().WithTags("Profile");
        profile.MapGet("/", GetProfile);
        profile.MapPut("/", UpdateProfile).AddEndpointFilter<ValidationFilter<UpdateProfileRequest>>();
        return app;
    }

    private static async Task<IResult> Register(RegisterRequest request, AppDbContext db,
        IPasswordHasher<User> hasher, TokenService tokens, CancellationToken ct)
    {
        if (request.Name.Trim().Length < 3)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["O nome deve ter no mínimo 3 caracteres úteis."] });
        if (request.Password != request.ConfirmPassword)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["confirmPassword"] = ["As senhas não coincidem."] });
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            return Results.Conflict(new MessageResponse("Já existe um usuário com este email."));
        var user = new User { Name = request.Name.Trim(), Email = email, PasswordHash = string.Empty };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        try { await SaveChangesWithSqliteContentionRetry(db, ct); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Results.Conflict(new MessageResponse("Já existe um usuário com este email."));
        }
        return Results.Created("/api/profile", new AuthResponse(tokens.Create(user), ToResponse(user)));
    }

    private static async Task<IResult> Login(LoginRequest request, AppDbContext db,
        IPasswordHasher<User> hasher, TokenService tokens, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null)
            return Results.Json(new MessageResponse("Email ou senha inválidos."), statusCode: StatusCodes.Status401Unauthorized);
        var passwordResult = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
            return Results.Json(new MessageResponse("Email ou senha inválidos."), statusCode: StatusCodes.Status401Unauthorized);
        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, request.Password);
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new AuthResponse(tokens.Create(user), ToResponse(user)));
    }

    private static async Task<IResult> GetProfile(ClaimsPrincipal principal, AppDbContext db, CancellationToken ct)
    {
        var user = await FindCurrentUser(principal, db, ct);
        return user is null ? Results.Unauthorized() : Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> UpdateProfile(UpdateProfileRequest request, ClaimsPrincipal principal,
        AppDbContext db, IPasswordHasher<User> hasher, CancellationToken ct)
    {
        var user = await FindCurrentUser(principal, db, ct);
        if (user is null) return Results.Unauthorized();
        if (request.Name.Trim().Length < 3)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["O nome deve ter no mínimo 3 caracteres úteis."] });
        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 6)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["password"] = ["A senha deve ter no mínimo 6 caracteres."] });
        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password != request.ConfirmPassword)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["confirmPassword"] = ["As senhas não coincidem."] });
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email && x.Id != user.Id, ct))
            return Results.Conflict(new MessageResponse("Já existe um usuário com este email."));
        user.Name = request.Name.Trim();
        user.Email = email;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = hasher.HashPassword(user, request.Password);
        try { await SaveChangesWithSqliteContentionRetry(db, ct); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Results.Conflict(new MessageResponse("Já existe um usuário com este email."));
        }
        return Results.Ok(ToResponse(user));
    }

    private static async Task SaveChangesWithSqliteContentionRetry(AppDbContext db, CancellationToken ct)
    {
        const int maximumAttempts = 3;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.SaveChangesAsync(ct);
                return;
            }
            catch (DbUpdateException ex) when (
                attempt < maximumAttempts && ex.InnerException is SqliteException { SqliteErrorCode: 5 or 6 })
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), ct);
            }
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) => exception.InnerException switch
    {
        SqliteException { SqliteErrorCode: 19 } => true,
        PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } => true,
        _ => false
    };

    private static async Task<User?> FindCurrentUser(ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? await db.Users.FindAsync([id], ct) : null;
    private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email);
}
