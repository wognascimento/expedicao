using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Keyless]
  [Table("qry_rpt_packlist_carreg_caminhao", Schema = "expedicao")]
  public class PacklistCarregCaminhaoModel
  {
    public string? ordem { get; set; }
    public string? sigla { get; set; }
    public string? nome { get; set; }
    public string? local_shoppings { get; set; }
    public string? nome_caixa { get; set; }
    public double? qtd_expedida { get; set; }
    public string? planilha { get; set; }
    public int? codcompl { get; set; }
    public string? descricao { get; set; }
    public string? descricao_adicional { get; set; }
    public string? complementoadicional { get; set; }
    public string? barcode { get; set; }
    public string? item_memorial { get; set; }
    public int? coddetalhescompl { get; set; }
    public double? liquido { get; set; }
    public double? bruto { get; set; }
    public string? descricao_completa { get; set; }
    public string? caminhao { get; set; }
    public DateTime? data { get; set; }
    public string? prodcontrolado { get; set; }
  }
}
