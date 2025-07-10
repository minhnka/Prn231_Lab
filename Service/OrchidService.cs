using BusinessObjects;
using DataAccess;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class OrchidService : IOrchidService
    {
        private readonly IOrchidRepo _orchidRepo;

        public OrchidService(IOrchidRepo orchidRepo)
        {
            _orchidRepo = orchidRepo;
        }

        public void Add(Orchid orchid)
        {
            _orchidRepo.Add(orchid);
        }

        public List<Orchid> GetAll()
        {
            return _orchidRepo.GetAll();
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
            return _orchidRepo.GetAllPaged(page, pageSize, searchTerm, sortBy, ascending,
                categoryId, minPrice, maxPrice, isNatural);
        }

        public Orchid GetById(int id)
        {
            return _orchidRepo.GetById(id);
        }

        public void Update(Orchid orchid)
        {
            _orchidRepo.Update(orchid);
        }

        public void Delete(int id)
        {
            _orchidRepo.Delete(id);
        }
    }
}
