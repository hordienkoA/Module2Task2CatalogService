namespace Catalog.Identity.Data
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Email { get; set; }

        // Optional requested role. If not provided, user gets the default role ("Store customer")..
        public string? Role { get; set; }
    }
}
