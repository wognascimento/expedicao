using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("t_caixas", Schema = "expedicao")]
  public class CaixaModel
  {
    [Key]
    [Column("cod_caixa")]
    public long? CodCaixa { get; set; }
    [Column("nome_caixa")]
    public string? NomeCaixa { get; set; }
    [Column("setor")]
    public string? Setor { get; set; }
    [Column("sigla")]
    public string? Sigla { get; set; }
    [Column("sequencia")]
    public long? Sequencia { get; set; }
    [Column("inserido_por")]
    public string? InseridoPor { get; set; }
    [Column("inserido_em")]
    public DateTime InseridoEm { get; set; }
    [Column("impresso")]
    public string? Impresso { get; set; }
    [Column("producao")]
    public string? Producao { get; set; }
  }
}
