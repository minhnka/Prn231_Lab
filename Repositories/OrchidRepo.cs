using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataAccess.OrchidDAO;

namespace Repositories
{   
    public class OrchidRepo : IOrchidRepo
    {
        public void Add(Orchid orchid)
        {
            OrchidDAO.Instance.Add(orchid);
        }
        public List<Orchid> GetAll()
        {
            return OrchidDAO.Instance.GetAll1();
        }

        public PagedResult<Orchid> GetAllPaged(
            int page = 1,
            int pageSize = 10,
            string searchTerm = "",
            string sortBy = "OrchidId",
            bool ascending = true,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isNatural = null)
        {
            // Assuming OrchidDAO now has a GetAllPaged method
            return OrchidDAO.Instance.GetAll(page, pageSize, searchTerm, sortBy, ascending,
                categoryId, minPrice, maxPrice, isNatural);
        }

        public Orchid GetById(int id)
        {
            return OrchidDAO.Instance.GetById(id);
        }
        public void Update(Orchid orchid)
        {
            OrchidDAO.Instance.Update(orchid);
        }
        public void Delete(int id)
        {
            OrchidDAO.Instance.Delete(id);
        }
    }
}
