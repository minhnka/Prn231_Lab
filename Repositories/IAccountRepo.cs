using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IAccountRepo
    {
        public Account GetByEmailandPassword(string email, string password);

        public List<Account> GetAll();

        public Account GetById(int id);
        public void Add(Account account);
    }
}
