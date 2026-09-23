using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.ShippingMethods;
using MK.Core.HelperModels;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingMethodController : ControllerBase
    {
        private readonly IShippingMethodService _shippingMethodService;
        public ShippingMethodController(IShippingMethodService shippingMethodService)
        {
            _shippingMethodService = shippingMethodService;
        }

        [Authorize(Roles = "Owner,Manager")]
        [HttpPost]
        public async Task<ActionResult<OperationResultModel>> AddShippingMethod(CreateShippingMethodDto dto)
            => Ok(await _shippingMethodService.AddShippingMethodAsync(dto));

        [Authorize(Roles = "Owner,Manager")]
        [HttpPatch("{shippingMethodId:int}")]
        public async Task<ActionResult<OperationResultModel>> UpdateShippingMethod(int shippingMethodId, UpdateShippingMethodDto dto)
            => Ok(await _shippingMethodService.UpdateShippingMethodAsync(shippingMethodId, dto));

        [Authorize(Roles = "Owner,Manager")]
        [HttpDelete("{shippingMethodId:int}")]
        public async Task<ActionResult<OperationResultModel>> DeleteShippingMethod(int shippingMethodId)
            => Ok(await _shippingMethodService.DeleteShippingMethodAsync(shippingMethodId));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("all")]
        public async Task<ActionResult<IReadOnlyList<ShippingMethodDto>>> GetAllShippingMethods()
            => Ok(await _shippingMethodService.GetAllShippingMethodsAsync());

        [HttpGet("all-active")]
        public async Task<ActionResult<IReadOnlyList<ShippingMethodDto>>> GetAllActiveShippingMethods()
            => Ok(await _shippingMethodService.GetAllActiveShippingMethodsAsync());

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpGet("{shippingMethodId:int}")]
        public async Task<ActionResult<ShippingMethodDetailsDto>> GetShippingMethodById(int shippingMethodId)
            => Ok(await _shippingMethodService.GetShippingMethodByIdAsync(shippingMethodId));

        [Authorize(Roles = "Owner,Manager")]
        [HttpGet("{shippingMethodId:int}/audit-logs")]
        public async Task<ActionResult<PaginationModel<AuditLogDto>>> GetShippingMethodAuditLogs(int shippingMethodId, [FromQuery] PaginationRequest dto)
            => Ok(await _shippingMethodService.GetShippingMethodAuditLogsAsync(shippingMethodId, dto));
    }
}
