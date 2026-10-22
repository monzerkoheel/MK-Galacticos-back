using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.FanRequests;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FanRequestsController : ControllerBase
    {
        private const string ManagementRoles = "Owner,Manager,Admin";
        private const string AllStaffRoles = "Owner,Manager,Admin,Editor,Customer Support";

        private readonly IFanRequestService _fanRequestService;

        public FanRequestsController(IFanRequestService fanRequestService)
        {
            _fanRequestService = fanRequestService;
        }

        // ---------------- Customer ----------------

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OperationResultModel>> CreateFanRequest([FromForm] CreateFanRequestDto dto)
            => Ok(await _fanRequestService.CreateFanRequestAsync(dto));

        [AllowAnonymous]
        [HttpGet("public-requests")]
        public async Task<ActionResult<PaginationModel<FanRequestDto>>> GetPublicFanRequests([FromQuery] PublicFanRequestsSpecPrams specPrams)
            => Ok(await _fanRequestService.GetPublicFanRequestsAsync(specPrams));

        [Authorize]
        [HttpGet("my-requests")]
        public async Task<ActionResult<PaginationModel<MyFanRequestDto>>> GetMyFanRequests([FromQuery] PaginationRequest dto)
            => Ok(await _fanRequestService.GetMyFanRequestsAsync(dto));

        [Authorize]
        [HttpPost("{fanRequestId:int}/like")]
        public async Task<ActionResult<OperationResultModel>> LikeFanRequest(int fanRequestId)
            => Ok(await _fanRequestService.LikeFanRequestAsync(fanRequestId));

        [Authorize]
        [HttpDelete("{fanRequestId:int}/like")]
        public async Task<ActionResult<OperationResultModel>> UnlikeFanRequest(int fanRequestId)
            => Ok(await _fanRequestService.UnlikeFanRequestAsync(fanRequestId));

        // ---------------- Management ----------------

        [Authorize(Roles = AllStaffRoles)]
        [HttpGet("all-requests")]
        public async Task<ActionResult<PaginationModel<ManagementFanRequestDto>>> GetAllFanRequests([FromQuery] FanRequestsSpecPrams specPrams)
            => Ok(await _fanRequestService.GetAllFanRequestsAsync(specPrams));

        [Authorize(Roles = ManagementRoles)]
        [HttpGet("{fanRequestId:int}")]
        public async Task<ActionResult<ManagementFanRequestDetailsDto>> GetFanRequestById(int fanRequestId)
            => Ok(await _fanRequestService.GetFanRequestByIdAsync(fanRequestId));

        [Authorize(Roles = ManagementRoles)]
        [HttpGet("{fanRequestId:int}/audit-logs")]
        public async Task<ActionResult<PaginationModel<AuditLogDto>>> GetFanRequestAuditLogs(int fanRequestId, [FromQuery] PaginationRequest dto)
            => Ok(await _fanRequestService.GetFanRequestAuditLogsAsync(fanRequestId, dto));

        [Authorize(Roles = ManagementRoles)]
        [HttpPatch("{fanRequestId:int}/status")]
        public async Task<ActionResult<OperationResultModel>> ChangeFanRequestStatus(int fanRequestId, [FromBody] ChangeFanRequestStatusDto dto)
            => Ok(await _fanRequestService.ChangeFanRequestStatusAsync(fanRequestId, dto));

        [Authorize(Roles = ManagementRoles)]
        [HttpDelete("{fanRequestId:int}")]
        public async Task<ActionResult<OperationResultModel>> DeleteFanRequest(int fanRequestId)
            => Ok(await _fanRequestService.DeleteFanRequestAsync(fanRequestId));
    }
}
