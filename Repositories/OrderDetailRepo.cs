using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class OrderDetailRepo : IOrderDetailRepo
    {
        public void Add(OrderDetail orderDetail)
        {
            OrderDetailDAO.Instance.Add(orderDetail);
        }
        public List<OrderDetail> GetAll()
        {
            return OrderDetailDAO.Instance.GetAll();
        }
        public OrderDetail GetById(int id)
        {
            return OrderDetailDAO.Instance.GetById(id);
        }
        public void Update(OrderDetail orderDetail)
        {
            OrderDetailDAO.Instance.Update(orderDetail);
        }
        public void Delete(int id)
        {
            OrderDetailDAO.Instance.Delete(id);
        }
    }
}
