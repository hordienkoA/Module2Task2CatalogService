using Catalog.Identity.Data;
using Catalog.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Catalog.Service.Api.DiConfiguration
{
    public static class IdentityMethods
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddHttpContextAccessor();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                    ),

                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
                };

                // debug events
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        Console.WriteLine("[Jwt] Token validated for: {0}", ctx.Principal?.Identity?.Name ?? "(no name)");
                        foreach (var c in ctx.Principal?.Claims ?? Array.Empty<System.Security.Claims.Claim>())
                        {
                            Console.WriteLine("[Jwt] Claim: {0} = {1}", c.Type, c.Value);
                        }
                        return System.Threading.Tasks.Task.CompletedTask;
                    },
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine("[Jwt] Authentication failed: {0}", ctx.Exception?.Message);
                        return System.Threading.Tasks.Task.CompletedTask;
                    },
                    OnChallenge = ctx =>
                    {
                        Console.WriteLine("[Jwt] OnChallenge: Error={0}, ErrorDescription={1}", ctx.Error, ctx.ErrorDescription);
                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                };
            });
            return services;
        }
    }
}
