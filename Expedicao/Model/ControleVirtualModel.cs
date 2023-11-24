using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_controle-virtual", Schema = "expedicao")]
    public class ControleVirtualModel
    {
        public string? sigla { get; set; }
        public long? coddetalhescompl { get; set; }
        public string? codvol { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public double? qtd_expedida { get; set; }
        public string? baia_virtual { get; set; }
    }
}
