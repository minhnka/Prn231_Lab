using BusinessObjects;
using DataAccess;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IOrderDetailRepo _orderDetailRepo;

        public OrderService(IOrderRepo orderRepo, IOrderDetailRepo orderDetailRepo)
        {
            _orderRepo = orderRepo;
            _orderDetailRepo = orderDetailRepo;
        }

        public void Add(Order order)
        {
            // Check if there are order details to be added
            var orderDetails = order.OrderDetails?.ToList();
            order.OrderDetails = null; // Temporarily clear to avoid EF issues

            using (var scope = new TransactionScope())
            {
                try
                {
                    // Create order first to get ID
                    _orderRepo.Add(order);

                    // Now add order details with the order ID if any exist
                    if (orderDetails != null && orderDetails.Any())
                    {
                        // Calculate total if not set
                        if (!order.TotalAmount.HasValue || order.TotalAmount == 0)
                        {
                            order.TotalAmount = orderDetails.Sum(od =>
                                (od.Price ?? 0) * (od.Quantity ?? 0));

                            // Update order with calculated total
                            _orderRepo.Update(order);
                        }

                        foreach (var detail in orderDetails)
                        {
                            detail.OrderId = order.Id;
                            _orderDetailRepo.Add(detail);
                        }
                    }

                    // Commit transaction
                    scope.Complete();
                }
                catch
                {
                    // Transaction will automatically roll back if we don't call Complete()
                    throw; // Re-throw to let caller handle it
                }
            }
        }

        public List<Order> GetAll()
        {
            return _orderRepo.GetAll();
        }

        public Order GetById(int id)
        {
            return _orderRepo.GetById(id);
        }

        public void Update(Order order)
        {
            _orderRepo.Update(order);
        }

        public void Delete(int id)
        {
            _orderRepo.Delete(id);
        }
    }
}
