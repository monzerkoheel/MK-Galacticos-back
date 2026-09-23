using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.LegendCards;
using MK.Core.HelperModels;
using MK.Service.Services;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LegendCardController : ControllerBase
    {
        private readonly ILegendCardService _legendCardService;
        public LegendCardController(ILegendCardService legendCardService)
        {
            _legendCardService = legendCardService;
        }

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPost]
        public async Task<ActionResult<OperationResultModel>> AddLegendCard([FromForm] CreateLegendCardDto dto)
            => Ok(await _legendCardService.AddCardAsync(dto));

        [Authorize(Roles = "Owner,Manager,Admin,Editor")]
        [HttpPatch("{cardId:int}")]
        public async Task<ActionResult<OperationResultModel>> UpdateLegendCard(int cardId, [FromForm] UpdateLegendCardDto dto)
            => Ok(await _legendCardService.UpdateCardAsync(cardId, dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpDelete("{cardId:int}")]
        public async Task<ActionResult<OperationResultModel>> DeleteLegendCard(int cardId)
            => Ok(await _legendCardService.DeleteCardAsync(cardId));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("all")]
        public async Task<ActionResult<IReadOnlyList<LegendCardDto>>> GetAllLegendCards()
            => Ok(await _legendCardService.GetAllCardsAsync());

        [HttpGet("all-active")]
        public async Task<ActionResult<IReadOnlyList<LegendCardDto>>> GetAllActiveLegendCards()
            => Ok(await _legendCardService.GetAllActiveCardsAsync());

        [HttpGet("{cardId:int}")]
        public async Task<ActionResult<LegendCardDto>> GetCardById(int cardId)
            => Ok(await _legendCardService.GetCardByIdAsync(cardId));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpGet("{cardId:int}/audit-logs")]
        public async Task<ActionResult<PaginationModel<AuditLogDto>>> GetCardAuditLogs(int cardId, [FromQuery] PaginationRequest dto)
            => Ok(await _legendCardService.GetCardAuditLogsAsync(cardId, dto));
    }
}
