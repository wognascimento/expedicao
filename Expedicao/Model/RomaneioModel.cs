using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("t_romaneio", Schema = "expedicao")]
  public class RomaneioModel
  {
    [Key]
    [Column("cod_romaneiro")]
    public long? CodRomaneiro { get; set; }
    [Column("data_carregamento")]
    public DateTime? DataCarregamento { get; set; }
    [Column("hora_chegada")]
    public TimeSpan? HoraChegada { get; set; }
    [Column("codtransportadora")]
    public long? CodTransportadora { get; set; }
    [Column("nome_motorista")]
    public string? NomeMotorista { get; set; }
    [Column("placa_caminhao")]
    public string? PlacaCaminhao { get; set; }
    [Column("placa_cidade")]
    public string? PlacaCidade { get; set; }
    [Column("placa_estado")]
    public string? PlacaEstado { get; set; }
    [Column("placa_carroceria")]
    public string? PlacaCarroceria { get; set; }
    [Column("placa_carroceria_cidade")]
    public string? PlacaCarroceriaCidade { get; set; }
    [Column("placa_carroceria_estado")]
    public string? PlacaCarroceriaEstado { get; set; }
    [Column("numero_container")]
    public string? NumeroContainer { get; set; }
    [Column("bau_altura")]
    public double? BauAltura { get; set; }
    [Column("bau_largura")]
    public double? BauLargura { get; set; }
    [Column("bau_profundidade")]
    public double? BauProfundidade { get; set; }
    [Column("m3_carregado")]
    public double? M3Carregado { get; set; }
    [Column("bau_soba")]
    public double? BauSoba { get; set; }
    [Column("condicao_caminhao")]
    public string? CondicaoCaminhao { get; set; }
    [Column("inicio_carregamento")]
    public TimeSpan? InicioCarregamento { get; set; }
    [Column("termino_carregamento")]
    public TimeSpan? TerminoCarregamento { get; set; }
    [Column("numero_caminhao")]
    public long? NumeroCaminhao { get; set; }
    [Column("shopping_destino")]
    public string? ShoppingDestino { get; set; }
    [Column("local_carregamento")]
    public string? LocalCarregamento { get; set; }
    [Column("num_lacres")]
    public string? NumLacres { get; set; }
    [Column("nome_conferente")]
    public string? NomeConferente { get; set; }
    [Column("numero_cnh")]
    public string? NumeroCnh { get; set; }
    [Column("telefone_motorista")]
    public string? TelefoneMotorista { get; set; }
    [Column("m3_portaria")]
    public double? M3Portaria { get; set; }
    [Column("operacao")]
    public string? Operacao { get; set; }
    [Column("conferente_descarregamento")]
    public string? ConferenteDescarregamento { get; set; }
    [Column("lacre_chegada")]
    public string? LacreChegada { get; set; }
    [Column("hora_inicio_descarregamento")]
    public TimeSpan? HoraInicioDescarregamento { get; set; }
    [Column("hora_termino_descarregamento")]
    public TimeSpan? HoraTerminoDescarregamento { get; set; }
    [Column("data_inicio_descarregamento")]
    public DateTime? DataInicioDescarregamento { get; set; }
    [Column("data_termino_descarregamento")]
    public DateTime? DataTerminoDescarregamento { get; set; }
    [Column("data_saida_caminhao")]
    public DateTime? DataSaidaCaminhao { get; set; }
    [Column("hora_saida_caminhao")]
    public TimeSpan? HoraSaidaCaminhao { get; set; }
    [Column("data_hora_liberacao")]
    public DateTime? DataHoraLiberacao { get; set; }
  }
}
