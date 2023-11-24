using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("qry_pre_conferencia_item_faltante", Schema = "expedicao")]
    public class PreConferenciaItemFaltanteModel
    {
        public string? sigla { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public string? unidade { get; set; }
        public double? qtd_expedida { get; set; }
        public string? nome_caixa { get; set; }
        public string? setor { get; set; }
        public long? codigo { get; set; }
        public string? barcode { get; set; }
        public string? codvol { get; set; }
        public string? item_memorial { get; set; }
        public string? baia_caminhao { get; set; }
        public string? endereco { get; set; }
    }
}
