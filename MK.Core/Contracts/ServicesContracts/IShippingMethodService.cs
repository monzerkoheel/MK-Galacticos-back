using MK.Core.DTOs;
using MK.Core.DTOs.ShippingMethods;
using MK.Core.HelperModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IShippingMethodService
    {
        Task<OperationResultModel> AddShippingMethodAsync(CreateShippingMethodDto dto);
        Task<OperationResultModel> UpdateShippingMethodAsync(int shippingMethodId, UpdateShippingMethodDto dto);
        Task<OperationResultModel> DeleteShippingMethodAsync(int shippingMethodId);
        Task<IReadOnlyList<ShippingMethodDto>> GetAllShippingMethodsAsync();
        Task<IReadOnlyList<ShippingMethodDto>> GetAllActiveShippingMethodsAsync();
        Task<ShippingMethodDetailsDto> GetShippingMethodByIdAsync(int shippingMethodId);
        Task<PaginationModel<AuditLogDto>> GetShippingMethodAuditLogsAsync(int shippingMethodId, PaginationRequest dto);
    }
}
