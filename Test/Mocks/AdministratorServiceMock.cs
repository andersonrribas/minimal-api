using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.DTOs;

namespace Test.Mocks
{
    public class AdministratorServiceMock : IAdministratorService
    {
        private static List<Administrator> administrators = new List<Administrator>(){
        new Administrator{
            Id = 1,
            Email = "adm@teste.com",
            Senha = "123456",
            Perfil = "Adm"
        },
        new Administrator{
            Id = 2,
            Email = "editor@teste.com",
            Senha = "123456",
            Perfil = "Editor"
        }
    };

        public Administrator Add(Administrator administrator)
        {
            administrator.Id = administrators.Count + 1;
            administrators.Add(administrator);
            return administrator;
        }

        public void Delete(Administrator administrator)
        {
            var adminToDelete = administrators.Find(a => a.Id == administrator.Id);
            if (adminToDelete != null)
            {
                administrators.Remove(adminToDelete);
            }
        }

        public List<Administrator> GetAll(int? page = 1)
        {
            return administrators;
        }

        public Administrator? GetById(int id)
        {
            return administrators.Find(a => a.Id == id);
        }

        public Administrator? Login(LoginDTO loginDTO)
        {
            return administrators.Find(
                a => a.Email == loginDTO.Email &&
                     a.Senha == loginDTO.Senha
            );
        }

        public void Update(Administrator administrator)
        {
            var existingAdmin = administrators.Find(a => a.Id == administrator.Id);

            if (existingAdmin != null)
            {
                // Atualizando os campos do administrador existente
                existingAdmin.Email = administrator.Email;
                existingAdmin.Senha = administrator.Senha;
                existingAdmin.Perfil = administrator.Perfil;
            }
        }
    }

}