using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using proj_minimal_api.Domain.Entities;

namespace Test.Domain.Entities
{
    [TestClass]
    public class VehicleTest
    {
        [TestClass]
        public class VehicleTests
        {
            [TestMethod]
            public void Should_Create_Vehicle_With_Valid_Data()
            {
                // Arrange
                var vehicle = new Vehicle
                {
                    Name = "Civic",
                    Brand = "Honda",
                    Year = 2020
                };

                // Act & Assert - Validar cada propriedade separadamente
                Assert.IsNotNull(vehicle, "Vehicle object is null.");

                Assert.IsFalse(string.IsNullOrEmpty(vehicle.Name),
                               "Vehicle Name is null or empty.");

                Assert.IsFalse(string.IsNullOrEmpty(vehicle.Brand),
                               "Vehicle Brand is null or empty.");

                Assert.IsTrue(vehicle.Year >= 1950,
                              "Vehicle Year is too old. Must be >= 1950.");
            }
        }
    }
}