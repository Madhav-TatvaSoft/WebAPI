using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeDemoWebApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeDemoWebApi.Services;

public class JWTService
{
    private readonly string _secretKey;
    private readonly int _tokenDuration;

    public JWTService(IConfiguration configuration)
    {
        _secretKey = configuration.GetValue<string>("JwtConfig:Key")!;
        _tokenDuration = configuration.GetValue<int>("JwtConfig:Duration");
    }

    public string GenerateToken(User user)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[]
        {
            new Claim("email", user.Email),
            new Claim("role", user.Role.Name),
            new Claim(ClaimTypes.NameIdentifier,user.Username),
            new Claim("id",user.Id.ToString()),
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: "localhost",
            audience: "localhost",
            claims: claims,
            expires: DateTime.Now.AddHours(_tokenDuration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? GetClaimsFromToken(string token)
    {
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
        ClaimsIdentity claims = new ClaimsIdentity(jwtToken.Claims);
        return new ClaimsPrincipal(claims);
    }

    public string? GetClaimValue(string token, string claimType)
    {
        ClaimsPrincipal? claimsPrincipal = GetClaimsFromToken(token);
        string? value = claimsPrincipal?.FindFirst(claimType)?.Value;
        return value;
    }

    // public ClaimsPrincipal? ValidateToken(string token)
    // {
    //     try
    //     {
    //         var tokenHandler = new JwtSecurityTokenHandler();
    //         var key = Encoding.UTF8.GetBytes(_secretKey);

    //         var validationParameters = new TokenValidationParameters
    //         {
    //             ValidateIssuer = true,                  // Must match the issuer while generating token
    //             ValidateAudience = true,                // Must match the audience while generating token
    //             ValidateLifetime = true,                // Ensures the token hasn’t expired
    //             ValidateIssuerSigningKey = true,        // Validates the signing key
    //             ValidIssuer = "localhost",              // Same as in GenerateToken()
    //             ValidAudience = "localhost",            // Same as in GenerateToken()
    //             IssuerSigningKey = new SymmetricSecurityKey(key),
    //             ClockSkew = TimeSpan.Zero               // Optional: remove default 5 minutes grace period
    //         };

    //         // Validate token and return the claims principal
    //         var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

    //         // Additional safety: ensure token is a proper JWT
    //         if (validatedToken is JwtSecurityToken jwtToken &&
    //             jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
    //         {
    //             return principal;
    //         }

    //         return null;
    //     }
    //     catch
    //     {
    //         // Invalid token
    //         return null;
    //     }
    // }

}