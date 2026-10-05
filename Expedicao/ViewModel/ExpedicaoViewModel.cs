using Expedicao.Model;
using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Expedicao
{
    public class ExpedicaoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        }

        private List<object> _searchItem;
        public List<object> SearchItem
        {
            get { return _searchItem; }
            set { _searchItem = value; }
        }

        public ExpedicaoViewModel()
        {
            _searchItem = new List<object>();
        }

        public async Task<IList<ProdutoExpedidoModel>> GetProdutoExpedidos(
          int IdAprovado)
        {
            IList<ProdutoExpedidoModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<ProdutoExpedidoModel>("\"IdAprovado\" = @IdAprovado", new { IdAprovado }))
                    .Where(n => n.IdAprovado == IdAprovado)
                    .OrderBy(n => n.ItemMemorial)
                    .ThenBy(n => n.DescricaoProduto)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList<ExpedModel>> GetExpedsAsync(int CodDetalhesCompl)
        {
            IList<ExpedModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<ExpedModel>("\"CodDetalhesCompl\" = @CodDetalhesCompl", new { CodDetalhesCompl }))
                    .Where(n => n.CodDetalhesCompl == CodDetalhesCompl)
                    .OrderBy(n => n.Volume)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<ExpedModel> AddExpedAsync(ExpedModel exped)
        {
            return await new Views.ExpedicaoProdutoViewModel().AddExpedAsync(exped);
        }

        public async Task DeleteExpedAsync(ExpedModel exped)
        {
            await new Views.ExpedicaoProdutoViewModel().DeleteExpedAsync(exped);
        }

        public async Task<IList<EtiquetaVolumeItemModel>> GetEtiquetaVolumesAsync()
        {
            IList<EtiquetaVolumeItemModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<EtiquetaVolumeItemModel>())
                    .OrderBy(n => n.Sigla)
                    .ThenBy(n => n.Sequencia)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList<LiberarImpressaoModel>> GetLiberarImpressaosAsync()
        {
            IList<LiberarImpressaoModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<LiberarImpressaoModel>("\"Impresso\" = TRUE"))
                    .Where(n => n.Impresso == true)
                    .OrderBy(n => n.Sigla)
                    .ThenBy(n => n.Planilha)
                    .ThenBy(n => n.NomeCaixa)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<CaixaModel> LiberarImpresaoAsync(
          LiberarImpressaoModel liberarImpressao)
        {
            using var db = new DapperDatabase();
            var caixa = (await db.ReadAsync<CaixaModel>("\"NomeCaixa\" = @nome", new { nome = liberarImpressao.NomeCaixa })).First();
            caixa.Impresso = Convert.ToInt32(liberarImpressao.Impresso).ToString();
            await db.UpdateAsync(caixa, nameof(CaixaModel.Impresso));
            return caixa;
        }

        public async Task<List<CaminaoModel>> GetCaminhoesAsync(List<string> siglas)
        {
            List<CaminaoModel> caminhoesAsync;
            try
            {
                using DapperDatabase db = new();
                List<CaminaoModel> list = new();
                var source = (await db.ReadAsync<CarregamentoItenFaltanteModel>("\"Sigla\" = ANY(@siglas)", new { siglas = siglas.ToArray() }))
                    .Where(x => siglas.Contains(x.Sigla))
                    .OrderBy(x => x.BaiaCaminhao)
                    .GroupBy(x => new
                    {
                        x.Sigla,
                        x.BaiaCaminhao
                    })
                    .Select(x => new
                    {
                        sigla = x.Key.Sigla,
                        caminao = x.Key.BaiaCaminhao,//x.Key.BaiaCaminhao ?? "0",
                        volumes = x.Count(),
                        selecao = true
                    });


                foreach (var data in source.ToList())
                    list.Add(new CaminaoModel()
                    {
                        sigla = data.sigla,
                        caminao = data.caminao,
                        selecao = data.selecao,
                        volumes = data.volumes
                    });
                caminhoesAsync = list;
            }
            catch (Exception)
            {
                throw;
            }
            return caminhoesAsync;
        }

        public async Task<IList<CarregamentoItenFaltanteModel>> GetItensFaltanteAsync(
          string sigla,
          string caminhao)
        {
            IList<CarregamentoItenFaltanteModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<CarregamentoItenFaltanteModel>("\"Sigla\" = @sigla", new { sigla }))
                    .Where(s => s.Sigla == sigla && s.BaiaCaminhao == caminhao)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList<CarregamentoItenFaltanteModel>> GetItensFaltanteAsync(string sigla)
        {
            IList<CarregamentoItenFaltanteModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                /*
                listAsync = (await db.ReadAsync<CarregamentoItenFaltanteModel>("\"Sigla\" = @sigla", new { sigla }))
                    .Where(s => s.Sigla == sigla)
                    .ToList();
                */

                listAsync = (from f in (await db.ReadAsync<CarregamentoItenFaltanteModel>("\"Sigla\" = @sigla", new { sigla }))
                             where f.Sigla == sigla
                             select f).ToList();

            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList<PreConferenciaItemFaltanteModel>> GetPreItensFaltanteAsync(string sigla)
        {
            IList<PreConferenciaItemFaltanteModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<PreConferenciaItemFaltanteModel>("\"sigla\" = @sigla", new { sigla }))
                    .Where(s => s.sigla == sigla)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList<PreConferenciaItemShoppModel>> GetPreItensShoppAsync(string sigla)
        {
            IList<PreConferenciaItemShoppModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<PreConferenciaItemShoppModel>("\"sigla\" = @sigla", new { sigla }))
                    .Where(s => s.sigla == sigla)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList<CarregamentoItenShoppModel>> GetCarregamentoItensAsync(
          string sigla)
        {
            IList<CarregamentoItenShoppModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<CarregamentoItenShoppModel>("\"Sigla\" = @sigla", new { sigla }))
                    .Where(s => s.Sigla == sigla)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task GetAddVolumeCarregado(ConfCargaGeralModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Barcode)) throw new InvalidOperationException("Informe o codigo de barras.");
            using var db = new DapperDatabase();
            await db.Connection.ExecuteAsync(@"INSERT INTO expedicao.t_conf_carga_geral
                (barcode, doca_origem, data, shopp, resp, caminhao, entradasistema, alterado_por, data_altera, inserido_por, inserido_em)
                VALUES (@Barcode, @DocaOrigem, @Data, @Shopp, @Resp, @Caminhao, @EntradaSistema, @AlteradoPor, @DataAltera, @InseridoPor, @InseridoEm)
                ON CONFLICT (barcode) DO NOTHING", model);
        }

        public async Task AtualizarVolumesCarregadosAsync(IEnumerable<string> barcodes, string caminhao, DateTime dataCarregamento)
        {
            using var db = new DapperDatabase();
            await db.Connection.OpenAsync();
            using var transaction = await db.Connection.BeginTransactionAsync();
            foreach (var barcode in barcodes.Where(b => !string.IsNullOrWhiteSpace(b)).Distinct())
            {
                var count = await db.Connection.ExecuteAsync(@"UPDATE expedicao.t_conf_carga_geral
                    SET caminhao = @caminhao, data = @dataCarregamento, data_altera = @alteradoEm, alterado_por = @usuario
                    WHERE barcode = @barcode", new { caminhao, dataCarregamento, barcode, alteradoEm = DateTime.Now.Date, usuario = Environment.UserName }, transaction);
                if (count != 1) throw new InvalidOperationException($"Existe codigo nao carregado na lista enviada: {barcode}");
            }
            await transaction.CommitAsync();
        }

        public async Task<long> OrcamentoSequenceAsync(OrcamentoSequenceModel orcamento)
        {
            using var db = new DapperDatabase();
            await db.InsertAsync(orcamento);
            return orcamento.NumeroOrcamento!.Value;
        }

        public async Task<IList> GetCarregamentoItemCaminhaosSemParemetroAsync(
          List<string> siglas)
        {
            IList listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<CarregamentoItemCaminhaoModel>("\"SiglaServ\" = ANY(@siglas)", new { siglas = siglas.ToArray() }))
                    .Where(x => siglas.Contains(x.SiglaServ) && (x.Custo == (double?)0.0 || x.Custo == new double?()) || x.Peso == new double?())
                    .GroupBy(x => new
                    {
                        x.CodComplAdicional,
                        x.DescricaoFiscal,
                        x.Qtd,
                        x.Custo,
                        x.Peso,
                        x.Unidade
                    })
                    .OrderBy(x => x.Key.DescricaoFiscal).Select(x => new
                    {
                        x.Key.CodComplAdicional,
                        x.Key.DescricaoFiscal,
                        x.Key.Unidade,
                        Custo = x.Sum(t => t.Custo),
                        Peso = x.Sum(t => t.Peso)
                    }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        private static string RemoverAcentos(string texto)
        {
            // Normaliza o texto para separar os acentos das letras
            string textoNormalizado = texto.Normalize(NormalizationForm.FormD);

            // Expressão regular para remover caracteres não-ASCII (acentos)
            Regex regexAcentos = new Regex(@"\p{IsCombiningDiacriticalMarks}+");

            // Remove os acentos
            return regexAcentos.Replace(textoNormalizado, "").Normalize(NormalizationForm.FormC);
        }

        public async Task<IList> GetCarregamentoItemCaminhaosNaoExportadoMaticAsync(
          List<string> siglas)
        {
            IList listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<CarregamentoItemCaminhaoModel>("\"SiglaServ\" = ANY(@siglas)", new { siglas = siglas.ToArray() }))
                    .Where(x => siglas.Contains(x.SiglaServ) && x.ExportadoFolhamatic == null)
                    .GroupBy(x => new
                    {
                        x.CodComplAdicional,
                        x.DescricaoFiscal,
                        x.Qtd,
                        x.Custo,
                        x.Peso,
                        x.Unidade,
                        x.Ncm
                    })
                    .OrderBy(x => x.Key.DescricaoFiscal)
                    .Select(x => new
                    {
                        //x.Key.CodComplAdicional,
                        //x.Key.DescricaoFiscal,
                        //x.Key.Unidade

                        IDENTIFICACAO = x.Key.CodComplAdicional,
                        DESCRICAO = RemoverAcentos(x.Key.DescricaoFiscal),
                        NCM = x.Key.Ncm,
                        CODBARRA = "",
                        UNIDADEDECOMPRA = x.Key.Unidade,
                        UNIDADEVENDA = x.Key.Unidade,
                        SITUACAOTRIBUTARIAA = "0",
                        SITUACAOTRIBUTARIAB = "41",
                        CSOSN = "",
                        SITTRIBPIS = "PIS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBCOFINS = "COFINS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBIPI = "IPI 99 - Outras saídas",
                        IPI = "0",
                        ICMS = "",
                        REDUCAOICMS = "",
                        ALIQCOFINS = "",
                        ALIQPIS = "",
                        CATEGORIA = "",
                        CEST = "",
                        CFOP = "",
                        CODIGODEBENEFICIOFISCAL = "SP070090",
                        COMISSAODEVENDA = "",
                        CUSTO = "",
                        ESTOQUECOMPRA = "",
                        ESTOQUEMAXIMO = "",
                        ESTOQUEMINIMO = "",
                        FATORUNIDDEVENDA = "1",
                        ATIVO = "Sim",
                        INDICADORDEESCALARELEVANTE = "",
                        CNPJFABRICANTE = "",
                        PESO = "",
                        MATERIAPRIMA = "FALSO",
                        PARAVENDA = "VERDADEIRO",
                        MOEDA = "R$",
                        OBSERVACOES = "",
                        PRECODEVENDA1 = "",
                        PRECODEVENDA2 = "",
                        TIPODOPRODUTO = "3",
                        PRODUTOTERCEIRO = "FALSO",
                        CODTRIBUTACAONOSISTEMA = "7",
                        CODENQUADRAMENTOIPI = "999",
                        OPERACAOFATORCONVERSAO = ""
                    })
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<List<ItemNotaModel>> GetCarregamentoItemCaminhaosAsync(List<string> siglas, string caminhao)
        {
            List<ItemNotaModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<CarregamentoItemCaminhaoModel>("\"SiglaServ\" = ANY(@siglas) AND \"Caminhao\" = @caminhao", new { siglas = siglas.ToArray(), caminhao }))
                    .Where(x => siglas.Contains(x.SiglaServ))
                    .Where(c => c.Caminhao == caminhao)
                    .GroupBy(x => new
                    {
                        x.CodComplAdicional,
                        x.DescricaoFiscal,
                        x.Qtd,
                        x.Custo,
                        x.Unidade
                    })
                    .OrderBy(x => x.Key.DescricaoFiscal)
                    .Select(y => new ItemNotaModel
                    {
                        CodComplAdicional = y.Key.CodComplAdicional,
                        DescricaoFiscal = y.Key.DescricaoFiscal,
                        Qtd = y.Key.Qtd,
                        Custo = y.Key.Custo,
                        Unidade = y.Key.Unidade,
                        volumes = y.Count()
                    })
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<List<VolumeSolicitacaoNotaFiscalModel>> GetCarregamentoVolumesAsync(List<string> siglas, string caminhao)
        {
            try
            {
                using DapperDatabase db = new();
                var listAsync = (await db.ReadAsync<VolumeSolicitacaoNotaFiscalModel>("\"sigla\" = ANY(@siglas) AND \"caminhao\" = @caminhao", new { siglas = siglas.ToArray(), caminhao }))
                    .Where(x => siglas.Contains(x.sigla))
                    .Where(c => c.caminhao == caminhao)
                    .OrderBy(x => x.codexped)
                    .ToList();

                return listAsync;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task GetVolumeCarregado(long? codexped)
        {
            return MarcarVolumesNotaEmitidaAsync(new[] { codexped });
        }

        public async Task MarcarVolumesNotaEmitidaAsync(IEnumerable<long?> codigos)
        {
            var ids = codigos.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToArray();
            if (ids.Length == 0) return;
            using var db = new DapperDatabase();
            await db.Connection.ExecuteAsync("UPDATE expedicao.t_exped SET nf_emitida = TRUE WHERE codexped = ANY(@ids)", new { ids });
        }


        public async Task<List<ResumoNotaModel>> GetInformasoesNfAsync(List<string> siglas, string caminhao)
        {
            List<ResumoNotaModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<RelatorioNfTotalCompletoModel>("\"Shopp\" = ANY(@siglas) AND \"Caminhao\" = @caminhao", new { siglas = siglas.ToArray(), caminhao }))
                    .Where(x => siglas.Contains(x.Shopp))
                    .Where(x => x.Caminhao.Equals(caminhao))
                    .GroupBy(x => new
                    {
                        x.Shopp,
                        x.Nome,
                        x.Caminhao,
                        x.Data,
                        x.NomeTransportadora,
                        x.Cnpj,
                        x.Ie,
                        x.Endereco,
                        x.Bairro,
                        x.Cidade,
                        x.Cep,
                        x.Uf
                    })
                    .Select(x => new ResumoNotaModel
                    {
                        Shopp                 = x.Key.Shopp,
                        Nome                  = x.Key.Nome,
                        Caminhao              = x.Key.Caminhao,
                        Data                  = x.Key.Data,
                        NomeTransportadora    = x.Key.NomeTransportadora,
                        Cnpj                  = x.Key.Cnpj,
                        Ie                    = x.Key.Ie,
                        Endereco              = x.Key.Endereco,
                        Bairro                = x.Key.Bairro,
                        Cidade                = x.Key.Cidade,
                        Cep                   = x.Key.Cep,
                        Uf                    = x.Key.Uf,
                        volumes               = x.Count(),
                        Bruto                 = x.Sum(t => t.Bruto),
                        Liquido               = x.Sum(t => t.Liquido),
                        Preco                 = x.Sum(t => t.Preco)
                    })
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList> GetPacklistCarregCaminhaoAsync(string sigla, string caminhao, DateTime data)
        {
            IList listAsync;
            try
            {
                using DapperDatabase db = new DapperDatabase();
                listAsync = (await db.ReadAsync<PacklistCarregCaminhaoModel>("\"sigla\" = @sigla AND \"caminhao\" = @caminhao", new { sigla, caminhao }))
                    .Where<PacklistCarregCaminhaoModel>(x => x.sigla == sigla && x.caminhao == caminhao)
                    .Select(x => new
                    {
                        cod = x.coddetalhescompl,
                        x.local_shoppings,
                        x.nome_caixa,
                        qtd = x.qtd_expedida,
                        x.planilha,
                        x.descricao_completa,
                        x.liquido,
                        x.bruto,
                        controlado = x.prodcontrolado == "0" ? "NÃO" : "SIM"
                    })
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
    }
}
