using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_>=_64chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ECNManager";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ECNClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

var usersPath = Path.Combine(AppContext.BaseDirectory, "ECN.Users.json");
var users = new List<UserRecord>();
if (File.Exists(usersPath))
{
    var json = await File.ReadAllTextAsync(usersPath);
    users = System.Text.Json.JsonSerializer.Deserialize<List<UserRecord>>(json) ?? new();
}

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { ok = true, ts = DateTime.UtcNow }));

app.MapPost("/api/login", (LoginDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("Missing username or password");

    var match = users.FirstOrDefault(u =>
        string.Equals(u.Id, dto.Username, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(u.Email, dto.Username, StringComparison.OrdinalIgnoreCase));

    if (match is null) return Results.Unauthorized();

    if (!BCrypt.Net.BCrypt.Verify(dto.Password, match.PasswordHash))
        return Results.Unauthorized();

    var claims = new[] {
        new Claim(ClaimTypes.NameIdentifier, match.Id),
        new Claim(ClaimTypes.Name, match.Name),
        new Claim("dept", match.Dept),
        new Claim(ClaimTypes.Role, match.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds
    );

    var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new {
        token = tokenString,
        user = new { match.Id, match.Name, match.Email, match.Dept, match.Role }
    });
});

app.MapGet("/api/me", [Authorize] (ClaimsPrincipal user) =>
{
    var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    var name = user.FindFirstValue(ClaimTypes.Name) ?? "";
    var dept = user.FindFirst("dept")?.Value ?? "";
    var role = user.FindFirstValue(ClaimTypes.Role) ?? "";
    var email = "";
    return Results.Ok(new { id, name, email, dept, role });
});

app.Run();

record LoginDto(string Username, string Password, string? Dept);
class UserRecord
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Dept { get; set; } = "";
    public string Role { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}
