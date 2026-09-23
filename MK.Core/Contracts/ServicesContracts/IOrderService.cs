using MK.Core.DTOs.Orders;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IOrderService
    {
        Task<CheckoutResponseDto> OrderCheckOutAsync(CreateOrderDto dto);

        Task<OperationResultModel> SendOrderInvoiceToUserAsync(int orderId);
    }
}
