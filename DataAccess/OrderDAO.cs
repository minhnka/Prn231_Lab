using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class OrderDAO
    {
        private static OrderDAO instance;
        private static ProductManagementDbContext dbContext;

        public OrderDAO()
        {
            dbContext = new ProductManagementDbContext();
        }
        public static OrderDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OrderDAO();
                }
                return instance;
            }
        }

        public void Add(Order order)
        {
            dbContext.Orders.Add(order);
            dbContext.SaveChanges();
        }

        // READ ALL
        public List<Order> GetAll()
        {
            return dbContext.Orders.Include(o => o.OrderDetails).Include(o => o.Account).ToList();
        }

        // READ BY ID
        public Order GetById(int id)
        {
            return dbContext.Orders.FirstOrDefault(h => h.Id == id);
        }

        // UPDATE
        public void Update(Order order)
        {
            var existing = dbContext.Orders.FirstOrDefault(h => h.Id == order.Id);
            if (existing != null)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(order);
                dbContext.SaveChanges();
            }
        }

        // DELETE
        public void Delete(int id)
        {
            var order = dbContext.Orders.FirstOrDefault(h => h.Id == id);
            if (order != null)
            {
                dbContext.Orders.Remove(order);
                dbContext.SaveChanges();
            }
        }
    }
}
