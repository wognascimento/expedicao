using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao.DataBaseLocal
{
  [Table("romaneio")]
  public class Romaneio
  {
    public int? id_aprovado { get; set; }
    public string sigla { get; set; }
    public string placa { get; set; }
    public string conferente { get; set; }
    public DateTime data { get; set; }
  }
}
