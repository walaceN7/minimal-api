using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entity;

namespace minimal_api.Infraestrutura.Db
{
    public class DBContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;
        public DBContexto(IConfiguration configuracao)
        {
            _configuracaoAppSettings = configuracao;
        }
        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador
                {
                    ID = 1,
                    Email = "administrador@teste.com",
                    Senha = "123456",
                    Perfil = "Adm"
                }
                );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("MySql")?.ToString();

                if (!string.IsNullOrWhiteSpace(stringConexao))
                {
                    optionsBuilder.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
                }
            }            
        }
    }
}
