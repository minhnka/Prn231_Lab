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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;

        public CategoryService(ICategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public void Add(Category category)
        {
            _categoryRepo.Add(category);
        }

        public List<Category> GetAll()
        {
            return _categoryRepo.GetAll();
        }

        public Category GetById(int id)
        {
            return _categoryRepo.GetById(id);
        }

        public void Update(Category category)
        {
            _categoryRepo.Update(category);
        }

        public void Delete(int id)
        {
            _categoryRepo.Delete(id);
        }
    }
}


