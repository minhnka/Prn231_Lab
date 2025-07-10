using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class OrderDetailDAO
    {
        private static OrderDetailDAO instance;
        private static ProductManagementDbContext dbContext;

        public OrderDetailDAO()
        {
            dbContext = new ProductManagementDbContext();
        }
        public static OrderDetailDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OrderDetailDAO();
                }
                return instance;
            }
        }

        public void Add(OrderDetail orderDetail)
        {
            dbContext.OrderDetails.Add(orderDetail);
            dbContext.SaveChanges();
        }

        // READ ALL
        public List<OrderDetail> GetAll()
        {
            return dbContext.OrderDetails.ToList();
        }

        // READ BY ID
        public OrderDetail GetById(int id)
        {
            return dbContext.OrderDetails.FirstOrDefault(h => h.Id == id);
        }

        // UPDATE
        public void Update(OrderDetail orderDetail)
        {
            var existing = dbContext.OrderDetails.FirstOrDefault(h => h.Id == orderDetail.Id);
            if (existing != null)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(orderDetail);
                dbContext.SaveChanges();
            }
        }

        // DELETE
        public void Delete(int id)
        {
            var orderDetail = dbContext.OrderDetails.FirstOrDefault(h => h.Id == id);
            if (orderDetail != null)
            {
                dbContext.OrderDetails.Remove(orderDetail);
                dbContext.SaveChanges();
            }
        }
    }
}
