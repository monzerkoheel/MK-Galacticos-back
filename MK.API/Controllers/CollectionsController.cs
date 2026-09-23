using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Collections;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        public CollectionsController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPost]
        public async Task<ActionResult<OperationResultModel>> CreateCollection([FromForm] CreateCollectionDto dto)
            => Ok(await _collectionService.CreateCollectionAsync(dto));

        [Authorize(Roles = "Owner,Manager,Admin,Editor")]
        [HttpPut("{collectionId:int}")]
        public async Task<ActionResult<OperationResultModel>> EditCollection(int collectionId, [FromForm] EditCollectionDto dto)
            => Ok(await _collectionService.EditCollectionAsync(collectionId,dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpDelete("{collectionId:int}")]
        public async Task<ActionResult<OperationResultModel>> DeleteCollection(int collectionId)
            => Ok(await _collectionService.DeleteCollectionAsync(collectionId));

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ActiveCollectionDto>?>> GetActiveCollections()
            => Ok(await _collectionService.GetAllActiveCollectionsAsync());

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("all")]
        public async Task<ActionResult<IReadOnlyList<CollectionDto>?>> GetAllCollections()
            => Ok(await _collectionService.GetAllCollectionsAsync());

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpGet("simple")]
        public async Task<ActionResult<IReadOnlyList<SimpleCollectionDto>?>> GetSimpleCollections()
            => Ok(await _collectionService.GetSimpleCollectionsAsync());

        [Authorize(Roles = "Owner,Manager,Admin,Editor")]
        [HttpGet("{collectionId:int}")]
        public async Task<ActionResult<CollectionDetailsDto>> GetCollectionDetails(int collectionId)
            => Ok(await _collectionService.GetCollectionDetailsByIdAsync(collectionId));

        [HttpGet("{collectionId:int}/products")]
        public async Task<ActionResult<PaginationModel<ActiveProductsDto>>> GetCollectionProducts(int collectionId, [FromQuery] PaginationRequest dto)
            => Ok(await _collectionService.GetCollectionProductsAsync(collectionId, dto));

        [HttpGet("women")]
        public async Task<ActionResult<PaginationModel<ActiveProductsDto>>> GetWomenCollections([FromQuery] PaginationRequest dto)
            => Ok(await _collectionService.GetWomenCollectionAsync(dto));

    }
}
