using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Expedicao
{
    internal class TranportadoraViewModel
    {
        public async Task<IList<TranportadoraModel>> GetTransportadoras()
        {
            IList<TranportadoraModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (IList<TranportadoraModel>)(await db.ReadAsync<TranportadoraModel>()).OrderBy<TranportadoraModel, string>(c => c.NomeTransportadora).ToList<TranportadoraModel>();
            }
            catch (Exception)
            {
                throw;
            }
          return listAsync;
        }
    }
}
