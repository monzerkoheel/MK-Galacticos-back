using MK.Core.DTOs;
using MK.Core.DTOs.Orders;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;
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
        Task<OperationResultModel> CancelOrderAsync(int orderId);
        Task<OperationResultModel> ChangeOrderStateAsync(int orderId, ChangeOrderStateDto dto);
        Task<PaginationModel<OrderDto>> GetAllOrdersAsync(OrdersSpecPrams specParams);
        Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId);
        Task<PaginationModel<AuditLogDto>> GetOrderAuditLogsAsync(int orderId, PaginationRequest dto);
        Task<OrderDetailsDto> GetOrderByIdAsync(int orderId);
        Task<OperationResultModel> SendOrderInvoiceToUserAsync(int orderId);
    }
}
