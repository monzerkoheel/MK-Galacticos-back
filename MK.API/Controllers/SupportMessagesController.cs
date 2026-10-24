using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.SupportMessages;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportMessagesController : ControllerBase
    {
        private const string SupportRoles = "Owner,Manager,Admin,Customer Support";

        private readonly ISupportMessageService _supportMessageService;

        public SupportMessagesController(ISupportMessageService supportMessageService)
        {
            _supportMessageService = supportMessageService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<OperationResultModel>> CreateSupportMessage([FromBody] CreateSupportMessageDto dto)
            => Ok(await _supportMessageService.CreateSupportMessageAsync(dto));

        [Authorize(Roles = SupportRoles)]
        [HttpGet]
        public async Task<ActionResult<PaginationModel<SupportMessageDto>>> GetSupportMessages([FromQuery] SupportMessagesSpecPrams specPrams)
            => Ok(await _supportMessageService.GetSupportMessagesAsync(specPrams));

        [Authorize(Roles = SupportRoles)]
        [HttpGet("{messageId:int}")]
        public async Task<ActionResult<SupportMessageDto>> GetSupportMessageById(int messageId)
            => Ok(await _supportMessageService.GetSupportMessageByIdAsync(messageId));

        [Authorize(Roles = SupportRoles)]
        [HttpPatch("{messageId:int}/handled")]
        public async Task<ActionResult<OperationResultModel>> ChangeHandledState(int messageId, [FromBody] ChangeSupportMessageHandledStateDto dto)
            => Ok(await _supportMessageService.ChangeHandledStateAsync(messageId, dto));

        [Authorize(Roles = SupportRoles)]
        [HttpPost("{messageId:int}/reply")]
        public async Task<ActionResult<OperationResultModel>> ReplyToSupportMessage(int messageId, [FromBody] ReplyToSupportMessageDto dto)
            => Ok(await _supportMessageService.ReplyToSupportMessageAsync(messageId, dto));
    }
}
