using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTO;
using minimal_api.Domain.Interface;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Service;
using minimal_api.Infraestrutura.Db;

namespace minimal_api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddScoped<IAdministradorService, AdministradorService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<DBContexto>(options =>
        {
            options.UseMySql(
                builder.Configuration.GetConnectionString("mysql"), 
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
                );
        });

        var app = builder.Build();

        app.MapGet("/", () => Results.Json(new Home()));

        app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
        {
            if (administradorService.Login(loginDTO) != null)
            {
                return Results.Ok("Login com sucesso");
            }
            else
            {
                return Results.Unauthorized();
            }
        });

        app.UseSwagger();
        app.UseSwaggerUI();

        app.Run();
    }

    
}
