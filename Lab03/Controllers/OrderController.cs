using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Order
        [HttpGet]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult GetAll()
        {
            var orders = _orderService.GetAll();
            return Ok(orders);
        }

        // GET: api/Order/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult GetById(int id)
        {
            var order = _orderService.GetById(id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // POST: api/Order
        [HttpPost]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult Create([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var order = new Order
            {
                AccountId = createOrderDto.AccountId,
                OrderDate = createOrderDto.OrderDate,
                OrderStatus = createOrderDto.OrderStatus,
                TotalAmount = createOrderDto.TotalAmount,
                OrderDetails = createOrderDto.OrderDetails?.Select(od => new OrderDetail
                {
                    OrchidId = od.OrchidId,
                    Price = od.Price,
                    Quantity = od.Quantity
                }).ToList() ?? new List<OrderDetail>()
            };

            _orderService.Add(order);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // PUT: api/Order
        [HttpPut]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult Update([FromBody] UpdateOrderDto updateOrderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Map DTO to entity
                var order = new Order
                {
                    Id = updateOrderDto.Id,
                    AccountId = updateOrderDto.AccountId,
                    OrderDate = updateOrderDto.OrderDate,
                    OrderStatus = updateOrderDto.OrderStatus,
                    TotalAmount = updateOrderDto.TotalAmount
                };

                _orderService.Update(order);
                return Ok(order);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // DELETE: api/Order/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult Delete(int id)
        {
            try
            {
                _orderService.Delete(id);
                return Ok(new { message = "Order deleted successfully" });
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public class OrderDetailDto
        {
            public int? OrchidId { get; set; }
            public decimal? Price { get; set; }
            public int? Quantity { get; set; }
        }

        public class CreateOrderDto
        {
            [Required]
            public int? AccountId { get; set; }
            
            public DateOnly? OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
            
            [StringLength(50)]
            public string? OrderStatus { get; set; } = "Pending";
            
            public decimal? TotalAmount { get; set; }
            
            public List<OrderDetailDto>? OrderDetails { get; set; }
        }

        public class UpdateOrderDto
        {
            [Required]
            public int Id { get; set; }
            
            [Required]
            public int? AccountId { get; set; }
            
            public DateOnly? OrderDate { get; set; }
            
            [StringLength(50)]
            public string? OrderStatus { get; set; }
            
            public decimal? TotalAmount { get; set; }
        }
    }
}
