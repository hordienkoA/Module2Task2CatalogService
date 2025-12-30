using CartService.BLL.Interfaces;
using CartService.BLL.Services;
using Catalog.Service.Api.DiConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var configuration = builder.Configuration;

        builder.Services.AddDb(configuration);


        builder.Services.AddIdentityConfiguration(configuration);

        builder.Services.AddRepositories();
        builder.Services.AddUow();
        builder.Services.AddSingleton<IRabbitMqPublisher>(sp =>
        {
            return new RabbitMqPublisher(
                configuration.GetValue<string>("RabbitMq:Host"),
                configuration.GetValue<string>("RabbitMq:UserName"),
                configuration.GetValue<string>("RabbitMq:Password"));
        });
        builder.Services.AddServices();
        

        builder.Services.AddAuthorization();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Swagger + JWT authorization button
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" });

            var jwtScheme = new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "Enter 'Bearer' [space] and then your JWT token.",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            c.AddSecurityDefinition("Bearer", jwtScheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtScheme, new string[] { } }
            });
        });

        var app = builder.Build();

        // Ensure predefined roles exist
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Manager", "Store customer" };
            foreach (var role in roles)
            {
                var exists = roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();
                if (!exists)
                {
                    roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
    
}

