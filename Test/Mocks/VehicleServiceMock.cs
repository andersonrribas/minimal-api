using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;

namespace Test.Mocks
{
    public class VehicleServiceMock : IVehicleService
    {
        // Lista de veículos estática que será usada como um "banco de dados"
        private static List<Vehicle> vehicles = new List<Vehicle>()
         {
             new Vehicle { Id = 1, Name = "Corolla", Brand = "Toyota", Year = 2020 },
             new Vehicle { Id = 2, Name = "Civic", Brand = "Honda", Year = 2019 },
             new Vehicle { Id = 3, Name = "Model S", Brand = "Tesla", Year = 2021 }
         };

        // Adicionar um veículo à lista
        public void Add(Vehicle vehicle)
        {
            vehicle.Id = vehicles.Count + 1; // Definir o ID automaticamente
            vehicles.Add(vehicle); // Adicionar o veículo à lista
        }

        // Remover um veículo da lista
        public void Delete(Vehicle vehicle)
        {
            vehicles.Remove(vehicle);
        }

        // Obter todos os veículos, com possibilidade de filtrar por nome ou marca
        public List<Vehicle> GetAll(int? page = 1, string? name = null, string? brand = null)
        {
            var query = vehicles.AsQueryable();

            // Filtragem opcional por nome
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(v => v.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            // Filtragem opcional por marca
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(v => v.Brand.Contains(brand, StringComparison.OrdinalIgnoreCase));
            }

            // Simulando paginação (10 itens por página)
            int pageSize = 10;
            return query.Skip((page.GetValueOrDefault() - 1) * pageSize).Take(pageSize).ToList();
        }

        // Obter veículo por ID
        public Vehicle? GetById(int id)
        {
            return vehicles.FirstOrDefault(v => v.Id == id);
        }

        // Atualizar um veículo existente
        public void Update(Vehicle vehicle)
        {
            var existingVehicle = vehicles.FirstOrDefault(v => v.Id == vehicle.Id);

            if (existingVehicle != null)
            {
                // Atualizar as propriedades do veículo existente
                existingVehicle.Name = vehicle.Name;
                existingVehicle.Brand = vehicle.Brand;
                existingVehicle.Year = vehicle.Year;
            }
        }
    }

}