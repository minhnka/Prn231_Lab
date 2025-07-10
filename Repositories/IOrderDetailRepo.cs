using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IOrderDetailRepo
    {
        public void Add(OrderDetail orderDetail);
        public List<OrderDetail> GetAll();

        public OrderDetail GetById(int id);
        public void Update(OrderDetail orderDetail);
        public void Delete(int id);
    }
}
