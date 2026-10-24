using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Dashboard;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner,Manager,Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetDashboard([FromQuery] DashboardRangePrams prams)
            => Ok(await _dashboardService.GetDashboardAsync(prams));

        [HttpGet("sales-overview")]
        public async Task<ActionResult<SalesOverviewDto>> GetSalesOverview([FromQuery] SalesOverviewRange range = SalesOverviewRange.Last7Days)
            => Ok(await _dashboardService.GetSalesOverviewAsync(range));
    }
}
