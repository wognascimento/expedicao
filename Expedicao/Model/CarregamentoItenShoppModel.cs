using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_carregamento_itens_shopp", Schema = "expedicao")]
    public class CarregamentoItenShoppModel
    {
        //[Key]
        //public int? id { get; set; }
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("local_shoppings")]
        public string? LocalShoppings { get; set; }
        [Column("planilha")]
        public string? Planilha { get; set; }
        [Column("descricao")]
        public string? Descricao { get; set; }
        [Column("descricao_adicional")]
        public string? DescricaoAdicional { get; set; }
        [Column("complementoadicional")]
        public string? ComplementoAdicional { get; set; }
        [Column("unidade")]
        public string? Unidade { get; set; }
        [Column("codcompladicional")]
        public long? CodcomplAdicional { get; set; }
        [Column("nome_caixa")]
        public string? NomeCaixa { get; set; }
        [Column("setor")]
        public string? Setor { get; set; }
        [Column("codigo")]
        public long? Codigo { get; set; }
        [Column("barcode")]
        public string? Barcode { get; set; }
        [Column("data")]
        public DateTime? Data { get; set; }
        [Column("caminhao")]
        public string? Caminhao { get; set; }
        [Column("codcompl")]
        public long? CodCompl { get; set; }
        [Column("qtd_expedida")]
        public double? QtdExpedida { get; set; }
        [Column("vol_exp")]
        public int? VolExp { get; set; }
        [Column("vol_tot_exp")]
        public int? VolTotExp { get; set; }
        [Column("coddetalhescompl")]
        public long? CodDetalhesCompl { get; set; }
        [Column("codvol")]
        public string? CodVol { get; set; }
        [Column("kit_solucao")]
        public double? KitSolucao { get; set; }
    }
}
