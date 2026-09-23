using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.Localization.Resources;
using MK.Core.Specifications.SpecificationParameters;
using System.Data;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPost]
        public async Task<ActionResult<OperationResultModel>> CreateProduct([FromForm] CreateProductDto dto)
            => Ok(await _productService.CreateProductAsync(dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPatch("{productId:int}")]
        public async Task<ActionResult<OperationResultModel>> EditProduct(int productId, [FromBody] EditProductDto dto)
            => Ok(await _productService.EditProductAsync(productId, dto));

        [Authorize(Roles = "Owner,Manager,Admin,Editor")]
        [HttpPatch("{productId:int}/design")]
        public async Task<ActionResult<OperationResultModel>> EditProductDesign(int productId,[FromForm] EditProductDesignDto dto)
            => Ok(await _productService.EditProductDesignAsync(productId, dto));

        [Authorize(Roles = "Owner,Manager,Admin,Editor")]
        [HttpPatch("{productId:int}/collections")]
        public async Task<ActionResult<OperationResultModel>> EditProductCollections(int productId,[FromBody] EditProductCollectionsDto dto)
            => Ok(await _productService.EditProductCollectionsAsync(productId, dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPatch("{productId:int}/prints")]
        public async Task<ActionResult<OperationResultModel>> EditProductPrints(int productId, [FromBody] EditProductPrintsDto dto)
            => Ok(await _productService.EditProductPrintsAsync(productId, dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPatch("{productId:int}/stock")]
        public async Task<ActionResult<OperationResultModel>> EditProductStock(int productId, [FromBody] EditProductStocksDto dto)
            => Ok(await _productService.EditProductStockAsync(productId, dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpDelete("{productId:int}")]
        public async Task<ActionResult<OperationResultModel>> DeleteProduct(int productId)
            => Ok(await _productService.DeleteProductAsync(productId));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("all-products")]
        public async Task<ActionResult<OperationResultModel>> GetAllProducts([FromQuery] ProductSpecPrams specPrams)
            => Ok(await _productService.GetAllProductsAsync(specPrams));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("products-statistics")]
        public async Task<ActionResult<OperationResultModel>> GetProductsStatistics()
            => Ok(await _productService.GetProductsStatisticsAsync());

        [HttpGet("active-products")]
        public async Task<ActionResult<OperationResultModel>> GetAllActiveProducts([FromQuery] ActiveProductsSpecPrams specPrams)
            => Ok(await _productService.GetAllActiveProductsAsync(specPrams));

        [HttpGet("active-product/{productId:int}")]
        public async Task<ActionResult<ActiveProductDto>> GetActiveProductById(int productId)
            => Ok(await _productService.GetActiveProductByIdAsync(productId));

        [HttpGet("{productId:int}/suggested-products/{productCount:int}")]
        public async Task<ActionResult<ActiveProductDto>> GetSuggestedProducts(int productId, int productCount)
            => Ok(await _productService.GetSuggestedProductsAsync(productId, productCount));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("{productId:int}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int productId)
            => Ok(await _productService.GetProductByIdAsync(productId));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpGet("{productId:int}/orders")]
        public async Task<ActionResult<ProductOrdersDto>> GetProductOrders(int productId, [FromQuery] PaginationRequest dto)
            => Ok(await _productService.GetProductOrdersAsync(productId, dto));

        [Authorize(Roles = "Owner,Manager,Admin,Customer Support")]
        [HttpGet("{productId:int}/ratings")]
        public async Task<ActionResult<ManagmentProductRatingDto>> GetProductRatings(int productId, [FromQuery] PaginationRequest dto)
            => Ok(await _productService.GetProductRatingsAsync(productId, dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpGet("{productId:int}/audit-logs")]
        public async Task<ActionResult<AuditLogDto>> GetProductAuditLogs(int productId, [FromQuery] PaginationRequest dto)
            => Ok(await _productService.GetProductAuditLogsAsync(productId, dto));

        [Authorize]
        [HttpPost("wish-list/{productId:int}")]
        public async Task<ActionResult<OperationResultModel>> AddProductToWishList(int productId)
            => Ok(await _productService.AddProductToWishListAsync(productId));

        [Authorize]
        [HttpDelete("wish-list/{productId:int}")]
        public async Task<ActionResult<OperationResultModel>> RemoveProductFromWishList(int productId)
            => Ok(await _productService.RemoveProductFromWishListAsync(productId));

        [Authorize]
        [HttpPost("re-stock-notification")]
        public async Task<ActionResult<OperationResultModel>> AddReStockNotification(AddReStockNotificationDto dto)
            => Ok(await _productService.AddReStockNotificationAsync(dto));

        [Authorize]
        [HttpPost("cart-item")]
        public async Task<ActionResult<OperationResultModel>> AddProductToCart(AddCartItemDto dto)
            => Ok(await _productService.AddProductToCartAsync(dto));

        [Authorize]
        [HttpPut("cart-item/{cartItemId:int}/quantity")]
        public async Task<ActionResult> UpdateCartItemQuantity(int cartItemId, UpdateCartItemDto dto)
        {
            await _productService.UpdateCartItemQuantityAsync(cartItemId, dto);
            return Ok();
        }

        [Authorize]
        [HttpDelete("cart-item/{cartItemId:int}")]
        public async Task<ActionResult> RemoveProductFromCart(int cartItemId)
        {
            await _productService.RemoveProductFromCartAsync(cartItemId);
            return Ok();
        }

    }
}
