using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("c_bd_cubagem_dia_dase_produto_data", Schema = "expedicao")]
    public class ProdutosBaiadosGeralTotalDataModel
    {
        public string? sigla { get; set; }
        public DateTime? data { get; set; }
        public long? codcompladicional { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public double? cubagem { get; set; }
        public double? qtd_expedida {  get; set; }
    }
}
