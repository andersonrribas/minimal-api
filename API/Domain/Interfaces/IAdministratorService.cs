using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.DTOs;
using MinimalApi.Domain.Entities;

namespace MinimalApi.Domain.Interfaces
{
    public interface IAdministratorService
    {
        Administrator? Login(LoginDTO loginDTO);
        Administrator Add(Administrator administrator);
        List<Administrator> GetAll(int? page = 1);
        Administrator? GetById(int id);
        void Update(Administrator administrator);
        void Delete(Administrator administrator);
    }
}