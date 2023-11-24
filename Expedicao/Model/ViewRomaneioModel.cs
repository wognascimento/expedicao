using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("view_romaneios", Schema = "expedicao")]
    public class ViewRomaneioModel
    {
        [Column("nome_motorista")]
        public string NomeMotorista { get; set; }
        [Column("placa_caminhao")]
        public string PlacaCaminhao { get; set; }
        [Column("placa_cidade")]
        public string PlacaCidade { get; set; }
        [Column("placa_estado")]
        public string PlacaEstado { get; set; }
        [Column("placa_carroceria")]
        public string PlacaCarroceria { get; set; }
        [Column("placa_carroceria_cidade")]
        public string PlacaCarroceriaCidade { get; set; }
        [Column("placa_carroceria_estado")]
        public string PlacaCarroceriaEstado { get; set; }
        [Column("bau_altura")]
        public double BauAltura { get; set; }
        [Column("bau_largura")]
        public double BauLargura { get; set; }
        [Column("bau_profundidade")]
        public double BauProfundidade { get; set; }
        [Column("numero_cnh")]
        public string NumeroCnh { get; set; }
        [Column("telefone_motorista")]
        public string TelefoneMotorista { get; set; }
    }
}
