using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("totvs_rpt_nf_total_completo", Schema = "expedicao")]
    public class RelatorioNfTotalCompletoModel
    {
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("shopp")]
        public string? Shopp { get; set; }
        [Column("nome")]
        public string? Nome { get; set; }
        [Column("data")]
        public DateTime? Data { get; set; }
        [Column("caminhao")]
        public string? Caminhao { get; set; }
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
        [Column("cnpj")]
        public string? Cnpj { get; set; }
        [Column("nome_caixa")]
        public string? NomeCaixa { get; set; }
        [Column("bruto")]
        public double? Bruto { get; set; }
        [Column("liquido")]
        public double? Liquido { get; set; }
        [Column("preco")]
        public double? Preco { get; set; }
    }
}
