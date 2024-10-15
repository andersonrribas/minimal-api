using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MinimalApi.Domain.DTOs;
using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class VehicleRequestTest
    {
        private async Task<string> GetAuthTokenAsync()
        {
            // Arrange: primeiro, faça login para obter um token
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var loginContent = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");
            var loginResponse = await Setup.client.PostAsync("/administrators/login", loginContent);

            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode, "Falha ao fazer login");

            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<LoggedAdministratorDTO>(loginResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admLogado?.Token, "Token não pode ser nulo");
            return admLogado.Token;
        }

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Setup.ClassInit(testContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Setup.ClassCleanup();
        }

        [TestMethod]
        public async Task AddVehicle_ShouldReturnOk()
        {
            // Arrange
            var vehicle = new VehicleDTO
            {
                Name = "Tesla Model S",
                Brand = "Tesla",
                Year = 2021
            };

            var content = new StringContent(JsonSerializer.Serialize(vehicle), Encoding.UTF8, "application/json");

            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.PostAsync("/vehicles", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var addedVehicle = JsonSerializer.Deserialize<Vehicle>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(addedVehicle);
            Assert.AreEqual(vehicle.Name, addedVehicle.Name);
            Assert.AreEqual(vehicle.Brand, addedVehicle.Brand);
            Assert.AreEqual(vehicle.Year, addedVehicle.Year);

            Console.WriteLine("Vehicle added successfully: " + addedVehicle.Name);
        }

        [TestMethod]
        public async Task GetAllVehicles_ShouldReturnOk()
        {
            // Arrange
            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync("/vehicles");


            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var vehicles = JsonSerializer.Deserialize<List<Vehicle>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(vehicles);
            Assert.IsTrue(vehicles.Count > 0);

            Console.WriteLine("Número de veículos: " + vehicles.Count);
        }

        [TestMethod]
        public async Task GetVehicleById_ShouldReturnOk()
        {
            // Arrange
            var vehicleId = 1; // Id de exemplo, mude conforme necessário

            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync($"/vehicles/{vehicleId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var vehicle = JsonSerializer.Deserialize<Vehicle>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(vehicle);
            Assert.AreEqual(vehicleId, vehicle.Id);

            Console.WriteLine("Veículo: " + vehicle.Name);
        }

        [TestMethod]
        public async Task UpdateVehicle_ShouldReturnOk()
        {
            // Arrange
            var vehicleId = 1; // Id de exemplo
            var updatedVehicle = new Vehicle
            {
                Id = vehicleId,
                Name = "Tesla Model X",
                Brand = "Tesla",
                Year = 2022
            };

            var content = new StringContent(JsonSerializer.Serialize(updatedVehicle), Encoding.UTF8, "application/json");

            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.PutAsync($"/vehicles/{vehicleId}", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var vehicle = JsonSerializer.Deserialize<Vehicle>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(vehicle);
            Assert.AreEqual(updatedVehicle.Name, vehicle.Name);
            Assert.AreEqual(updatedVehicle.Brand, vehicle.Brand);
            Assert.AreEqual(updatedVehicle.Year, vehicle.Year);

            Console.WriteLine("Veículo atualizado com sucesso: " + vehicle.Name);
        }

        [TestMethod]
        public async Task DeleteVehicle_ShouldReturnNoContent()
        {
            // Arrange
            var vehicleId = 1; // Id de exemplo

            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.DeleteAsync($"/vehicles/{vehicleId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            Console.WriteLine("Veículo deletado com sucesso: ID " + vehicleId);
        }
    }
}
