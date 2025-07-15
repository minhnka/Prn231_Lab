using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class AccountDAO
    {
        private static AccountDAO instance;
        private static ProductManagementDbContext dbContext;

        public AccountDAO()
        {
            dbContext = new ProductManagementDbContext();
        }
        public static AccountDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccountDAO();
                }
                return instance;
            }
        }


        public Account GetByEmailandPassword(string email, string password)
        {


            return dbContext.Accounts.SingleOrDefault(m => m.Email.Equals(email) && m.Password.Equals(password));

        }


        public void Add(Account account)
        {
            // Optional: check if email already exists
            if (dbContext.Accounts.Any(a => a.Email == account.Email))
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
        }


        // READ ALL
        public List<Account> GetAll()
        {
            return dbContext.Accounts.ToList();
        }

        // READ BY ID
        public Account GetById(int id)
        {
            return dbContext.Accounts.FirstOrDefault(h => h.AccountId == id);
        }
    }
}
