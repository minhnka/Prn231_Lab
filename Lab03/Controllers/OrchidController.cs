using AutoMapper;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Lab03.Controllers.OrchidController;

namespace Lab03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrchidController : Controller
    {
        private readonly IOrchidService _orchidService; 
        private readonly IMapper _mapper;

        public OrchidController(IOrchidService orchidService, IMapper mapper)
        {
            _orchidService = orchidService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult GetAll(
              [FromQuery] int page = 1,
              [FromQuery] int pageSize = 10,
              [FromQuery] string searchTerm = "",
              [FromQuery] string sortBy = "OrchidId",
              [FromQuery] bool ascending = true,
              [FromQuery] int? categoryId = null,
              [FromQuery] decimal? minPrice = null,
              [FromQuery] decimal? maxPrice = null,
              [FromQuery] bool? isNatural = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var pagedResult = _orchidService.GetAllPaged(
                page, pageSize, searchTerm, sortBy, ascending,
                categoryId, minPrice, maxPrice, isNatural);

            var orchidDtos = _mapper.Map<List<OrchidDto>>(pagedResult.Items);

            var result = new PagedResponseDto<OrchidDto>
            {
                Items = orchidDtos,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                PageCount = pagedResult.PageCount
            };

            return Ok(result);
        }

        // GET: api/Orchid/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult GetById(int id)
        {
            var orchid = _orchidService.GetById(id);

            if (orchid == null)
                return NotFound();

            var orchidDto = _mapper.Map<OrchidDto>(orchid);
            return Ok(orchidDto);
        }

        // POST: api/Orchid
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create([FromBody] CreateOrchidDto createOrchidDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var orchid = _mapper.Map<Orchid>(createOrchidDto);
            _orchidService.Add(orchid);
            
            var resultDto = _mapper.Map<OrchidDto>(orchid);
            return CreatedAtAction(nameof(GetById), new { id = orchid.OrchidId }, resultDto);
        }


        [HttpPut]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Update( [FromBody] UpdateOrchidDto updateOrchidDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var orchid = _mapper.Map<Orchid>(updateOrchidDto);
                _orchidService.Update(orchid);
                return Ok(updateOrchidDto);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // DELETE: api/Orchid/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Delete(int id)
        {
            try
            {
                _orchidService.Delete(id);
                return Ok(new { message = "Orchid deleted successfully" });
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public class PagedResponseDto<T>
        {
            public List<T> Items { get; set; } = new List<T>();
            public int TotalCount { get; set; }
            public int PageCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
        public class OrchidDto
        {
            public int OrchidId { get; set; }
            public string? OrchidName { get; set; }
            public string? OrchidDescription { get; set; }
            public decimal? Price { get; set; }
            public bool? IsNatural { get; set; }
            public string? OrchidUrl { get; set; }
            public int? CategoryId { get; set; }
            public string? CategoryName { get; set; }
        }

        public class CreateOrchidDto
        {
            [Required]
            [StringLength(100)]
            public string? OrchidName { get; set; }

            public string? OrchidDescription { get; set; }

            [Range(0, 10000)]
            public decimal? Price { get; set; }

            public bool? IsNatural { get; set; }

            public string? OrchidUrl { get; set; }

            public int? CategoryId { get; set; }
        }

        public class UpdateOrchidDto
        {
            [Required]
            public int OrchidId { get; set; }

            [StringLength(100)]
            public string? OrchidName { get; set; }

            public string? OrchidDescription { get; set; }

            [Range(0, 10000)]
            public decimal? Price { get; set; }

            public bool? IsNatural { get; set; }

            public string? OrchidUrl { get; set; }

            public int? CategoryId { get; set; }
        }
    }
}
