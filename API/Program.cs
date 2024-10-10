using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;
using proj_minimal_api.Domain.DTOs;
using proj_minimal_api.Domain.Entities;
using proj_minimal_api.Domain.Enum;
using proj_minimal_api.Domain.Interfaces;
using proj_minimal_api.Domain.ModelViews;
using proj_minimal_api.Domain.Services;
{

}
#region Builder
var builder = WebApplication.CreateBuilder(args);

//Adição da autenticação JWTBearer
var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "12345";

builder.Services.AddAuthentication(option =>
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

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

//Adição Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    //Configuração da assinatura JWT
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui."
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddDbContext<DbContexto>(
    options =>
    {
        options.UseMySql(
            builder.Configuration.GetConnectionString("Mysql"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Mysql"))
        );
    }
);

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home()))
.AllowAnonymous()
.WithTags("Home");
#endregion

#region Administradores
string TokenJwtGeneration(Administrator administrator)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim("Email", administrator.Email),
        new Claim("Perfil", administrator.Perfil),
        new Claim(ClaimTypes.Role, administrator.Perfil),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credential
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    var adm = administratorService.Login(loginDTO);

    if (adm != null)
    {
        string toke = TokenJwtGeneration(adm);
        return Results.Ok(new LoggedAdministratorDTO
        {
            Email = adm.Email,
            Profile = adm.Perfil,
            Token = toke
        });
    }
    else
        return Results.Unauthorized();

})
.AllowAnonymous()
.WithTags("Administradores");

app.MapGet("/administrators", ([FromQuery] int? page, IAdministratorService administratorService) =>
{
    var adms = new List<AdministratorModelView>();
    var administrador = administratorService.GetAll(page);

    foreach (var adm in administrador)
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

app.MapGet("/administrator/{id}",
    ([FromRoute]
       int id,
        IAdministratorService administratorService) =>
    {
        var administrator = administratorService.GetById(id);

        if (administrator == null) return Results.NotFound("Veículo não encontrado");
        var administratorModel = new AdministratorModelView
        {
            Id = administrator.Id,
            Email = administrator.Email,
            Perfil = administrator.Perfil
        };

        return Results.Ok(administratorModel);
    }

)
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");

app.MapPost("/administrators", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
{
    var validationError = new ValidationError();

    if (string.IsNullOrEmpty(administratorDTO.Email))
        validationError.Messages.Add("Email não pode ser vazio.");

    if (string.IsNullOrEmpty(administratorDTO.Senha))
        validationError.Messages.Add("Senha não pode ser vazia.");

    if (administratorDTO.Perfil == null)
        validationError.Messages.Add("Perfil não pode ser vazio.");

    if (validationError.Messages.Count > 0)
        return Results.BadRequest(validationError);

    var administrator = new Administrator
    {
        Email = administratorDTO.Email,
        Senha = administratorDTO.Senha,
        Perfil = administratorDTO.Perfil.ToString() ?? PerfilEnum.Editor.ToString()
    };

    administratorService.Add(administrator);

    return Results.Created($"/administrator/{administrator.Id}", administrator);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");
#endregion

#region Veículos
app.MapPost("/vehicles", (
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

app.MapGet("/vehicles",
    ([FromQuery]
        int? page,
        IVehicleService vehicleService) =>
    {
        var vehicles = vehicleService.GetAll(page);
        return Results.Ok(vehicles);
    }

)
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veículos");

app.MapGet("/vehicles/{id}",
    ([FromRoute]
       int id,
        IVehicleService vehicleService) =>
    {
        var vehicle = vehicleService.GetById(id);

        if (vehicle == null) return Results.NotFound("Veículo não encontrado");
        return Results.Ok(vehicle);
    }

)
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veículos");

app.MapPut("/vehicles/{id}",
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

app.MapDelete("/vehicles/{id}",
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

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
