using MK.Core.DTOs.Dashboard;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(DashboardRangePrams prams);
        Task<SalesOverviewDto> GetSalesOverviewAsync(SalesOverviewRange range);
    }
}
