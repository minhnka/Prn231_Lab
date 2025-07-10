using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IRoleService
    {
        public List<Role> GetAll();
        public Role GetById(int id);
        public void Delete(int id);
    }
}
