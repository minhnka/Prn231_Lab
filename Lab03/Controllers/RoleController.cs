using Microsoft.AspNetCore.Mvc;
using Service;

namespace Lab03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/Role
        [HttpGet]
        public IActionResult GetAll()
        {
            var roles = _roleService.GetAll();
            return Ok(roles);
        }

        // GET: api/Role/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var role = _roleService.GetById(id);

            if (role == null)
                return NotFound();

            return Ok(role);
        }

        // DELETE: api/Role/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _roleService.Delete(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
