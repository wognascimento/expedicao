using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("c_cubagem_enderecada_be", Schema = "expedicao")]
    public class CubagemEnderecada
    {
        public string? sigla { get; set; }
        public string? sigla_serv { get; set; }
        public string? tema { get; set; }
        public double? qtd { get; set; }
        public string? descricaocomercial { get; set; }
        public string? dimensao { get; set; }
        public double? cubagem { get; set; }
        public double? cubagem_total { get; set; }
        public string? observacao { get; set; }
        public string? item { get; set; }
        public string? baia_caminhao { get; set; }
        public string? local {  get; set; }
    }
}
