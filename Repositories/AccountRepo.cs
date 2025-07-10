using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class AccountRepo : IAccountRepo
    {
        public Account GetByEmailandPassword(string email, string password) => AccountDAO.Instance.GetByEmailandPassword(email, password);


        public void Add(Account account)
        {
            AccountDAO.Instance.Add(account);
        }

        public List<Account> GetAll()
        {
            return AccountDAO.Instance.GetAll();
        }
        public Account GetById(int id)
        {
            return AccountDAO.Instance.GetById(id);
        }
    }
}
