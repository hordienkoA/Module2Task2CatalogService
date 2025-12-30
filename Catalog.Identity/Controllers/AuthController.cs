using Catalog.Identity.Data;
using Catalog.Identity.Models;
using Catalog.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Identity.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController: ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtTokenService _jwt;
        private readonly AuthDbContext _db;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<IdentityUser> userManager,
            JwtTokenService jwt,
            AuthDbContext db,
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _jwt = jwt;
            _db = db;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if(user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized();
            var role = (await _userManager.GetRolesAsync(user)).First();
            var accessToken = _jwt.GenerateAccessToken(user.Id, role);

            var refresh = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return Ok(new { accessToken, refresh = refresh.Token });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(string refreshToken)
        {
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(t =>
            t.Token == refreshToken && !t.IsRevoked && t.Expires > DateTime.UtcNow);

            if(token == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(token.UserId);
            var role = (await _userManager.GetRolesAsync(user!)).First();
            
            return Ok(new 
            {
                accessToken = _jwt.GenerateAccessToken(user!.Id, role)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {

            var (success, accessToken, refreshToken, errors) = await _authService.RegisterAsync(dto);
            if (!success)
                return BadRequest(new { Errors = errors });
            return Ok(new { accessToken, refresh = refreshToken });
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AssignRole(AssignRoleDto dto)
        {
            var (success, errors) = await _authService.AssignRoleAsync(dto.Username, dto.Role);
            if (!success)
                return BadRequest(new { Errors = errors });
            return Ok();
        }
    }
}
