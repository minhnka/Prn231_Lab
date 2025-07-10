using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RoleRepo : IRoleRepo
    {
        public List<Role> GetAll()
        {
            return RoleDAO.Instance.GetAll();
        }
        public Role GetById(int id)
        {
            return RoleDAO.Instance.GetById(id);
        }

        public void Delete(int id)
        {
            RoleDAO.Instance.Delete(id);
        }
    }
}
