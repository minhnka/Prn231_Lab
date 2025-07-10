using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Lab03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
      
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAll()
        {
            var categories = _categoryService.GetAll();
            return Ok(categories);
        }

        // GET: api/Category/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetById(int id)
        {
            var category = _categoryService.GetById(id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST: api/Category
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _categoryService.Add(category);
            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
        }

        // PUT: api/Category/{id}
        [HttpPut]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Update( [FromBody] Category category)
        {
            

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _categoryService.Update(category);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Delete(int id)
        {
            try
            {
                _categoryService.Delete(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
