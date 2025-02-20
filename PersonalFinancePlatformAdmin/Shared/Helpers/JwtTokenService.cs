using Microsoft.IdentityModel.Tokens;
using PersonalFinancePlatformAdmin.Components.Account.Pages.Manage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PersonalFinancePlatformAdmin.Shared.Helpers;
public class JwtTokenService
{
    public string GenerateToken(string UserId, string Email)
    {
        byte[] key = Encoding.UTF8.GetBytes("PO+3e2/FL7y0Q3dmhqnTFzUnM+DTO4GMB/yWUhABMiM=");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, UserId),
            new Claim(JwtRegisteredClaimNames.Email, Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        JwtSecurityToken token = new (
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
