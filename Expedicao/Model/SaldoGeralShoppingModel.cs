using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_saldo_geral_shopping", Schema = "expedicao")]
    public class SaldoGeralShoppingModel
    {
        public DateTime? data_saida { get; set; }
        public string? sigla { get; set; }
        public double? qtde_pedida { get; set; }
        public double? qtde_expedido { get; set; }
        public double? peso_liquido { get; set; }
        public double? peso_bruto { get; set; }
        public double? cubagem { get; set; }
        public double? volumes { get; set; }
        public double? perc_shop { get; set; }
        //public DateTime? fechamento_shopp { get; set; }
        public double? cubagem_memorial { get; set; }
        //public double? novo_perc {  get; set; }
        public double? m3_media { get; set; }
        //public double? pl_media { get; set; }
        //public double? pb_media { get; set; }
    }
}
