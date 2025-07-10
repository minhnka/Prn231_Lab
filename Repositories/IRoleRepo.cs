using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IRoleRepo
    {
        public List<Role> GetAll();
        public Role GetById(int id);
        public void Delete(int id);
    }
}
