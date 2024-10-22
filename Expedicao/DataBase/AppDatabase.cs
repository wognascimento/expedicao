using Expedicao.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace Expedicao
{
    public class AppDatabase : DbContext
    {
        private readonly DataBase dB = DataBase.Instance;
        public DbSet<AprovadoModel> Aprovados { get; set; }
        public DbSet<ProdutoExpedidoModel> ProdutoExpedidos { get; set; }
        public DbSet<ExpedModel> Expeds { get; set; }
        public DbSet<MedidaModel> Medidas { get; set; }
        public DbSet<EtiquetaVolumeItemModel> EtiquetaVolumes { get; set; }
        public DbSet<LiberarImpressaoModel> LiberarImpressaos { get; set; }
        public DbSet<CaixaModel> Caixas { get; set; }
        public DbSet<TranportadoraModel> Tranportadoras { get; set; }
        public DbSet<ViewRomaneioModel> ViewRomaneios { get; set; }
        public DbSet<RomaneioModel> Romaneios { get; set; }
        public DbSet<CarregamentoItenFaltanteModel> CarregamentoItenFaltantes { get; set; }
        public DbSet<CarregamentoItenShoppModel> CarregamentoItenShopps { get; set; }
        public DbSet<ConfCargaGeralModel> ConfCargaGerals { get; set; }
        public DbSet<OrcamentoSequenceModel> OrcamentoSequences { get; set; }
        public DbSet<CarregamentoItemCaminhaoModel> CarregamentoItemCaminhaos { get; set; }
        public DbSet<VolumeSolicitacaoNotaFiscalModel> VolumesSolicitacaoNotaFiscal { get; set; }
        public DbSet<RelatorioNfTotalCompletoModel> RelatorioNfTotalCompletos { get; set; }
        public DbSet<PacklistCarregCaminhaoModel> PacklistCarregCaminhaos { get; set; }

        public DbSet<QryExpedModel> QryExpeds { get; set; }
        public DbSet<ControleVirtualModel> ControleVirtuals { get; set; }
        public DbSet<CaixasEnderecadasModel> CaixasEnderecadas { get; set; }
        public DbSet<SaldoGeralShoppingModel> SaldoGeralShoppings { get; set; }
        public DbSet<ProdutosBaiadosGeralTotalDataModel> produtosBaiadosData { get; set; }
        public DbSet<CubagemDiaModel> CubagemDias { get; set; }
        public DbSet<CubagemSemanaAnoAnteriorAtualModel> CubagemSemanaAnos { get; set; }
        public DbSet<CubagemPrevistaClienteModel> CubagemPrevistaClientes { get; set; }
        public DbSet<CubagemEnderecada> CubagemEnderecadas { get; set; }
        public DbSet<PendenciaExpedicaoModel> PendenciaExpedicaos { get; set; }
        public DbSet<PreConferenciaItemFaltanteModel> PreConferenciaItemFaltantes { get; set; }
        public DbSet<PreConferenciaItemShoppModel> PreConferenciaItemShopps { get; set; }
        public DbSet<CubagemSiglaCaminaoModel> CubagemSiglaCaminhoes { get; set; }
        public DbSet<ItemCaminhaoModel> ItensCaminhoes { get; set; }
        



        static AppDatabase() => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseNpgsql($"host={dB.Host};user id={dB.Username};password={dB.Password};database={dB.Database};Pooling=false;Timeout=300;CommandTimeout=300;");

            /*
            optionsBuilder.UseNpgsql(
                $"host={dB.Host};" +
                $"user id={dB.Username};" +
                $"password={dB.Password};" +
                $"database={dB.Database};" +
                $"Timeout=300;" +
                $"CommandTimeout=300;" +
                $"KeepAlive=300;"
                );
            */

            optionsBuilder.UseNpgsql(
                $"host={dB.Host};" +
                $"user id={dB.Username};" +
                $"password={dB.Password};" +
                $"database={dB.Database};" +
                $"Pooling=false;" +
                $"Timeout=300;" +
                $"CommandTimeout=300;" +
                $"Application Name=SIG Expedicao <{dB.Database}>;",
                options => { options.EnableRetryOnFailure(); }
                );
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AprovadoModel>().HasKey(m => new { m.IdAprovado, m.Sigla });
            //modelBuilder.Entity<CarregamentoItemCaminhaoModel>().HasKey(m => new { m.SiglaServ, m.Data, m.Caminhao, m.CodComplAdicional });
            //modelBuilder.Entity<EtiquetaVolumeItemModel>().HasKey(m => new { m.CodDetalhesCompl, m.CodVol, m.Sequencia });
            //modelBuilder.Entity<LiberarImpressaoModel>().HasKey(m => new { m.Sigla, m.NomeCaixa });
            //modelBuilder.Entity<ProdutoExpedidoModel>().HasKey(m => new { m.CodDetalhesCompl, m.IdAprovado });
            //modelBuilder.Entity<RelatorioNfTotalCompletoModel>().HasKey(m => new { m.Sigla, m.Shopp, m.Nome, m.Data, m.Caminhao, m.NomeTransportadora, m.Endereco, m.Bairro, m.Cep, m.Cidade, m.Uf, m.Ie, m.Cnpj, m.NomeCaixa, m.Bruto, m.Liquido, m.Preco });
            //modelBuilder.Entity<ViewRomaneioModel>().HasKey(m => new {m.NomeMotorista, m.PlacaCaminhao, m.PlacaCidade, m.PlacaEstado, m.PlacaCarroceria, m.PlacaCarroceriaCidade, m.PlacaCarroceriaEstado, m.BauAltura, m.BauLargura, m.BauProfundidade, m.NumeroCnh, m.TelefoneMotorista });
            base.OnModelCreating(modelBuilder);
        }
    }
}
