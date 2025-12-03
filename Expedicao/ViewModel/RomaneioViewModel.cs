using Microsoft.EntityFrameworkCore;
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
            try
            {
                using AppDatabase db = new AppDatabase();
                //db.Entry<RomaneioModel>(romaneio).State = !romaneio.CodRomaneiro.HasValue ? EntityState.Added : EntityState.Modified;
                await db.Romaneios.SingleMergeAsync(romaneio);
                int num = await db.SaveChangesAsync();
                long? codRomaneiro = romaneio.cod_romaneiro;
            }
            catch (Exception)
            {
                throw;
            }
            return romaneio;
        }

        public async Task<List<RomaneioModel>> GetRomaneiosAsync()
        {
            List<RomaneioModel> listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = await db.Romaneios.OrderBy<RomaneioModel, string>((Expression<Func<RomaneioModel, string>>)(n => n.shopping_destino)).ThenBy<RomaneioModel, long>(n => (long)n.numero_caminhao).ToListAsync<RomaneioModel>();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
    }
}
