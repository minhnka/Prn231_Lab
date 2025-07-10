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
    public class RoleService : IRoleService
    {
        private readonly IRoleRepo _roleRepo;

        public RoleService(IRoleRepo roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public List<Role> GetAll()
        {
            return _roleRepo.GetAll();
        }

        public Role GetById(int id)
        {
            return _roleRepo.GetById(id);
        }

        public void Delete(int id)
        {
            _roleRepo.Delete(id);
        }
    }
}
