using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("t_conf_carga_geral", Schema = "expedicao")]
  public class ConfCargaGeralModel
  {
    [Key]
    [Column("barcode")]
    public string? Barcode { get; set; }
    [Column("doca_origem")]
    public string? DocaOrigem { get; set; }
    [Column("data")]
    public DateTime? Data { get; set; }
    [Column("shopp")]
    public string? Shopp { get; set; }
    [Column("resp")]
    public string? Resp { get; set; }
    [Column("caminhao")]
    public string? Caminhao { get; set; }
    [Column("entradasistema")]
    public DateTimeOffset? EntradaSistema { get; set; }
    [Column("alterado_por")]
    public string? AlteradoPor { get; set; }
    [Column("data_altera")]
    public DateTimeOffset? DataAltera { get; set; }
    [Column("inserido_por")]
    public string? InseridoPor { get; set; }
    [Column("inserido_em")]
    public DateTime? InseridoEm { get; set; }
  }
}
