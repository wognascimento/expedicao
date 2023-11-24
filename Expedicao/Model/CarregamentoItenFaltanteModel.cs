using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_carregamento_itens_faltantes", Schema = "expedicao")]
    public class CarregamentoItenFaltanteModel
    {
        //[Key]
        //public int id { get; set; }
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("id_aprovado")]
        public long? IdAprovado { get; set; }
        [Column("planilha")]
        public string? Planilha { get; set; }
        [Column("descricao")]
        public string? Descricao { get; set; }
        [Column("descricao_adicional")]
        public string? DescricaoAdicional { get; set; }
        [Column("complementoadicional")]
        public string? ComplementoAdicional { get; set; }
        [Column("descricao_completa")]
        public string? DescricaoCompleta { get; set; }
        [Column("unidade")]
        public string? Unidade { get; set; }
        [Column("qtd_expedida")]
        public double? QtdExpedida { get; set; }
        [Column("nome_caixa")]
        public string? NomeCaixa { get; set; }
        [Column("setor")]
        public string? Setor { get; set; }
        [Column("codigo")]
        public long? Codigo { get; set; }
        [Column("barcode")]
        public string? Barcode { get; set; }
        [Column("codvol")]
        public string? CodVol { get; set; }
        [Column("m3_volume")]
        public double? M3Volume { get; set; }
        [Column("item_memorial")]
        public string? ItemMemorial { get; set; }
        [Column("baia_caminhao")]
        public string? BaiaCaminhao { get; set; }
        [Column("endereco")]
        public string? Endereco { get; set; }
    }
}
