using MK.Core.DTOs;
using MK.Core.DTOs.FanRequests;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IFanRequestService
    {
        // ---- Customer ----
        Task<OperationResultModel> CreateFanRequestAsync(CreateFanRequestDto dto);
        Task<PaginationModel<FanRequestDto>> GetPublicFanRequestsAsync(PublicFanRequestsSpecPrams specPrams);
        Task<PaginationModel<MyFanRequestDto>> GetMyFanRequestsAsync(PaginationRequest dto);
        Task<OperationResultModel> LikeFanRequestAsync(int fanRequestId);
        Task<OperationResultModel> UnlikeFanRequestAsync(int fanRequestId);

        // ---- Management ----
        Task<PaginationModel<ManagementFanRequestDto>> GetAllFanRequestsAsync(FanRequestsSpecPrams specPrams);
        Task<ManagementFanRequestDetailsDto> GetFanRequestByIdAsync(int fanRequestId);
        Task<PaginationModel<AuditLogDto>> GetFanRequestAuditLogsAsync(int fanRequestId, PaginationRequest dto);
        Task<OperationResultModel> ChangeFanRequestStatusAsync(int fanRequestId, ChangeFanRequestStatusDto dto);
        Task<OperationResultModel> DeleteFanRequestAsync(int fanRequestId);
    }
}
