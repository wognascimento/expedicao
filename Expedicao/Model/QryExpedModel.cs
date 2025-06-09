using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_t_exped", Schema = "expedicao")]
    public class QryExpedModel
    {
        public string? sigla {get; set; }
        public string? codvol {get; set; }
        public long? coddetalhescompl {get; set; }
        public string? planilha {get; set; }
        public string? descricao_completa {get; set; }
        public string? unidade {get; set; }
        public double? qtd_expedida {get; set; }
        public string? modelo_de_cx { get; set; }
        public int? vol_exp {get; set; }
        public int? vol_tot_exp {get; set; }
        public double? pl {get; set; }
        public double? pb {get; set; }
        public double? largura {get; set; }
        public double? altura {get; set; }
        public double? profundidade {get; set; }
        public double? cubagem { get; set; }
        public string? inserido_por {get; set; }
        public DateTime? inserido_em {get; set; }
        public string? alterado_por {get; set; }
        public DateTime? alterado_quando { get; set; }
        public int? semana { get; set; }
    }
}
