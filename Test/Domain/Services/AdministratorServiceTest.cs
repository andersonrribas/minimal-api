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
    public class AdministratorServiceTests
    {
        private TestDbContext _context = null!;
        private AdministratorService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            // Configurando o banco de dados em memória
            var options = new DbContextOptionsBuilder<DbContexto>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TestDbContext(options);

            // Populando o banco de dados com dados iniciais para testes
            _context.Administrators.Add(new Administrator
            {
                Email = "admin@test.com",
                Senha = "password123",
                Perfil = "Adm"
            });
            _context.SaveChanges();

            // Inicializando o serviço com o contexto de teste
            _service = new AdministratorService(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Limpar o banco de dados entre os testes
            _context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void AddAdministrator_ShouldAddAdministrator()
        {
            // Arrange
            var newAdmin = new Administrator
            {
                Email = "newadmin@test.com",
                Senha = "newpassword123",
                Perfil = "User"
            };

            // Act
            _service.Add(newAdmin);

            // Assert
            var admin = _context.Administrators.FirstOrDefault(a => a.Email == "newadmin@test.com");
            Assert.IsNotNull(admin);
            Assert.AreEqual("newadmin@test.com", admin.Email);
        }

        [TestMethod]
        public void GetAdministratorById_ShouldReturnAdministrator()
        {
            // Act
            var admin = _service.GetById(1);

            // Assert
            Assert.IsNotNull(admin);
            Assert.AreEqual("admin@test.com", admin.Email);
        }

        [TestMethod]
        public void UpdateAdministrator_ShouldUpdateAdministrator()
        {
            // Arrange
            var admin = _service.GetById(1);
            if (admin == null)
            {
                Assert.Fail("Administrador não encontrado para atualização.");
                return;
            }

            admin.Email = "updated@test.com";

            // Act
            _service.Update(admin);

            // Assert
            var updatedAdmin = _context.Administrators.FirstOrDefault(a => a.Email == "updated@test.com");
            Assert.IsNotNull(updatedAdmin);
        }

        [TestMethod]
        public void DeleteAdministrator_ShouldRemoveAdministrator()
        {
            // Arrange
            var admin = _service.GetById(1);
            if (admin == null)
            {
                Assert.Fail("Administrador não encontrado para deleção.");
                return;
            }

            // Act
            _service.Delete(admin);

            // Assert
            var deletedAdmin = _context.Administrators.FirstOrDefault(a => a.Id == 1);
            Assert.IsNull(deletedAdmin);
        }
    }


}