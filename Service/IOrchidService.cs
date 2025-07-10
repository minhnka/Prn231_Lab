using BusinessObjects;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IOrchidService
    {
        void Add(Orchid orchid);
        List<Orchid> GetAll();
        PagedResult<Orchid> GetAllPaged(
            int page = 1,
            int pageSize = 10,
            string searchTerm = "",
            string sortBy = "OrchidId",
            bool ascending = true,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isNatural = null);
        Orchid GetById(int id);
        void Update(Orchid orchid);
        void Delete(int id);
    }
}
