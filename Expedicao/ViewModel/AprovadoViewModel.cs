using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Expedicao
{
    internal class AprovadoViewModel
    {
        public async Task<IList<AprovadoModel>> GetAprovados()
        {
            IList<AprovadoModel> listAsync;
            try
            {
                using DapperDatabase db = new();
                listAsync = (await db.ReadAsync<AprovadoModel>()).OrderBy(c => c.SiglaServ).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return listAsync;
        }

        public async Task<AprovadoModel> GetAprovadoAsync(string SiglaServ)
        {
            AprovadoModel aprovadoAsync;
            try
            {
                using DapperDatabase db = new();
                aprovadoAsync = (await db.ReadAsync<AprovadoModel>("\"SiglaServ\" = @SiglaServ", new { SiglaServ })).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
          return aprovadoAsync;
        }
    }
}
