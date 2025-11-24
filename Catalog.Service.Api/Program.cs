using CartService.BLL.Interfaces;
using CartService.BLL.Services;
using Catalog.DAL;
using Catalog.Service.Api.DiConfiguration;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var configuration = builder.Configuration;

        builder.Services.AddDb(configuration);
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

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
    
}

