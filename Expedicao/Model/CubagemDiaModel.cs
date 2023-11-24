using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("c_bd_cubagem_dia", Schema = "expedicao")]
    public class CubagemDiaModel
    {
        public DateTime? data { get; set; }
        public double? volumes { get; set; }
        public string? sigla { get; set; }
        public double? cubagem { get; set; }
        public double? qtd_expedida {  get; set; }
    }
}
