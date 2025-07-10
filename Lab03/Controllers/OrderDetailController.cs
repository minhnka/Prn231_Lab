using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Lab03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderDetailController : Controller
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        // GET: api/OrderDetail
        [HttpGet]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult GetAll()
        {
            var orderDetails = _orderDetailService.GetAll();
            return Ok(orderDetails);
        }

        // GET: api/OrderDetail/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult GetById(int id)
        {
            var orderDetail = _orderDetailService.GetById(id);

            if (orderDetail == null)
                return NotFound();

            return Ok(orderDetail);
        }

        // POST: api/OrderDetail
        [HttpPost]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult Create([FromBody] OrderDetail orderDetail)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _orderDetailService.Add(orderDetail);
            return CreatedAtAction(nameof(GetById), new { id = orderDetail.Id }, orderDetail);
        }

        // PUT: api/OrderDetail/{id}
        [HttpPut]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult Update( [FromBody] OrderDetail orderDetail)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _orderDetailService.Update(orderDetail);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // DELETE: api/OrderDetail/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult Delete(int id)
        {
            try
            {
                _orderDetailService.Delete(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
