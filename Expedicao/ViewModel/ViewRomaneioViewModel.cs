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
                using DapperDatabase db = new();
                listAsync = (IList)(await db.ReadAsync<ViewRomaneioModel>()).OrderBy<ViewRomaneioModel, string>(x => x.NomeMotorista).GroupBy<ViewRomaneioModel, string>(x => x.NomeMotorista).Select(x => new
                {
                    Motorista = x.Key,
                    Quantidade = x.Count<ViewRomaneioModel>()
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
    }
}
