using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("view_pendencia_expedicao", Schema = "expedicao")]
    public class PendenciaExpedicaoModel
    {
        public DateTime? data_de_expedicao { get; set; }
        public string? sigla { get; set; }
        public string? item_memorial { get; set; }
        public string? baia_caminhao {  get; set; }
        public string? planilha { get; set; }
        public string? descricao { get; set; }
        public string? descricao_adicional { get; set; }
        public string? complementoadicional { get; set; }
        public double? qtd_compl { get; set; }
        public double? qtd_expedida { get; set; }
        public double? saldo_expedido { get; set; }
        public double? m3_faltante {  get; set; }
    }
}
