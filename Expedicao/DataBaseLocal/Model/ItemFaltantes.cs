using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao.DataBaseLocal
{
  [Table("itemFaltantes")]
  public class ItemFaltantes
  {
    [Key]
    public int id { get; set; }
    public int id_aprovado { get; set; }
    public string sigla { get; set; }
    public string planilha { get; set; }
    public string descricao_completa { get; set; }
    public string unidade { get; set; }
    public double qtd_expedida { get; set; }
    public string nome_caixa { get; set; }
    public string setor { get; set; }
    public int codigo { get; set; }
    public string barcode { get; set; }
    public string codvol { get; set; }
    public double m3_volume { get; set; }
    public string item_memorial { get; set; }
    public string? baia_caminhao { get; set; }
    public string? endereco { get; set; }
    public string carregado { get; set; }
    public string enviado { get; set; }
  }
}
