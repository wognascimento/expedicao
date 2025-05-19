using Expedicao.DataBaseLocal;
using Microsoft.EntityFrameworkCore;

namespace Expedicao
{
    public class BaseLocal : DbContext
    {
        DataBase BaseSettings = DataBase.Instance;

        public virtual DbSet<ItemCarregado> ItemCarregados { get; set; }
        public virtual DbSet<ItemFaltantes> ItemFaltantes { get; set; }
        public virtual DbSet<Romaneio> Romaneios { get; set; }
        public virtual DbSet<RomaneioCaminhao> RomaneioCaminhaos { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(@$"Data Source={BaseSettings.CaminhoSistema}\Database.db");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RomaneioCaminhao>().HasKey(m => new {m.id_aprovado, m.numero_caminhao});
            modelBuilder.Entity<Romaneio>().HasKey(m => new { m.id_aprovado, m.sigla, m.placa, m.conferente, m.data });

            base.OnModelCreating(modelBuilder);
        }
    }
}
