using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao.DataBaseLocal
{
  [Table("romaneio_caminhao")]
  public class RomaneioCaminhao
  {
    public int? id_aprovado { get; set; }
    public string? numero_caminhao { get; set; }
  }
}
