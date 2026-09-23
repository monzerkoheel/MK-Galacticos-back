using MK.Core.DTOs;
using MK.Core.DTOs.LegendCards;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface ILegendCardService
    {
        Task<OperationResultModel> AddCardAsync(CreateLegendCardDto dto);
        Task<OperationResultModel> UpdateCardAsync(int cardId, UpdateLegendCardDto dto);
        Task<OperationResultModel> DeleteCardAsync(int cardId);
        Task<IReadOnlyList<LegendCardDto>> GetAllCardsAsync();
        Task<IReadOnlyList<LegendCardDto>> GetAllActiveCardsAsync();
        Task<LegendCardDto> GetCardByIdAsync(int cardId);
        Task<PaginationModel<AuditLogDto>> GetCardAuditLogsAsync(int cardId, PaginationRequest dto);
    }
}
