using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface ICategoryRepo
    {
        public void Add(Category category);
        public List<Category> GetAll();

        public Category GetById(int id);
        public void Update(Category category);
        public void Delete(int id);
    }
}
