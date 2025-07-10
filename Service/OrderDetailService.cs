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
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepo _orderDetailRepo;

        public OrderDetailService(IOrderDetailRepo orderDetailRepo)
        {
            _orderDetailRepo = orderDetailRepo;
        }

        public void Add(OrderDetail orderDetail)
        {
            _orderDetailRepo.Add(orderDetail);
        }

        public List<OrderDetail> GetAll()
        {
            return _orderDetailRepo.GetAll();
        }

        public OrderDetail GetById(int id)
        {
            return _orderDetailRepo.GetById(id);
        }

        public void Update(OrderDetail orderDetail)
        {
            _orderDetailRepo.Update(orderDetail);
        }

        public void Delete(int id)
        {
            _orderDetailRepo.Delete(id);
        }
    }
}
