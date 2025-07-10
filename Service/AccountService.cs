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
    public class AccountService : IAccountService
    {
		private readonly IAccountRepo _accountRepo;

		public AccountService(IAccountRepo accountRepo)
		{
			_accountRepo = accountRepo;
		}

		public Account GetByEmailandPassword(string email, string password)
		{
			return _accountRepo.GetByEmailandPassword(email, password);
		}

		public void Add(Account account)
		{
			_accountRepo.Add(account);
		}

        public List<Account> GetAll()
        {
            return _accountRepo.GetAll();
        }

        public Account GetById(int id)
        {
            return _accountRepo.GetById(id);
        }
    }
}
