using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Infrastructure.Db;

namespace Test.Infrastructure.Db
{
    public class TestDbContext : DbContexto
    {
        public TestDbContext(DbContextOptions<DbContexto> options)
            : base(new ConfigurationRoot(new List<IConfigurationProvider>())) // ou utilize uma configuração válida se necessário
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Garantir que estamos usando um banco de dados em memória para testes
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("TestDatabase");
            }
        }
    }



}