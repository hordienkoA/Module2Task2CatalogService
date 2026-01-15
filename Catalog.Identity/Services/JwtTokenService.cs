using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Catalog.Identity.Services
{
    public class JwtTokenService(IConfiguration config)
    {
        private readonly IConfiguration config = config;

        public string GenerateAccessToken(string userId, string role)
        {
            var claim = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.Role, role)
            };

            string jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claim,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(config["Jwt:AccessTokenMinutes"]!, System.Globalization.CultureInfo.InvariantCulture)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
