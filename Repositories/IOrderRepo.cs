using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IOrderRepo
    {
        public void Add(Order order);
        public List<Order> GetAll();

        public Order GetById(int id);
        public void Update(Order order);
        public void Delete(int id);
    }
}
