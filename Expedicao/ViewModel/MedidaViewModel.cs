using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Expedicao
{
  internal class MedidaViewModel
  {
        public async Task<IList<MedidaModel>> GetMedidasAsync()
        {
            IList<MedidaModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<MedidaModel>()).OrderBy(n => n.NomeCaixa).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
  }
}
