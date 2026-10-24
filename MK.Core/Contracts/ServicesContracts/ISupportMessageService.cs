using MK.Core.DTOs.SupportMessages;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface ISupportMessageService
    {
        Task<OperationResultModel> CreateSupportMessageAsync(CreateSupportMessageDto dto);
        Task<PaginationModel<SupportMessageDto>> GetSupportMessagesAsync(SupportMessagesSpecPrams specPrams);
        Task<SupportMessageDto> GetSupportMessageByIdAsync(int messageId);
        Task<OperationResultModel> ChangeHandledStateAsync(int messageId, ChangeSupportMessageHandledStateDto dto);
        Task<OperationResultModel> ReplyToSupportMessageAsync(int messageId, ReplyToSupportMessageDto dto);
    }
}
