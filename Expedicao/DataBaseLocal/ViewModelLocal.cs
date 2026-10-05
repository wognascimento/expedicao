using Dapper;
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
            using var db = new DapperDatabase(local: true);
            return await db.Connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM itemFaltantes WHERE barcode = @barcode", new { barcode }) > 0;
        }

        public async Task GetAddItemCarregado(string barcode)
        {
            using var db = new DapperDatabase(local: true);
            await db.InsertAsync(new ItemCarregado { barcode = barcode, enviado = "false" }, generatedKey: false);
        }

        public async Task GetRemoveAllItemCarregado()
        {
            using var db = new DapperDatabase(local: true);
            await db.Connection.ExecuteAsync("DELETE FROM itemCarregado");
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
                    //setor = faltanteModel.Setor,
                    //codigo = (int)faltanteModel.Codigo,
                    //barcode = faltanteModel.Barcode,
                    //codvol = faltanteModel.CodVol,
                    m3_volume = (double)faltanteModel.M3Volume,
                    item_memorial = faltanteModel.ItemMemorial,
                    baia_caminhao = faltanteModel.BaiaCaminhao,
                    endereco = faltanteModel.Endereco,
                    descricao_completa = faltanteModel.DescricaoCompleta
                };
                using var db = new DapperDatabase(local: true);
                await db.InsertAsync(entity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetRemoveAllItemFaltante()
        {
            using var db = new DapperDatabase(local: true);
            await db.Connection.ExecuteAsync("DELETE FROM itemFaltantes");
        }

        public async Task GetRemoveItemFaltante(string barcode)
        {
            using var db = new DapperDatabase(local: true);
            await db.Connection.ExecuteAsync("DELETE FROM itemFaltantes WHERE barcode = @barcode", new { barcode });
        }

        public async Task<IList> Getaitens()
        {
            using var db = new DapperDatabase(local: true);
            var faltantes = await db.ReadAsync<ItemFaltantes>("NOT EXISTS (SELECT 1 FROM itemCarregado c WHERE c.barcode = source.barcode)");
            return faltantes.Select(f => new { f.planilha, f.descricao_completa, f.unidade, f.qtd_expedida,
                f.nome_caixa, f.setor, f.barcode, f.m3_volume, f.item_memorial, f.baia_caminhao, f.endereco }).ToList();
        }

        public async Task<int> GetVolumes()
        {
            using var db = new DapperDatabase(local: true);
            return await db.Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM (SELECT barcode FROM itemFaltantes GROUP BY barcode)");
        }

        public async Task GetAdicionarRomaneio(Romaneio romaneio)
        {
            using var db = new DapperDatabase(local: true);
            await db.InsertAsync(romaneio, generatedKey: false);
        }

        public async Task GetRemoveAllIRomaneios()
        {
            using var db = new DapperDatabase(local: true);
            await db.Connection.ExecuteAsync("DELETE FROM romaneio");
        }

        public async Task<List<string>> GetRomaneios()
        {
            using var db = new DapperDatabase(local: true);
            return (await db.Connection.QueryAsync<string>("SELECT sigla FROM romaneio ORDER BY sigla")).ToList();
        }

        public async Task<string> GetPlaca()
        {
            using var db = new DapperDatabase(local: true);
            return await db.Connection.QueryFirstOrDefaultAsync<string>("SELECT placa FROM romaneio ORDER BY placa LIMIT 1") ?? "";
        }

        public async Task<string> GetConferente()
        {
            using var db = new DapperDatabase(local: true);
            return await db.Connection.QueryFirstOrDefaultAsync<string>("SELECT conferente FROM romaneio ORDER BY conferente LIMIT 1") ?? "";
        }

        public async Task<DateTime> GetDataCarregamento()
        {
            using var db = new DapperDatabase(local: true);
            return await db.Connection.QueryFirstAsync<DateTime>("SELECT data FROM romaneio ORDER BY data LIMIT 1");
        }

        public async Task<List<ItemCarregado>> ItemCarregadosAsync()
        {
            using var db = new DapperDatabase(local: true);
            return await db.ReadAsync<ItemCarregado>();
        }
    }
}
