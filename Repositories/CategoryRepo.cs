using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class CategoryRepo : ICategoryRepo
    {
        public void Add(Category category)
        {
            CategoryDAO.Instance.Add(category);
        }
        public List<Category> GetAll()
        {
            return CategoryDAO.Instance.GetAll();
        }
        public Category GetById(int id)
        {
            return CategoryDAO.Instance.GetById(id);
        }
        public void Update(Category category)
        {
            CategoryDAO.Instance.Update(category);
        }
        public void Delete(int id)
        {
            CategoryDAO.Instance.Delete(id);
        }
    }
}


