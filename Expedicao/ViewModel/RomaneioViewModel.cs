using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Expedicao
{
    internal class RomaneioViewModel
    {

        public async Task<RomaneioModel> SaveAsync(RomaneioModel romaneio)
        {
            var vm = new Views.RomaneioViewModel();
            vm.Tranportadoras = await vm.GetTransportadoras();
            return await vm.SaveAsync(romaneio);
        }

        public async Task<List<RomaneioModel>> GetRomaneiosAsync()
        {
            List<RomaneioModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<RomaneioModel>()).OrderBy(n => n.shopping_destino).ThenBy(n => n.numero_caminhao).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
    }
}
