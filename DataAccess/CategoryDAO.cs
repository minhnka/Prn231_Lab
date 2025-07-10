using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class CategoryDAO
    {
        private static CategoryDAO instance;
        private static ProductManagementDbContext dbContext;

        public CategoryDAO()
        {
            dbContext = new ProductManagementDbContext();
        }
        public static CategoryDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CategoryDAO();
                }
                return instance;
            }
        }


        public void Add(Category category)
        {
            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
        }

        // READ ALL
        public List<Category> GetAll()
        {
            return dbContext.Categories.ToList();
        }

        // READ BY ID
        public Category GetById(int id)
        {
            return dbContext.Categories.FirstOrDefault(h => h.CategoryId == id);
        }

        // UPDATE
        public void Update(Category category)
        {
            var existing = dbContext.Categories.FirstOrDefault(h => h.CategoryId == category.CategoryId);
            if (existing != null)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(category);
                dbContext.SaveChanges();
            }
        }

        // DELETE
        public void Delete(int id)
        {
            var category = dbContext.Categories.FirstOrDefault(h => h.CategoryId == id);
            if (category != null)
            {
                dbContext.Categories.Remove(category);
                dbContext.SaveChanges();
            }
        }
    }
}
