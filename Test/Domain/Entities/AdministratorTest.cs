using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using proj_minimal_api.Domain.Entities;

namespace Test.Domain.Entities
{
    [TestClass]
    public class AdministratorTest
    {
        [TestMethod]
        public void Should_Create_Administrator_With_Valid_Data()
        {
            // Arrange
            var administrator = new Administrator
            {
                Email = "admin@example.com",
                Senha = "password123",
                Perfil = "Admin"
            };

            // Act & Assert - Validar cada propriedade separadamente
            Assert.IsNotNull(administrator, "Administrador é nulo.");

            Assert.IsFalse(string.IsNullOrEmpty(administrator.Email),
                           "Email é nulo ou vazio.");

            Assert.IsFalse(string.IsNullOrEmpty(administrator.Senha),
                           "Senha é nula ou vazia.");

            Assert.IsFalse(string.IsNullOrEmpty(administrator.Perfil),
                           "Perfil é nulo ou vazio.");
        }
    }
}