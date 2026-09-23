using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.PaymentMethods;
using MK.Core.HelperModels;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodsService _paymentMethodsService;
        public PaymentMethodsController(IPaymentMethodsService paymentMethodsService)
        {
            _paymentMethodsService = paymentMethodsService;
        }

        [Authorize(Roles = "Owner,Manager")]
        [HttpPost]
        public async Task<ActionResult<OperationResultModel>> AddPaymentMethod([FromForm] CreatePaymentMethodDto dto)
            => Ok(await _paymentMethodsService.AddPaymentMethodAsync(dto));

        [Authorize(Roles = "Owner,Manager")]
        [HttpPatch("{methodId:int}")]
        public async Task<ActionResult<OperationResultModel>> UpdatePaymentMethod(int methodId, [FromForm] UpdatePaymentMethodDto dto)
            => Ok(await _paymentMethodsService.UpdatePaymentMethodAsync(methodId, dto));

        [Authorize(Roles = "Owner,Manager")]
        [HttpDelete("{methodId:int}")]
        public async Task<ActionResult<OperationResultModel>> DeletePaymentMethod(int methodId)
            => Ok(await _paymentMethodsService.DeletePaymentMethodAsync(methodId));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("all")]
        public async Task<ActionResult<IReadOnlyList<PaymentMethodDto>>> GetAllPaymentMethods()
            => Ok(await _paymentMethodsService.GetAllPaymentMethodsAsync());

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PaymentMethodDto>>> GetAllAvailablePaymentMethods()
            => Ok(await _paymentMethodsService.GetAllAvailablePaymentMethodsAsync());

        [Authorize(Roles = "Owner,Manager,Admin,Editor")]
        [HttpGet("{methodId:int}")]
        public async Task<ActionResult<PaymentMethodDetailsDto>> GetPaymentMethodById(int methodId)
            => Ok(await _paymentMethodsService.GetPaymentMethodByIdAsync(methodId));

        [Authorize(Roles = "Owner,Manager")]
        [HttpGet("{methodId:int}/audit-logs")]
        public async Task<ActionResult<PaginationModel<AuditLogDto>>> GetPaymentMethodAuditLogs(int methodId, [FromQuery] PaginationRequest dto)
            => Ok(await _paymentMethodsService.GetPaymentMethodAuditLogsAsync(methodId, dto));
    }
}
