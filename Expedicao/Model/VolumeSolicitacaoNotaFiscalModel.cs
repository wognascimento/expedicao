using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_volumes_solicitacao_notafiscal", Schema = "expedicao")]
    public class VolumeSolicitacaoNotaFiscalModel
    {
        public long? codexped { get; set; }
        public string? sigla { get; set; }
        public string? caminhao { get; set; }
    }
}
