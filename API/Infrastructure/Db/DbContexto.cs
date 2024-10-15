using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;

namespace MinimalApi.Infrastructure.Db
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;
        public DbContexto(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
        public DbSet<Administrator> Administrators { get; set; } = default!;
        public DbSet<Vehicle> Vehicles { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var stringConnection = _configuracaoAppSettings.GetConnectionString("Mysql")?.ToString();

                if (!string.IsNullOrEmpty(stringConnection))
                {
                    optionsBuilder.UseMySql(
                        stringConnection,
                        ServerVersion.AutoDetect(stringConnection)
                    );
                }
            }
        }
    }
}
