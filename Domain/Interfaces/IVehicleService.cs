using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.DTOs;
using proj_minimal_api.Domain.Entities;

namespace proj_minimal_api.Domain.Interfaces
{
    public interface IVehicleService
    {
        List<Vehicle> GetAll(
                int? page = 1,
                string? name = null,
                string? brand = null
                );
        Vehicle? GetById(int id);
        void Add(Vehicle vehicle);
        void Update(Vehicle vehicle);
        void Delete(Vehicle vehicle);
    }
}