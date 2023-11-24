using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace Expedicao
{
    internal class ViewRomaneioViewModel
    {
        public static async Task<IList> GetMotoristas()
        {
            IList listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = (IList)await db.ViewRomaneios.OrderBy<ViewRomaneioModel, string>(x => x.NomeMotorista).GroupBy<ViewRomaneioModel, string>(x => x.NomeMotorista).Select(x => new
                {
                    Motorista = x.Key,
                    Quantidade = x.Count<ViewRomaneioModel>()
                }).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
    }
}
