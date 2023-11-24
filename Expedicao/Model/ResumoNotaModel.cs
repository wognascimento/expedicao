using System;

namespace Expedicao
{
  public class ResumoNotaModel
  {
    public string? Shopp { get; set; }
    public string? Nome { get; set; }
    public string? Caminhao { get; set; }
    public DateTime? Data { get; set; }
    public string? NomeTransportadora { get; set; }
    public string? Cnpj { get; set; }
    public string? Ie { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Cep { get; set; }
    public string? Uf { get; set; }
    public int? volumes { get; set; }
    public double? Bruto { get; set; }
    public double? Liquido { get; set; }
    public double? Preco { get; set; }
  }
}
