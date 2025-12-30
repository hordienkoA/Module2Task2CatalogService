using Catalog.Identity.Data;
using Catalog.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Catalog.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwt;
        private readonly AuthDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        private const string DefaultRole = "Store customer";
        private const string ManagerRole = "Manager";

        public AuthService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            JwtTokenService jwt,
            AuthDbContext db,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt;
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, IEnumerable<string>? Errors)> RegisterAsync(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return (false, null, null, new[] { "Username and password are required." });

            var requestedRole = string.IsNullOrWhiteSpace(dto.Role) ? DefaultRole : dto.Role!.Trim();

            if (!string.Equals(requestedRole, DefaultRole, StringComparison.OrdinalIgnoreCase))
            {
                var callerIsManager = IsCallerManagerAsync();
                if (!callerIsManager)
                    return (false, null, null, new[] { "Insufficient permissions to assign the requested role." });
            }

            var existing = await _userManager.FindByNameAsync(dto.Username);
            if (existing != null)
                return (false, null, null, new[] { "Username already exists." });

            var user = new IdentityUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description);
                return (false, null, null, errors);
            }

            var rolesToEnsure = new[] { DefaultRole, ManagerRole };
            foreach (var r in rolesToEnsure)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                {
                    await _roleManager.CreateAsync(new IdentityRole(r));
                }
            }

            if (!await _roleManager.RoleExistsAsync(requestedRole))
            {
                await _userManager.DeleteAsync(user);
                return (false, null, null, new[] { "Requested role does not exist." });
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, requestedRole);
            if (!addToRoleResult.Succeeded)
            {
                var errors = addToRoleResult.Errors.Select(e => e.Description);
                await _userManager.DeleteAsync(user);
                return (false, null, null, errors);
            }

            var accessToken = _jwt.GenerateAccessToken(user.Id, requestedRole);
            var refresh = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return (true, accessToken, refresh.Token, null);
        }

        private bool IsCallerManagerAsync()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
                return false;

            var principal = ctx.User;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                if (principal.IsInRole(ManagerRole))
                    return true;

                var roleClaimTypes = new[] { ClaimTypes.Role, "role", "roles", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" };
                if (principal.Claims.Any(c => roleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase) &&
                                              string.Equals(c.Value, ManagerRole, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            // Try to read Authorization header and validate token manually
            if (!ctx.Request.Headers.TryGetValue("Authorization", out var authHeaders))
                return false;

            var authHeader = authHeaders.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader))
                return false;

            // Accept both "Bearer <token>" and raw token
            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader.Substring("Bearer ".Length).Trim()
                : authHeader.Trim();

            if (string.IsNullOrEmpty(token))
                return false;

            var key = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                return false;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                // avoid automatic claim mapping to keep claim types predictable
                handler.InboundClaimTypeMap.Clear();

                var principalFromToken = handler.ValidateToken(token, validationParameters, out _);
                if (principalFromToken == null)
                    return false;

                if (principalFromToken.IsInRole(ManagerRole))
                    return true;

                var roleClaimTypes = new[] { ClaimTypes.Role, "role", "roles", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" };
                if (principalFromToken.Claims.Any(c => roleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase) &&
                                                       string.Equals(c.Value, ManagerRole, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public async Task<(bool Success, IEnumerable<string>? Errors)> AssignRoleAsync(string username, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
                return (false, new[] { "Username and role are required." });

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return (false, new[] { "User not found." });

            var normalizedRole = role.Trim();

            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
                if (!createRoleResult.Succeeded)
                    return (false, createRoleResult.Errors.Select(e => e.Description));
            }

            if (await _userManager.IsInRoleAsync(user, normalizedRole))
                return (true, null);

            var addResult = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (!addResult.Succeeded)
                return (false, addResult.Errors.Select(e => e.Description));

            return (true, null);
        }
    }
}
