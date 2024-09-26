using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao.Model
{
    [Keyless]
    [Table("qry_cubagem_sigla_caminao", Schema = "expedicao")]
    public class CubagemSiglaCaminaoModel
    {
        public DateTime? data_de_expedicao { get; set; }
        public string? est { get; set; }
        public string? cidade { get; set; }
        public string? sigla { get; set; }
        public string? baia_caminhao { get; set; }
        public string? placa_caminhao { get; set; }
        public double? preco { get; set; }
        public int? volumes { get; set; }
        public double? pl { get; set; }
        public double? pb { get; set; }
        public double? m3 { get; set; }
    }
}
