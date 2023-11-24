using Microsoft.EntityFrameworkCore;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Expedicao.DataBaseLocal
{
    internal class ViewModelLocal
    {
        public async Task<bool> GetVolume(string barcode)
        {
            bool volume;
            try
            {
                using BaseLocal db = new BaseLocal();
                volume = (await db.ItemFaltantes.Where<ItemFaltantes>(x => x.barcode == barcode).GroupBy<ItemFaltantes, string>(x => x.barcode).Select(x => new
                {
                    barcode = x.Key,
                    Quantidade = x.Count<ItemFaltantes>()
                }).ToListAsync()).Count != 0;
            }
            catch (Exception)
            {
                throw;
            }
            return volume;
        }

        public async Task GetAddItemCarregado(string barcode)
        {
            try
            {
                ItemCarregado entity = new()
                {
                    barcode = barcode,
                    enviado = "false"
                };
                using BaseLocal db = new();
                db.ItemCarregados.Add(entity);
                int num = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetRemoveAllItemCarregado()
        {
            try
            {
                using BaseLocal db = new();
                int num1 = await db.Database.ExecuteSqlRawAsync("DELETE FROM ItemCarregado", new CancellationToken());
                int num2 = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetAddItenFaltante(CarregamentoItenFaltanteModel faltanteModel)
        {
            try
            {
                ItemFaltantes entity = new ItemFaltantes()
                {
                    id_aprovado = (int)faltanteModel.IdAprovado,
                    sigla = faltanteModel.Sigla,
                    planilha = faltanteModel.Planilha,
                    unidade = faltanteModel.Unidade,
                    qtd_expedida = (double)faltanteModel.QtdExpedida,
                    nome_caixa = faltanteModel.NomeCaixa,
                    setor = faltanteModel.Setor,
                    codigo = (int)faltanteModel.Codigo,
                    barcode = faltanteModel.Barcode,
                    codvol = faltanteModel.CodVol,
                    m3_volume = (double)faltanteModel.M3Volume,
                    item_memorial = faltanteModel.ItemMemorial,
                    baia_caminhao = faltanteModel.BaiaCaminhao,
                    endereco = faltanteModel.Endereco,
                    descricao_completa = faltanteModel.DescricaoCompleta
                };
                using BaseLocal db = new();
                db.ItemFaltantes.Add(entity);
                int num = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetRemoveAllItemFaltante()
        {
            try
            {
                string sql = "DELETE FROM ItemFaltantes";
                using BaseLocal db = new();
                int num1 = await db.Database.ExecuteSqlRawAsync(sql, new CancellationToken());
                int num2 = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetRemoveItemFaltante(string barcode)
        {
            try
            {
                string sql = "DELETE FROM ItemFaltantes where barcode = '" + barcode + "'";
                using BaseLocal db = new();
                int num1 = await db.Database.ExecuteSqlRawAsync(sql, new CancellationToken());
                int num2 = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList> Getaitens()
        {
            /*IList listAsync;
            try
            {
                using BaseLocal db = new();
                listAsync = await db.ItemFaltantes.GroupJoin(db.ItemCarregados, (f => f.barcode), (s => s.barcode), (f, set) => new
                {
                    f,
                    set
                }).SelectMany(data => data.set.DefaultIfEmpty<ItemCarregado>(), (data, itens) => data.f).ToListAsync<ItemFaltantes>();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;*/

            

            try
            {
                using BaseLocal db = new();
                var dados = (from f in db.ItemFaltantes
                             join c in db.ItemCarregados on f.barcode equals c.barcode into grouping
                             from ic in grouping.DefaultIfEmpty()
                             where ic.barcode == null
                             select new {
                                 f.planilha,
                                 f.descricao_completa,
                                 f.unidade,
                                 f.qtd_expedida,
                                 f.nome_caixa,
                                 f.setor,
                                 f.barcode,
                                 f.m3_volume,
                                 f.item_memorial,
                                 f.baia_caminhao,
                                 f.endereco
                             });
                return await dados.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> GetVolumes()
        {
            int count;
            try
            {
                using BaseLocal db = new();
                count = (await db.ItemFaltantes.GroupBy<ItemFaltantes, string>(x => x.barcode).Select(x => new
                {
                    Quantidade = x.Count<ItemFaltantes>()
                }).ToListAsync()).Count;
            }
            catch (Exception)
            {
                throw;
            }
            return count;
        }

        public async Task GetAdicionarRomaneio(Romaneio romaneio)
        {
            try
            {
                using BaseLocal db = new();
                db.Entry<Romaneio>(romaneio).State = EntityState.Added;
                int num = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetRemoveAllIRomaneios()
        {
            try
            {
                using BaseLocal db = new();
                int num1 = await db.Database.ExecuteSqlRawAsync("DELETE FROM Romaneio", new CancellationToken());
                int num2 = await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<string>> GetRomaneios()
        {
            List<string> listAsync;
            try
            {
                using BaseLocal db = new();
                listAsync = await db.Romaneios.OrderBy<Romaneio, string>(n => n.sigla).Select<Romaneio, string>(n => n.sigla).ToListAsync<string>();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<string> GetPlaca()
        {
            string placa;
            try
            {
                using BaseLocal db = new();
                List<string> listAsync = await db.Romaneios.OrderBy<Romaneio, string>(r => r.placa).Select<Romaneio, string>(x => x.placa).ToListAsync<string>();
                placa = listAsync.Count <= 0 ? "" : listAsync[0];
            }
            catch (Exception)
            {
                throw;
            }
            return placa;
        }

        public async Task<string> GetConferente()
        {
            string conferente;
            try
            {
                using BaseLocal db = new();
                List<string> listAsync = await db.Romaneios.OrderBy<Romaneio, string>(r => r.conferente).Select<Romaneio, string>(x => x.conferente).ToListAsync<string>();
                conferente = listAsync.Count <= 0 ? "" : listAsync[0];
            }
            catch (Exception)
            {
                throw;
            }
            return conferente;
        }

        public async Task<DateTime> GetDataCarregamento()
        {
            DateTime dataCarregamento;
            try
            {
                using BaseLocal db = new();
                dataCarregamento = await db.Romaneios.OrderBy<Romaneio, DateTime>(r => r.data).Select<Romaneio, DateTime>(x => x.data).FirstAsync<DateTime>();
            }
            catch (Exception)
            {
                throw;
            }
            return dataCarregamento;
        }

        public async Task<List<ItemCarregado>> ItemCarregadosAsync()
        {
            List<ItemCarregado> listAsync;
            try
            {
                using BaseLocal db = new();
                listAsync = await db.ItemCarregados.ToListAsync<ItemCarregado>();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
    }
}
