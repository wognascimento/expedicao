using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expedicao
{
  [Table("t_romaneio", Schema = "expedicao")]
  public class RomaneioModel
  {
    [Key]
    public long? cod_romaneiro { get; set; }
    public DateTime? data_carregamento { get; set; }
    public TimeSpan? hora_chegada { get; set; }
    public long? codtransportadora { get; set; }
    public string? nome_motorista { get; set; }
    public string? placa_caminhao { get; set; }
    public string? placa_cidade { get; set; }
    public string? placa_estado { get; set; }
    public string? placa_carroceria { get; set; }
    public string? placa_carroceria_cidade { get; set; }
    public string? placa_carroceria_estado { get; set; }
    public string? numero_container { get; set; }
    public double? bau_altura { get; set; }
    public double? bau_largura { get; set; }
    public double? bau_profundidade { get; set; }
    public double? m3_carregado { get; set; }
    public double? bau_soba { get; set; }
    public string? condicao_caminhao { get; set; }
    public TimeSpan? inicio_carregamento { get; set; }
    public TimeSpan? termino_carregamento { get; set; }
    public long? numero_caminhao { get; set; }
    public string? shopping_destino { get; set; }
    public string? local_carregamento { get; set; }
    public string? num_lacres { get; set; }
    public string? nome_conferente { get; set; }
    public string? numero_cnh { get; set; }
    public string? telefone_motorista { get; set; }
    public double? m3_portaria { get; set; }
    public string? operacao { get; set; }
    public string? conferente_descarregamento { get; set; }
    public string? lacre_chegada { get; set; }
    public TimeSpan? hora_inicio_descarregamento { get; set; }
    public TimeSpan? hora_termino_descarregamento { get; set; }
    public DateTime? data_inicio_descarregamento { get; set; }
    public DateTime? data_termino_descarregamento { get; set; }
    public DateTime? data_saida_caminhao { get; set; }
    public TimeSpan? hora_saida_caminhao { get; set; }
    public DateTime? data_hora_liberacao { get; set; }
  }
}
