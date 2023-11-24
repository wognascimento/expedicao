using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("c_bd_cubagem_semana_ano_anterior_atual", Schema = "expedicao")]
    public class CubagemSemanaAnoAnteriorAtualModel
    {
        public double? semana { get; set; }
        public double? cubagem_atual { get; set; }
        public double? cubagem_anterior { get; set; }
        public double? referencia_ano_anterior {  get; set; }
    }
}
