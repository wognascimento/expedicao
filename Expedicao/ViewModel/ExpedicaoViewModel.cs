using Expedicao.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Expedicao
{
    public class ExpedicaoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));

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
                using AppDatabase db = new();
                listAsync = await db.ProdutoExpedidos
                    .Where(n => n.IdAprovado == IdAprovado)
                    .OrderBy(n => n.ItemMemorial)
                    .ThenBy(n => n.DescricaoProduto)
                    .ToListAsync();
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
                using AppDatabase db = new();
                listAsync = await db.Expeds
                    .Where(n => n.CodDetalhesCompl == CodDetalhesCompl)
                    .OrderBy(n => n.Volume)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<ExpedModel> AddExpedAsync(ExpedModel exped)
        {
            ExpedModel expedModel;
            try
            {
                using AppDatabase db = new();
                db.Entry<ExpedModel>(exped).State = !exped.CodExped.HasValue ? EntityState.Added : EntityState.Modified;
                int num = await db.SaveChangesAsync();
                long? codExped = exped.CodExped;
                expedModel = exped;
            }
            catch (Exception)
            {
                throw;
            }
            return expedModel;
        }

        public async Task DeleteExpedAsync(ExpedModel exped)
        {
            try
            {
                using AppDatabase db = new();
                db.Entry<ExpedModel>(exped).State = EntityState.Deleted;
                int num = await db.SaveChangesAsync();
                db.Entry<ExpedModel>(exped).State = EntityState.Detached;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<EtiquetaVolumeItemModel>> GetEtiquetaVolumesAsync()
        {
            IList<EtiquetaVolumeItemModel> listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = await db.EtiquetaVolumes
                    .OrderBy(n => n.Sigla)
                    .ThenBy(n => n.Sequencia)
                    .ToListAsync();
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
                using AppDatabase db = new();
                listAsync = await db.LiberarImpressaos
                    .Where(n => n.Impresso == true)
                    .OrderBy(n => n.Sigla)
                    .ThenBy(n => n.Planilha)
                    .ThenBy(n => n.NomeCaixa)
                    .ToListAsync();
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
            CaixaModel caixaModel;
            try
            {
                using AppDatabase db = new();
                CaixaModel caixa = db.Caixas
                    .First(c => c.NomeCaixa == liberarImpressao.NomeCaixa);
                caixa.Impresso = Convert.ToInt32(liberarImpressao.Impresso).ToString();
                db.Caixas.Update(caixa);
                int num = await db.SaveChangesAsync();
                caixaModel = caixa;
            }
            catch (Exception)
            {
                throw;
            }
            return caixaModel;
        }

        public async Task<List<CaminaoModel>> GetCaminhoesAsync(List<string> siglas)
        {
            List<CaminaoModel> caminhoesAsync;
            try
            {
                using AppDatabase db = new();
                List<CaminaoModel> list = new();
                var source = db.CarregamentoItenFaltantes
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


                foreach (var data in await source.ToListAsync())
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
                using AppDatabase db = new();
                listAsync = await db.CarregamentoItenFaltantes
                    .Where(s => s.Sigla == sigla && s.BaiaCaminhao == caminhao)
                    .ToListAsync();
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
                using AppDatabase db = new();
                listAsync = await db.CarregamentoItenFaltantes
                    .Where(s => s.Sigla == sigla)
                    .ToListAsync();
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
                using AppDatabase db = new();
                listAsync = await db.PreConferenciaItemFaltantes
                    .Where(s => s.sigla == sigla)
                    .ToListAsync();
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
                using AppDatabase db = new();
                listAsync = await db.PreConferenciaItemShopps
                    .Where(s => s.sigla == sigla)
                    .ToListAsync();
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
                using AppDatabase db = new();
                listAsync = await db.CarregamentoItenShopps
                    .Where(s => s.Sigla == sigla)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task GetAddVolumeCarregado(ConfCargaGeralModel model)
        {
            try
            {
                using AppDatabase db = new();
                if (await db.ConfCargaGerals.FirstOrDefaultAsync(c => c.Barcode == model.Barcode) == null)
                {
                    EntityEntry entityEntry = await db.ConfCargaGerals.AddAsync(model);
                }
                int num = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<long> OrcamentoSequenceAsync(OrcamentoSequenceModel orcamento)
        {
            long num1;
            try
            {
                using AppDatabase db = new();
                db.OrcamentoSequences.Add(orcamento);
                int num2 = await db.SaveChangesAsync();
                num1 = orcamento.NumeroOrcamento.Value;
            }
            catch (Exception)
            {
                throw;
            }
            return num1;
        }

        public async Task<IList> GetCarregamentoItemCaminhaosSemParemetroAsync(
          List<string> siglas)
        {
            IList listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = await db.CarregamentoItemCaminhaos
                    .Where(x => siglas.Contains(x.SiglaServ) && x.Custo == (double?)0.0 || x.Peso == new double?())
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
                        CodComplAdicional = x.Key.CodComplAdicional,
                        DescricaoFiscal = x.Key.DescricaoFiscal,
                        Unidade = x.Key.Unidade,
                        Custo = x.Sum(t => t.Custo),
                        Peso = x.Sum(t => t.Peso)
                    }).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<IList> GetCarregamentoItemCaminhaosNaoExportadoMaticAsync(
          List<string> siglas)
        {
            IList listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = await db.CarregamentoItemCaminhaos
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

                        IDENTIFICACAO               = x.Key.CodComplAdicional,
                        DESCRICAO                   = x.Key.DescricaoFiscal,
                        NCM                         = x.Key.Ncm,
                        CODBARRA                    = "",
                        UNIDADEDECOMPRA             = x.Key.Unidade,
                        UNIDADEVENDA                = x.Key.Unidade,
                        SITUACAOTRIBUTARIAA         = "0",
                        SITUACAOTRIBUTARIAB         = "41",
                        CSOSN                       = "",
                        SITTRIBPIS                  = "PIS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBCOFINS               = "COFINS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBIPI                  = "IPI 99 - Outras saídas",
                        IPI                         = "0",
                        ICMS                        = "",
                        REDUCAOICMS                 = "",
                        ALIQCOFINS                  = "0",
                        ALIQPIS                     = "",
                        CATEGORIA                   = "",
                        CEST                        = "",
                        CFOP                        = "",
                        CODIGODEBENEFICIOFISCAL     = "",
                        COMISSAODEVENDA             = "",
                        CUSTO                       = "",
                        ESTOQUECOMPRA               = "",
                        ESTOQUEMAXIMO               = "",
                        ESTOQUEMINIMO               = "",
                        FATORUNIDDEVENDA            = "1",
                        ATIVO                       = "Sim",
                        INDICADORDEESCALARELEVANTE  = "",
                        CNPJFABRICANTE              = "",
                        PESO                        = "",
                        MATERIAPRIMA                = "FALSO",
                        PARAVENDA                   = "VERDADEIRO",
                        MOEDA                       = "R$",
                        OBSERVACOES                 = "",
                        PRECODEVENDA1               = "",
                        PRECODEVENDA2               = "",
                        TIPODOPRODUTO               = "3",
                        PRODUTOTERCEIRO             = "FALSO",
                        CODTRIBUTACAONOSISTEMA      = "7",
                        CODENQUADRAMENTOIPI         = "999",
                        OPERACAOFATORCONVERSAO      = ""
                    })
                    .ToListAsync();
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
                using AppDatabase db = new();
                listAsync = await db.CarregamentoItemCaminhaos
                    .Where(x => siglas.Contains(x.SiglaServ))
                    .Where(c => c.Caminhao.Contains(caminhao))
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
                    .ToListAsync();
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
                using AppDatabase db = new();
                var listAsync = await db.VolumesSolicitacaoNotaFiscal
                    .Where(x => siglas.Contains(x.sigla))
                    .Where(c => c.caminhao.Contains(caminhao))
                    .OrderBy(x => x.codexped)
                    .ToListAsync();

                return listAsync;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetVolumeCarregado(long? codexped)
        {
            try
            {
                using AppDatabase db = new();
                var volume = await db.Expeds.FirstOrDefaultAsync(v => v.CodExped == codexped);
                if (volume != null)
                {
                    volume.nf_emitida = true;
                    db.Entry(volume).Property(v => v.nf_emitida).IsModified = true;
                    await db.SaveChangesAsync();
                }
                
                //EntityEntry entityEntry = await db.Expeds.AddAsync(model);
                
                //int num = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<ResumoNotaModel>> GetInformasoesNfAsync(List<string> siglas, string caminhao)
        {
            List<ResumoNotaModel> listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = await db.RelatorioNfTotalCompletos
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
                    .ToListAsync();
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
                using AppDatabase db = new AppDatabase();
                listAsync = await db.PacklistCarregCaminhaos
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
                        x.bruto
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
    }
}
