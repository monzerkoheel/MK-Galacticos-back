using MK.Core.DTOs;
using MK.Core.DTOs.PaymentMethods;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IPaymentMethodsService
    {
        Task<OperationResultModel> AddPaymentMethodAsync(CreatePaymentMethodDto dto);
        Task<OperationResultModel> UpdatePaymentMethodAsync(int methodId, UpdatePaymentMethodDto dto);
        Task<OperationResultModel> DeletePaymentMethodAsync(int methodId);
        Task<IReadOnlyList<PaymentMethodDto>> GetAllPaymentMethodsAsync();
        Task<IReadOnlyList<PaymentMethodDto>> GetAllAvailablePaymentMethodsAsync();
        Task<PaymentMethodDetailsDto> GetPaymentMethodByIdAsync(int methodId);
        Task<PaginationModel<AuditLogDto>> GetPaymentMethodAuditLogsAsync(int methodId, PaginationRequest dto);
    }
}
