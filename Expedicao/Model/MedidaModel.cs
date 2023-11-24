using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("tblmedidas", Schema = "producao")]
  public class MedidaModel
  {
    [Key]
    [Column("nomecx")]
    public string? NomeCaixa { get; set; }
    [Column("alt")]
    public double? Altura { get; set; }
    [Column("larg")]
    public double? Largura { get; set; }
    [Column("prof")]
    public double? Profundidade { get; set; }
    [Column("m3")]
    public double? Cubagem { get; set; }
  }
}
