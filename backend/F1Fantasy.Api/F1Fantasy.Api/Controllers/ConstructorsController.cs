using F1Fantasy.Api.DTOs;
using F1Fantasy.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConstructorsController : ControllerBase
    {
        private readonly ConstructorService _constructorService;

        public ConstructorsController(ConstructorService constructorService)
        {
            _constructorService = constructorService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ConstructorDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ConstructorDto>>> GetAll()
        {
            var constructors = await _constructorService.GetAllAsync();
            return Ok(constructors);
        }
    }
}
