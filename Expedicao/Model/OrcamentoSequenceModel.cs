using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("orcamento1_sequences", Schema = "nf")]
  public class OrcamentoSequenceModel
  {
    [Key]
    [Column("numero_do_orcamento")]
    public long? NumeroOrcamento { get; set; }
    [Column("data_do_orcamento")]
    public DateTime? DataOrcamento { get; set; }
    [Column("cliente")]
    public string? Cliente { get; set; }
  }
}
