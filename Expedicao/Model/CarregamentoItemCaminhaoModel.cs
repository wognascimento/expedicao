using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("totvs_carregamento_itens_caminhao", Schema = "expedicao")]
    public class CarregamentoItemCaminhaoModel
    {
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("sigla_serv")]
        public string? SiglaServ { get; set; }
        [Column("data")]
        public DateTime? Data { get; set; }
        [Column("caminhao")]
        public string? Caminhao { get; set; }
        [Column("codcompladicional")]
        public long? CodComplAdicional { get; set; }
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
        [Column("qtd")]
        public double? Qtd { get; set; }
        [Column("conta")]
        public string? Conta { get; set; }
        [Column("est")]
        public string? Est { get; set; }
        [Column("cfop")]
        public string? Cfop { get; set; }
        [Column("custo")]
        public double? Custo { get; set; }
        [Column("peso")]
        public double? Peso { get; set; }
        [Column("cliente")]
        public string? Cliente { get; set; }
        [Column("cnpj")]
        public string? Cnpj { get; set; }
        [Column("totvs")]
        public long? Totvs { get; set; }
        [Column("descricaofiscal")]
        public string? DescricaoFiscal { get; set; }
        [Column("exportado_folhamatic")]
        public string? ExportadoFolhamatic { get; set; }
        [Column("operacao")]
        public string? Operacao { get; set; }
        [Column("ncm")]
        public string? Ncm { get; set; }
    }
}
