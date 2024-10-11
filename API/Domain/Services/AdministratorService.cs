using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;
using proj_minimal_api.Domain.Entities;
using proj_minimal_api.Domain.Interfaces;

namespace proj_minimal_api.Domain.Services
{
    public class AdministratorService : IAdministratorService
    {
        private readonly DbContexto _contexto;
        public AdministratorService(DbContexto contexto)
        {
            _contexto = contexto;
        }
        /// <summary>
        /// Adicionar um administrador
        /// </summary>
        /// <param name="administrator"></param>
        public Administrator Add(Administrator administrator)
        {
            _contexto.Administrators.Add(administrator);
            _contexto.SaveChanges();

            return administrator;
        }

        public void Delete(Administrator administrator)
        {
            _contexto.Administrators.Remove(administrator);
            _contexto.SaveChanges();
        }

        public void Update(Administrator administrator)
        {
            _contexto.Administrators.Update(administrator);
            _contexto.SaveChanges();
        }
        public Administrator? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administrators
                .Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha)
                .FirstOrDefault();

            return adm;
        }

        public List<Administrator> GetAll(int? page = 1)
        {
            // Definir o número de itens por página
            int pageSize = 10;

            // Começa a construção da consulta base
            var query = _contexto.Administrators.AsQueryable();

            // Aplicar a paginação
            if (page.HasValue)
                query = query
                    .Skip((page.Value - 1) * pageSize)   // Ignora os registros das páginas anteriores
                    .Take(pageSize);               // Pega apenas o número de registros da página atual

            return query.ToList();// Executa a consulta e retorna os resultados como lista
        }

        public Administrator? GetById(int id)
        {
            return _contexto.Administrators
                .Where(a => a.Id == id)
                .FirstOrDefault();
        }
    }
}