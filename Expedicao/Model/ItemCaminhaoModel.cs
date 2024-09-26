using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao.Model
{
    [Keyless]
    [Table("qry_item_caminhao", Schema = "expedicao")]
    public class ItemCaminhaoModel
    {
        public string? sigla { get; set; }
        public string? baia_caminhao { get; set; }
        public long? codcompladicional { get; set; }        
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public string? unidade { get; set; }
        public double? qtd { get; set; }
        public string? est { get; set; }        
        public string? cnpj { get; set; }
        public string? cfop { get; set; }
        public double? custo { get; set; }
        public double? peso { get; set; }
        public string? descricaofiscal { get; set; }
        public string? exportado_folhamatic { get; set; }
        public string? ncm { get; set; }
    }
}
