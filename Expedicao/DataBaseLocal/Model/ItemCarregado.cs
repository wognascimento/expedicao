using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao.DataBaseLocal
{
  [Table("itemCarregado")]
  public class ItemCarregado
  {
    [Key]
    public string? barcode { get; set; }
    public string? enviado { get; set; }
  }
}
