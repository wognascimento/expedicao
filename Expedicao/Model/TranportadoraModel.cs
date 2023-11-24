using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("tbltranportadoras", Schema = "operacional")]
  public class TranportadoraModel
  {
    [Key]
    [Column("codtransportadora")]
    public long? CodTransportadora { get; set; }
    [Column("nometransportadora")]
    public string? NomeTransportadora { get; set; }
    [Column("endereco")]
    public string? Endereco { get; set; }
    [Column("bairro")]
    public string? Bairro { get; set; }
    [Column("cep")]
    public string? Cep { get; set; }
    [Column("cidade")]
    public string? Cidade { get; set; }
    [Column("uf")]
    public string? Uf { get; set; }
    [Column("ie")]
    public string? Ie { get; set; }
    [Column("ccm")]
    public string? Ccm { get; set; }
    [Column("cnpj")]
    public string? Cnpj { get; set; }
    [Column("ddd")]
    public int? Ddd { get; set; }
    [Column("fone_1")]
    public int? Fone1 { get; set; }
    [Column("fone_2")]
    public int? Fone2 { get; set; }
    [Column("contato")]
    public string? Contato { get; set; }
    [Column("id_nextel")]
    public string? IdNextel { get; set; }
  }
}
