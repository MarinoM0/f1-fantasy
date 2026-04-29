using F1Fantasy.Api.DTOs.Dashboard;
using F1Fantasy.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DashboardDto>> GetDashboard(
            CancellationToken cancellationToken)
        {
            var dashboard = await _dashboardService.GetDashboardAsync(cancellationToken);
            return Ok(dashboard);
        }
    }
}
