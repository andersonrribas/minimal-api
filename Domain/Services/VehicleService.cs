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
    public class VehicleService : IVehicleService
    {
        private readonly DbContexto _contexto;
        public VehicleService(DbContexto contexto)
        {
            _contexto = contexto;
        }

        /// <summary>
        /// Adiciona um veículo
        /// </summary>
        /// <param name="vehicle"></param>
        public void Add(Vehicle vehicle)
        {
            _contexto.Vehicles.Add(vehicle);
            _contexto.SaveChanges();
        }

        /// <summary>
        /// Delata um veículo
        /// </summary>
        /// <param name="vehicle"></param>
        public void Delete(Vehicle vehicle)
        {
            _contexto.Vehicles.Remove(vehicle);
            _contexto.SaveChanges();
        }

        /// <summary>
        /// Busca paginada por todos os veículos
        /// </summary>
        /// <param name="page"></param>
        /// <param name="name"></param>
        /// <param name="brand"></param>
        /// <returns></returns>
        public List<Vehicle> GetAll(int? page = 1, string? name = null, string? brand = null)
        {
            // Definir o número de itens por página
            int pageSize = 10;

            // Começa a construção da consulta base
            var query = _contexto.Vehicles.AsQueryable();

            // Se o nome for fornecido, faz a busca por nome
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(v => v.Name.Contains(name));
            }

            // Se o nome não for fornecido, mas a marca for, faz a busca pela marca
            else if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(v => v.Brand.Contains(brand));
            }

            // Aplicar a paginação
            if (page.HasValue)
                query = query
                    .Skip((page.Value - 1) * pageSize)   // Ignora os registros das páginas anteriores
                    .Take(pageSize);               // Pega apenas o número de registros da página atual

            return query.ToList();// Executa a consulta e retorna os resultados como lista
        }

        /// <summary>
        /// Busca um veículo pelo identificador 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Vehicle? GetById(int id)
        {
            return _contexto.Vehicles
                .Where(v => v.Id == id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Atualiza os dados de um veículo
        /// </summary>
        /// <param name="vehicle"></param>
        public void Update(Vehicle vehicle)
        {
            _contexto.Vehicles.Update(vehicle);
            _contexto.SaveChanges();
        }
    }
}