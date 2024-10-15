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
using MinimalApi.Domain.Enum;
using MinimalApi.DTOs;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class AdministratorRequestTest
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
        public async Task Login_ShouldReturnOk()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await Setup.client.PostAsync("/administrators/login", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<LoggedAdministratorDTO>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admLogado?.Email);
            Assert.IsNotNull(admLogado?.Perfil);
            Assert.IsNotNull(admLogado?.Token);

            Console.WriteLine("Token: " + admLogado?.Token);
        }

        [TestMethod]
        public async Task AddAdministrator_ShouldReturnCreated()
        {
            // Arrange
            var newAdministratorDTO = new AdministratorDTO
            {
                Email = "newadmin@teste.com",
                Senha = "senha123",
                Perfil = PerfilEnum.Adm // ou "Admin" se for string
            };


            var content = new StringContent(JsonSerializer.Serialize(newAdministratorDTO), Encoding.UTF8, "application/json");

            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.PostAsync("/administrators", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var addedAdministrator = JsonSerializer.Deserialize<Administrator>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(addedAdministrator);
            Assert.AreEqual(newAdministratorDTO.Email, addedAdministrator.Email);
            Assert.AreEqual(newAdministratorDTO.Perfil.ToString(), addedAdministrator.Perfil);

            Console.WriteLine("Administrador criado: " + addedAdministrator.Email);
        }

        [TestMethod]
        public async Task GetAllAdministrators_ShouldReturnOk()
        {
            // Act
            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await Setup.client.GetAsync("/administrators");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var administrators = JsonSerializer.Deserialize<List<Administrator>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(administrators);
            Assert.IsTrue(administrators.Count > 0);

            Console.WriteLine("Número total de administradores: " + administrators.Count);
        }

        [TestMethod]
        public async Task GetAdministratorById_ShouldReturnOk()
        {
            // Arrange
            var adminId = 1; // ID de exemplo

            // Act
            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await Setup.client.GetAsync($"/administrators/{adminId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var administrator = JsonSerializer.Deserialize<Administrator>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(administrator);
            Assert.AreEqual(adminId, administrator.Id);

            Console.WriteLine("Administrador encontrado: " + administrator.Email);
        }

        [TestMethod]
        public async Task UpdateAdministrator_ShouldReturnOk()
        {
            // Arrange
            var adminId = 2; // ID de exemplo
            var updatedAdministrator = new AdministratorDTO
            {
                Email = "updatedadmin@teste.com",
                Senha = "senha-forte",
                Perfil = PerfilEnum.Adm
            };

            // Obtenha o token de autenticação
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(updatedAdministrator), Encoding.UTF8, "application/json");

            // Act
            var response = await Setup.client.PutAsync($"/administrator/{adminId}", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var administrator = JsonSerializer.Deserialize<Administrator>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(administrator);
            Assert.AreEqual(updatedAdministrator.Email, administrator.Email);
            Assert.AreEqual(updatedAdministrator.Perfil.ToString(), administrator.Perfil);

            Console.WriteLine("Administrador atualizado: " + administrator.Email);
        }

        [TestMethod]
        public async Task DeleteAdministrator_ShouldReturnNoContent()
        {
            // Arrange
            var adminId = 2; // ID de exemplo
            var token = await GetAuthTokenAsync();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.DeleteAsync($"/administrator/{adminId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            Console.WriteLine("Administrator deletado: ID " + adminId);
        }
    }
}