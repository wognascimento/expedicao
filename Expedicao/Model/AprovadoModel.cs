using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("t_aprovados", Schema = "producao")]
  public class AprovadoModel
  {
        [Key]
        [Column("id_aprovado")]
        public int? IdAprovado { get; set; }
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("sigla_serv")]
        public string? SiglaServ { get; set; }
        [Column("nome")]
        public string? Nome { get; set; }
        [Column("cidade")]
        public string? Cidade { get; set; }
        [Column("tema")]
        public string? Tema { get; set; }
  }
}
