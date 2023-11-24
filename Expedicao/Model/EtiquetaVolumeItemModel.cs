using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
    [Keyless]
    [Table("view_etiqueta_volumes_itens", Schema = "expedicao")]
      public class EtiquetaVolumeItemModel
      {
        [Column("coddetalhescompl")]
        public long? CodDetalhesCompl { get; set; }
        [Column("sigla")]
        public string? Sigla { get; set; }
        [Column("codvol")]
        public string? CodVol { get; set; }
        [Column("setor")]
        public string? Setor { get; set; }
        [Column("planilha")]
        public string? Planilha { get; set; }
        [Column("descricao")]
        public string? Descricao { get; set; }
        [Column("qtd_expedida")]
        public double? QtdExpedida { get; set; }
        [Column("codcompladicional")]
        public long? CodComplAdicional { get; set; }
        [Column("local_shoppings")]
        public string? LocalShoppings { get; set; }
        [Column("barcode")]
        public string? Barcode { get; set; }
        [Column("baia_caminhao")]
        public string? BaiaCaminhao { get; set; }
        [Column("sequencia")]
        public long? Sequencia { get; set; }
        [Column("pl")]
        public double? Pl { get; set; }
        [Column("pb")]
        public double? Pb { get; set; }
        [Column("vol_exp")]
        public int? VolExp { get; set; }
        [Column("vol_tot_exp")]
        public int? VolTotExp { get; set; }
        [Column("impresso")]
        public string? Impresso { get; set; }
        [Column("item_memorial")]
        public string? ItemMemorial { get; set; }
        [Column("anexo")]
        public bool? Anexo { get; set; }
        [Column("controlado")]
        public bool? Controlado { get; set; }
        [Column("volume")]
        public long? Volume { get; set; }
      }
}
