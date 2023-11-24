using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_liberar_impressao", Schema = "expedicao")]
      public class LiberarImpressaoModel
      {
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("planilha")]
        public string? Planilha { get; set; }
        [Column("nome_caixa")]
        public string? NomeCaixa { get; set; }
        [Column("impresso")]
        public bool? Impresso { get; set; }
      }
}
