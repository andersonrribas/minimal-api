using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Infrastructure.Db;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Services;
using Test.Infrastructure.Db;

namespace Test.Domain.Services
{
    [TestClass]
    public class VehicleServiceTests
    {
        private TestDbContext _context = null!;
        private VehicleService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            // Configurando o banco de dados em memória
            var options = new DbContextOptionsBuilder<DbContexto>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TestDbContext(options);

            // Populando o banco de dados com dados iniciais para testes
            _context.Vehicles.Add(new Vehicle
            {
                Name = "Car A",
                Brand = "Brand X",
                Year = 2020
            });
            _context.SaveChanges();

            // Inicializando o serviço com o contexto de teste
            _service = new VehicleService(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Limpar o banco de dados entre os testes
            _context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void AddVehicle_ShouldAddVehicle()
        {
            // Arrange
            var newVehicle = new Vehicle
            {
                Name = "Car B",
                Brand = "Brand Y",
                Year = 2022
            };

            // Act
            _service.Add(newVehicle);

            // Assert
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.Name == "Car B");
            Assert.IsNotNull(vehicle);
            Assert.AreEqual("Car B", vehicle.Name);
        }

        [TestMethod]
        public void GetVehicleById_ShouldReturnVehicle()
        {
            // Act
            var vehicle = _service.GetById(1);

            // Assert
            Assert.IsNotNull(vehicle);
            Assert.AreEqual("Car A", vehicle.Name);
        }

        [TestMethod]
        public void UpdateVehicle_ShouldUpdateVehicle()
        {
            // Arrange
            var vehicle = _service.GetById(1);

            // Verifica se o veículo foi encontrado
            if (vehicle == null)
            {
                Assert.Fail("Veículo não encontrado para atualização.");
                return;
            }

            vehicle.Name = "Updated Car";

            // Act
            _service.Update(vehicle);

            // Assert
            var updatedVehicle = _context.Vehicles.FirstOrDefault(v => v.Name == "Updated Car");
            Assert.IsNotNull(updatedVehicle);
        }

        [TestMethod]
        public void DeleteVehicle_ShouldRemoveVehicle()
        {
            // Arrange
            var vehicle = _service.GetById(1);
            // Verifica se o veículo foi encontrado
            if (vehicle == null)
            {
                Assert.Fail("Veículo não encontrado para deleção.");
                return;
            }


            // Act
            _service.Delete(vehicle);

            // Assert
            var deletedVehicle = _context.Vehicles.FirstOrDefault(v => v.Id == 1);
            Assert.IsNull(deletedVehicle);
        }
    }

}