using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao.Model
{
    [Keyless]
    [Table("qry_pre_conferencia_itens_shopp", Schema = "expedicao")]
    public class PreConferenciaItemShoppModel
    {
        public string? sigla { get; set; }
        public string? local_shoppings { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public string? unidade { get; set; }
        public long? codcompladicional { get; set; }
        public double? qtd_expedida { get; set; }
        public string? nome_caixa { get; set; }
        public string? setor { get; set; }
        public long? codigo { get; set; }
        public string? barcode { get; set; }
        public DateTime? data { get; set; }
        public string? caminhao { get; set; }
        public long? codcompl { get; set; }
        public int? vol_exp { get; set; }
        public int? vol_tot_exp { get; set; }
        public long? coddetalhescompl { get; set; }
    }
}
