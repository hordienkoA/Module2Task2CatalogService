using Catalog.Identity.Data;

namespace Catalog.Identity.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? AccessToken, string? RefreshToken, IEnumerable<string>? Errors)> RegisterAsync(RegisterDto dto);
        Task<(bool Success, IEnumerable<string>? Errors)> AssignRoleAsync(string username, string role);

    }
}
