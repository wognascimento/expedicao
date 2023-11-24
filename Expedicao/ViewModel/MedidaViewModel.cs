using Microsoft.EntityFrameworkCore;
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
                using AppDatabase db = new();
                listAsync = (IList<MedidaModel>)await db.Medidas.OrderBy<MedidaModel, string>((Expression<Func<MedidaModel, string>>)(n => n.NomeCaixa)).ToListAsync<MedidaModel>();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }
  }
}
