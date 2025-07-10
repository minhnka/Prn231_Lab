using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class OrderRepo : IOrderRepo
    {
        public void Add(Order order)
        {
            OrderDAO.Instance.Add(order);
        }
        public List<Order> GetAll()
        {
            return OrderDAO.Instance.GetAll();
        }
        public Order GetById(int id)
        {
            return OrderDAO.Instance.GetById(id);
        }
        public void Update(Order order)
        {
            OrderDAO.Instance.Update(order);
        }
        public void Delete(int id)
        {
            OrderDAO.Instance.Delete(id);
        }
    }
}
