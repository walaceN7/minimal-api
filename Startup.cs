using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTO;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Interface;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Service;
using minimal_api.Infraestrutura.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace minimal_api
{
    public class Startup
    {
        private readonly string key = string.Empty;

        public IConfiguration Configuration { get; set; } = default!;

        public Startup(IConfiguration configuration)
        {            
            Configuration = configuration;
            key = Configuration?.GetSection("Jwt")?.ToString() ?? "";            
        }        

        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization();

            services.AddScoped<IAdministradorService, AdministradorService>();
            services.AddScoped<IVeiculosService, VeiculoService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT aqui"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }

                });
            });

            services.AddDbContext<DBContexto>(options =>
            {
                options.UseMySql(
                    Configuration.GetConnectionString("MySql"),
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
                    );
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region Administradores
                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
                {
                    var adm = administradorService.Login(loginDTO);
                    if (adm != null)
                    {
                        string token = GerarTokenJwt(adm, key);
                        return Results.Ok(new AdministradorLogado
                        {
                            Email = adm.Email,
                            Perfil = adm.Perfil,
                            Token = token
                        });
                    }
                    else
                    {
                        return Results.Unauthorized();
                    }
                }).AllowAnonymous().WithTags("Administradores");

                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) =>
                {
                    var validacao = new ErrosDeValidacao
                    {
                        Mensagens = new List<string>()
                    };

                    if (string.IsNullOrWhiteSpace(administradorDTO.Email))
                    {
                        validacao.Mensagens.Add("Email não pode ser vazio");
                    }

                    if (string.IsNullOrWhiteSpace(administradorDTO.Senha))
                    {
                        validacao.Mensagens.Add("Senha não pode ser vazia");
                    }

                    if (administradorDTO.Perfil == null)
                    {
                        validacao.Mensagens.Add("Perfil não pode ser vazio");
                    }

                    if (validacao.Mensagens.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    var adm = new Administrador
                    {
                        Email = administradorDTO.Email,
                        Senha = administradorDTO.Senha,
                        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                    };

                    administradorService.Incluir(adm);

                    return Results.Created($"/administrador/{adm.ID}", adm);

                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

                endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
                {
                    var adms = new List<AdministradorModelView>();
                    var administradores = administradorService.Todos(pagina);

                    foreach (var adm in administradores)
                    {
                        adms.Add(new AdministradorModelView
                        {
                            Id = adm.ID,
                            Email = adm.Email,
                            Perfil = adm.Perfil
                        });
                    }
                    return Results.Ok(adms);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

                endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
                {
                    var administrador = administradorService.BuscarPorId(id);

                    if (administrador == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(new AdministradorModelView
                    {
                        Id = administrador.ID,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");
                #endregion

                #region Veiculos        
                endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculosService veiculoService) =>
                {
                    var validacao = ValidaDTO(veiculoDTO);
                    if (validacao.Mensagens.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    var veiculo = new Veiculo
                    {
                        Nome = veiculoDTO.Nome,
                        Marca = veiculoDTO.Marca,
                        Ano = veiculoDTO.Ano
                    };

                    veiculoService.Incluir(veiculo);

                    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veiculos");

                endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculosService veiculoService) =>
                {
                    var veiculos = veiculoService.Todos(pagina);

                    return Results.Ok(veiculos);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veiculos");

                endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculosService veiculoService) =>
                {
                    var veiculo = veiculoService.BuscaPorId(id);

                    if (veiculo == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(veiculo);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veiculos");

                endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculosService veiculoService) =>
                {
                    var veiculo = veiculoService.BuscaPorId(id);
                    if (veiculo == null)
                    {
                        return Results.NotFound();
                    }

                    var validacao = ValidaDTO(veiculoDTO);
                    if (validacao.Mensagens.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    veiculo.Nome = veiculoDTO.Nome;
                    veiculo.Marca = veiculoDTO.Marca;
                    veiculo.Ano = veiculoDTO.Ano;

                    veiculoService.Atualizar(veiculo);

                    return Results.Ok(veiculo);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");

                endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculosService veiculoService) =>
                {
                    var veiculo = veiculoService.BuscaPorId(id);

                    if (veiculo == null)
                    {
                        return Results.NotFound();
                    }

                    veiculoService.Apagar(veiculo);

                    return Results.NoContent();
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");
                #endregion                    
            });              
        }

        public static ErrosDeValidacao ValidaDTO(VeiculoDTO veiculoDTO)
        {
            var validacao = new ErrosDeValidacao
            {
                Mensagens = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(veiculoDTO.Nome))
            {
                validacao.Mensagens.Add("O nome não pode ser vazio");
            }

            if (string.IsNullOrWhiteSpace(veiculoDTO.Marca))
            {
                validacao.Mensagens.Add("A marca não pode ficar em branco");
            }

            if (veiculoDTO.Ano < 1950)
            {
                validacao.Mensagens.Add("Veículo muito antigo, aceito somente anos acima de 1950");
            }

            return validacao;
        }

        public static string GerarTokenJwt(Administrador administrador, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
                    {
                        new Claim("Email", administrador.Email),
                        new Claim("Perfil", administrador.Perfil),
                        new Claim(ClaimTypes.Role, administrador.Perfil)
                    };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
