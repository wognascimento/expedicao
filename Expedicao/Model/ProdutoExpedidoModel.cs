using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_produto_expedido", Schema = "expedicao")]
    public class ProdutoExpedidoModel
    {
        [Column("coddetalhescompl")]
        public int? CodDetalhesCompl { get; set; }
        [Column("id_aprovado")]
        public int? IdAprovado { get; set; }
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("local_shoppings")]
        public string? LocalShoppings { get; set; }
        [Column("planilha")]
        public string? Planilha { get; set; }
        [Column("qtd")]
        public double? Qtd { get; set; }
        [Column("descricao_produto")]
        public string? DescricaoProduto { get; set; }
        [Column("codcompl")]
        public int? CodCompl { get; set; }
        [Column("codcompladicional")]
        public int? CodComplAdicional { get; set; }
        [Column("item_memorial")]
        public string? ItemMemorial { get; set; }
    }
}
