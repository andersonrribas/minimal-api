using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;
using proj_minimal_api.Domain.Entities;
using proj_minimal_api.Domain.Enuns;
using proj_minimal_api.Domain.Interfaces;
using proj_minimal_api.Domain.ModelViews;
using proj_minimal_api.Domain.Services;
{

}
#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

//Adição Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    if (administratorService.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();

}).WithTags("Administradores");

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

}).WithTags("Administradores");

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

).WithTags("Administradores");

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
}).WithTags("Administradores");
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
}).WithTags("Veículos");

app.MapGet("/vehicles",
    ([FromQuery]
        int? page,
        IVehicleService vehicleService) =>
    {
        var vehicles = vehicleService.GetAll(page);
        return Results.Ok(vehicles);
    }

).WithTags("Veículos");

app.MapGet("/vehicles/{id}",
    ([FromRoute]
       int id,
        IVehicleService vehicleService) =>
    {
        var vehicle = vehicleService.GetById(id);

        if (vehicle == null) return Results.NotFound("Veículo não encontrado");
        return Results.Ok(vehicle);
    }

).WithTags("Veículos");

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

).WithTags("Veículos");

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


).WithTags("Veículos");

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

app.Run();
#endregion
