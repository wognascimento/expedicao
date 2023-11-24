using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Table("c_cubagem_prevista_cliente", Schema = "expedicao")]
    public class CubagemPrevistaClienteModel
    {
        [Key]
        public long? cod_linha_qdfecha {  get; set; }
        public string? sigla { get; set; }
        public string? sigla_serv { get; set; }
        public string? tema { get; set; }
        public string? bloco { get; set; }
        public string? item { get; set; }
        public double? qtd { get; set; }
        public string? local { get; set; }
        public string? detalhe_local { get; set; }
        public long? coddimensao {  get; set; }
        public string? descricaocomercial { get; set; }
        public string? dimensao { get; set; }
        public double? cubagem { get; set; }
        public double? cubagem_total { get; set; }
        public string? observacao { get; set; }
    }
}
