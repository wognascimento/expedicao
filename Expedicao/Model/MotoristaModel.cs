using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("view_romaneios", Schema = "expedicao")]
  public class MotoristaModel
  {
    [Key]
    [Column("nome_motorista", Order = 0)]
    public string? NomeMotorista { get; set; }
    [Key]
    [Column("placa_caminhao", Order = 1)]
    public string? PlacaCaminhao { get; set; }
    [Key]
    [Column("placa_cidade")]
    public string? PlacaCidade { get; set; }
    [Key]
    [Column("placa_estado")]
    public string? PlacaEstado { get; set; }
    [Key]
    [Column("placa_carroceria", Order = 2)]
    public string? PlacaCarroceria { get; set; }
    [Key]
    [Column("placa_carroceria_cidade")]
    public string? PlacaCarroceriaCidade { get; set; }
    [Key]
    [Column("placa_carroceria_estado")]
    public string? PlacaCarroceriaEstado { get; set; }
    [Key]
    [Column("bau_altura")]
    public string? BauAltura { get; set; }
    [Key]
    [Column("bau_largura")]
    public string? BauLargura { get; set; }
    [Key]
    [Column("bau_profundidade")]
    public string? BauProfundidade { get; set; }
    [Key]
    [Column("numero_cnh", Order = 3)]
    public string? NumeroCnh { get; set; }
    [Key]
    [Column("telefone_motorista")]
    public string? TelefoneMotorista { get; set; }
  }
}
