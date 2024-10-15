using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enum;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
    }

    private string key = "";
    public IConfiguration Configuration { get; set; } = default!;

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
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IVehicleService, VehicleService>();

        services.AddEndpointsApiExplorer();
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
                    new OpenApiSecurityScheme{
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

        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
            );
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home()))
            .AllowAnonymous()
            .WithTags("Home");
            #endregion

            #region Administradores
            string TokenJwtGeneration(Administrator administrator)
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrator.Email),
                    new Claim("Perfil", administrator.Perfil),
                    new Claim(ClaimTypes.Role, administrator.Perfil),
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService AdministratorService) =>
            {
                var adm = AdministratorService.Login(loginDTO);
                if (adm != null)
                {
                    string token = TokenJwtGeneration(adm);
                    return Results.Ok(new LoggedAdministratorDTO
                    {
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });
                }
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administradores");

            endpoints.MapGet("/administrators", ([FromQuery] int? pagina, IAdministratorService AdministratorService) =>
            {
                var adms = new List<AdministratorModelView>();
                var administrators = AdministratorService.GetAll(pagina);
                foreach (var adm in administrators)
                {
                    adms.Add(new AdministratorModelView
                    {
                        Id = adm.Id,
                        Email = adm.Email,
                        Perfil = adm.Perfil
                    });
                }
                return Results.Ok(adms);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapGet("/Administrators/{id}", ([FromRoute] int id, IAdministratorService AdministratorService) =>
            {
                var administrador = AdministratorService.GetById(id);
                if (administrador == null) return Results.NotFound();
                return Results.Ok(new AdministratorModelView
                {
                    Id = administrador.Id,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil
                });
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapPost("/administrators", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService AdministratorService) =>
            {
                var validationError = new ValidationError();

                if (string.IsNullOrEmpty(administratorDTO.Email))
                    validationError.Messages.Add("Email não pode ser vazio");
                if (string.IsNullOrEmpty(administratorDTO.Senha))
                    validationError.Messages.Add("Senha não pode ser vazia");
                if (administratorDTO.Perfil == null)
                    validationError.Messages.Add("Perfil não pode ser vazio");

                if (validationError.Messages.Count > 0)
                    return Results.BadRequest(validationError);

                var administrador = new Administrator
                {
                    Email = administratorDTO.Email,
                    Senha = administratorDTO.Senha,
                    Perfil = administratorDTO.Perfil.ToString() ?? PerfilEnum.Editor.ToString()
                };

                AdministratorService.Add(administrador);

                return Results.Created($"/administrator/{administrador.Id}", new AdministratorModelView
                {
                    Id = administrador.Id,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil
                });

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapPut("/administrator/{id}",
                ([FromRoute] int id, [FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
                {
                    var administrator = administratorService.GetById(id);

                    if (administrator == null) return Results.NotFound("Administrador não encontrado");

                    administrator.Email = administratorDTO.Email;
                    administrator.Perfil = administratorDTO.Perfil.ToString() ?? "Editor";

                    administratorService.Update(administrator);

                    return Results.Ok(administrator);
                })
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administradores");

            endpoints.MapDelete("/administrator/{id}",
                ([FromRoute] int id, IAdministratorService administratorService) =>
                {
                    var administrator = administratorService.GetById(id);

                    if (administrator == null) return Results.NotFound("Administrador não encontrado");

                    administratorService.Delete(administrator);

                    return Results.NoContent(); // Retorna 204 No Content após a exclusão bem-sucedida
                })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            #endregion

            #region Veículos
            endpoints.MapPost("/vehicles", (
                    [FromBody] VehicleDTO vehicleDTO,
                    IVehicleService vehicleService) =>
            {
                var validationError = VehicleValidation(vehicleDTO);

                if (validationError.Messages.Count > 0)
                    return Results.BadRequest(validationError);

                var vehicle = new Vehicle
                {
                    Name = vehicleDTO.Name,
                    Brand = vehicleDTO.Brand,
                    Year = vehicleDTO.Year
                };

                vehicleService.Add(vehicle);

                return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veículos");

            endpoints.MapGet("/vehicles",
                ([FromQuery] int? page, IVehicleService vehicleService) =>
                {
                    var vehicles = vehicleService.GetAll(page);
                    return Results.Ok(vehicles);
                }

            )
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veículos");

            endpoints.MapGet("/vehicles/{id}",
                ([FromRoute] int id, IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.GetById(id);

                    if (vehicle == null) return Results.NotFound("Veículo não encontrado");
                    return Results.Ok(vehicle);
                }

            )
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veículos");

            endpoints.MapPut("/vehicles/{id}",
                ([FromRoute]
                    int id,
                    VehicleDTO vehicleDTO,
                    IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.GetById(id);

                    if (vehicle == null) return Results.NotFound("Veículo não encontrado");

                    var validationError = VehicleValidation(vehicleDTO);

                    if (validationError.Messages.Count > 0)
                        return Results.BadRequest(validationError);

                    vehicle.Name = vehicleDTO.Name;
                    vehicle.Brand = vehicleDTO.Brand;
                    vehicle.Year = vehicleDTO.Year;

                    vehicleService.Update(vehicle);

                    return Results.Ok(vehicle);
                }

            )
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veículos");

            endpoints.MapDelete("/vehicles/{id}",
                ([FromRoute]
                    int id,
                    IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.GetById(id);

                    if (vehicle == null) return Results.NotFound("Veículo não encontrado");

                    vehicleService.Delete(vehicle);

                    return Results.NoContent();
                }


            )
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veículos");

            // Valida informações do veículo
            ValidationError VehicleValidation(VehicleDTO vehicle)
            {
                var validation = new ValidationError();
                if (vehicle == null)
                {
                    validation.Messages.Add("Veículo sem informações");
                    return validation;
                }

                if (string.IsNullOrEmpty(vehicle.Name))
                    validation.Messages.Add("Nome do veículo não informado!");

                if (string.IsNullOrEmpty(vehicle.Brand))
                    validation.Messages.Add("Marca do veículo não informado");

                if (vehicle.Year < 1950)
                    validation.Messages.Add("Veículo muito antigo!");

                return validation;

            };

            #endregion
        });
    }
}