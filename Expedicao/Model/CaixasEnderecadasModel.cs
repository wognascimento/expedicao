using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_caixas_enderecadas", Schema = "expedicao")]
    public class CaixasEnderecadasModel
    {
        public string? barcode_volume {get; set; }
        public DateTime? ultimoinserido_em {get; set; }
        public string? ultimoendereco {get; set; }
        public string? nome_caixa {get; set; }
        public string? planilha {get; set; }
        public string? descricao_completa {get; set; }
        public string? descricao_adicional {get; set; }
        public string? complementoadicional {get; set; }
        public long? codcompladicional {get; set; }
        public string? unidade {get; set; }
        public double? qtd_expedida {get; set; }
        public string? sigla { get; set; }
    }
}
