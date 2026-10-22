using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.Orders;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CreateOrderDto dto)
        {
            var result = await _orderService.OrderCheckOutAsync(dto);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{orderId:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
            => Ok(await _orderService.CancelOrderAsync(orderId));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPut("{orderId:int}/change-state")]
        public async Task<IActionResult> ChangeOrderState(int orderId, ChangeOrderStateDto dto)
            => Ok(await _orderService.ChangeOrderStateAsync(orderId, dto));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("all-orders")]
        public async Task<ActionResult<PaginationModel<OrderDto>>> GetAllOrders([FromQuery] OrdersSpecPrams specParams)
            => Ok(await _orderService.GetAllOrdersAsync(specParams));

        [Authorize(Roles = "Owner,Manager,Admin,Customer Support")]
        [HttpGet("{orderId:int}/details")]
        public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(int orderId)
            => Ok(await _orderService.GetOrderDetailsAsync(orderId));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpGet("{orderId:int}/audit-logs")]
        public async Task<ActionResult<PaginationModel<AuditLogDto>>> GetOrderAuditLogs(int orderId, [FromQuery] PaginationRequest dto)
            => Ok(await _orderService.GetOrderAuditLogsAsync(orderId, dto));

        [Authorize]
        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDto>> GetOrderById(int orderId)
            => Ok(await _orderService.GetOrderByIdAsync(orderId));

        [Authorize]
        [HttpGet("{orderId:int}/invoice")]
        public async Task<IActionResult> GetInvoice(int orderId)
        {
            var result = await _orderService.SendOrderInvoiceToUserAsync(orderId);
            return Ok(result);
        }
    }
}
