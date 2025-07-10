using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class OrchidDAO
    {
        private static OrchidDAO instance;
        private static ProductManagementDbContext dbContext;

        public OrchidDAO()
        {
            dbContext = new ProductManagementDbContext();
        }
        public static OrchidDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OrchidDAO();
                }
                return instance;
            }
        }

        public void Add(Orchid orchid)
        {
            dbContext.Orchids.Add(orchid);
            dbContext.SaveChanges();
        }

        public PagedResult<Orchid> GetAll(
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
            IQueryable<Orchid> query = dbContext.Orchids.Include(o => o.Category);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(o =>
                    (o.OrchidName != null && o.OrchidName.ToLower().Contains(searchTerm)) ||
                    (o.OrchidDescription != null && o.OrchidDescription.ToLower().Contains(searchTerm)));
            }

            if (categoryId.HasValue)
                query = query.Where(o => o.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(o => o.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(o => o.Price <= maxPrice.Value);

            if (isNatural.HasValue)
                query = query.Where(o => o.IsNatural == isNatural.Value);

            int totalCount = query.Count();

            query = ApplySorting(query, sortBy, ascending);

            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<Orchid>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                PageCount = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        // Keep the original method for backward compatibility
        public List<Orchid> GetAll1()
        {
            return GetAll(1, int.MaxValue).Items;
        }

        private IQueryable<Orchid> ApplySorting(IQueryable<Orchid> query, string sortBy, bool ascending)
        {
            switch (sortBy.ToLower())
            {
                case "orchidname":
                    return ascending ? query.OrderBy(o => o.OrchidName) : query.OrderByDescending(o => o.OrchidName);
                case "price":
                    return ascending ? query.OrderBy(o => o.Price) : query.OrderByDescending(o => o.Price);
                case "categoryid":
                    return ascending ? query.OrderBy(o => o.CategoryId) : query.OrderByDescending(o => o.CategoryId);
                case "categoryname":
                    return ascending ? query.OrderBy(o => o.Category.CategoryName) : query.OrderByDescending(o => o.Category.CategoryName);
                case "isnatural":
                    return ascending ? query.OrderBy(o => o.IsNatural) : query.OrderByDescending(o => o.IsNatural);
                default: // Default to OrchidId
                    return ascending ? query.OrderBy(o => o.OrchidId) : query.OrderByDescending(o => o.OrchidId);
            }
        }

        // READ BY ID
        public Orchid GetById(int id)
        {
            return dbContext.Orchids.FirstOrDefault(h => h.OrchidId == id);
        }

        // UPDATE
        public void Update(Orchid orchid)
        {
            var existing = dbContext.Orchids.FirstOrDefault(h => h.OrchidId == orchid.OrchidId);
            if (existing != null)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(orchid);
                dbContext.SaveChanges();
            }
        }

        // DELETE
        public void Delete(int id)
        {
            var orchid = dbContext.Orchids.FirstOrDefault(h => h.OrchidId == id);
            if (orchid != null)
            {
                dbContext.Orchids.Remove(orchid);
                dbContext.SaveChanges();
            }
        }
    }
}
