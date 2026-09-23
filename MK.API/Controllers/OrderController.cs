using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Orders;
using MK.Core.HelperModels.Paymob.RequestsAndResults;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymobService _paymobService;
        public OrderController(IOrderService orderService, IPaymobService paymobService)
        {
            _orderService = orderService;
            _paymobService = paymobService;
        }

        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CreateOrderDto dto)
        {
            var result = await _orderService.OrderCheckOutAsync(dto);
            return Ok(result);
        }

        [HttpPost("paymob-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();
            var hmac = Request.Query["hmac"].ToString();

            await _paymobService.HandlePaymobWebhookAsync(rawBody, hmac);

            return Ok();
        }



        [Authorize]
        [HttpGet("{orderId:int}/invoice")]
        public async Task<IActionResult> GetInvoice(int orderId)
        {
            var result = await _orderService.SendOrderInvoiceToUserAsync(orderId);
            return Ok(result);
        }
    }
}
