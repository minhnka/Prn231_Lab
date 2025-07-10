using BusinessObjects;
using System.Collections.Generic;

namespace Repositories
{
    public interface IOrchidRepo
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
