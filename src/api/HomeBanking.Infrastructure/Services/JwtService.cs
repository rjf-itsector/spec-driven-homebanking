using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HomeBanking.Core.Interfaces;

namespace HomeBanking.Infrastructure.Services;

/// <summary>
/// JWT token generation and validation service.
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "HomeBankingAPI";
    }

    public string GenerateToken(Guid userId, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = _issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Guid? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

            return Guid.Parse(userIdClaim);
        }
        catch
        {
            return null;
        }
    }
}
