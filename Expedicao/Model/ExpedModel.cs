using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("t_exped", Schema = "expedicao")]
  public class ExpedModel
  {
    [Key]
    [Column("codexped")]
    public long? CodExped { get; set; }
    [Column("qtd_expedida")]
    public double? QtdExpedida { get; set; }
    [Column("vol_exp")]
    public int? VolExp { get; set; }
    [Column("vol_tot_exp")]
    public int? VolTotExp { get; set; }
    [Column("pl")]
    public double? Pl { get; set; }
    [Column("pb")]
    public double? Pb { get; set; }
    [Column("largura")]
    public double? Largura { get; set; }
    [Column("altura")]
    public double? Altura { get; set; }
    [Column("profundidade")]
    public double? Profundidade { get; set; }
    [Column("codvol")]
    public string? CodVol { get; set; }
    [Column("cadastrado_por")]
    public string? CadastradoPor { get; set; }
    [Column("quando")]
    public DateTimeOffset? Quando { get; set; }
    [Column("baia_virtual")]
    public string? BaiaVirtual { get; set; }
    [Column("modelo_de_cx")]
    public string? ModeloCaixa { get; set; }
    [Column("coddetalhescompl")]
    public long? CodDetalhesCompl { get; set; }
    [Column("alterado_por")]
    public string? AlteradoPor { get; set; }
    [Column("alterado_quando")]
    public DateTime? AlteradoQuando { get; set; }
    [Column("inserido_por")]
    public string? InseridoPor { get; set; }
    [Column("inserido_em")]
    public DateTime? InseridoEm { get; set; }
    [Column("operacao")]
    public string? Operacao { get; set; }
    [Column("volume")]
    public int? Volume { get; set; }
    public bool? nf_emitida { get; set; }
  }
}
