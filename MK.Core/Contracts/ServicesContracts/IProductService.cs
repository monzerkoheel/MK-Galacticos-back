using MK.Core.DTOs;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IProductService
    {
        Task<OperationResultModel> CreateProductAsync(CreateProductDto dto);
        Task<OperationResultModel> EditProductAsync(int productId, EditProductDto dto);
        Task<OperationResultModel> EditProductDesignAsync(int productId,EditProductDesignDto dto);
        Task<OperationResultModel> EditProductCollectionsAsync(int productId, EditProductCollectionsDto dto);
        Task<OperationResultModel> EditProductPrintsAsync(int productId, EditProductPrintsDto dto);
        Task<OperationResultModel> EditProductStockAsync(int productId, EditProductStocksDto dto);
        Task<OperationResultModel> DeleteProductAsync(int productId);
        Task<PaginationModel<ProductsDto>> GetAllProductsAsync(ProductSpecPrams specPrams);
        Task<ProductsStatisticsDto> GetProductsStatisticsAsync();
        Task<PaginationModel<ActiveProductsDto>> GetAllActiveProductsAsync(ActiveProductsSpecPrams specPrams);
        Task<ActiveProductDto> GetActiveProductByIdAsync(int productId);
        Task<IReadOnlyList<ActiveProductDto>> GetSuggestedProductsAsync(int productId, int ProductCount);
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<PaginationModel<ProductOrdersDto>> GetProductOrdersAsync(int productId, PaginationRequest dto);
        Task<PaginationModel<ManagmentProductRatingDto>> GetProductRatingsAsync(int productId, PaginationRequest dto);
        Task<PaginationModel<AuditLogDto>> GetProductAuditLogsAsync(int productId, PaginationRequest dto);
        Task<OperationResultModel> AddProductToWishListAsync(int productId);
        Task<OperationResultModel> RemoveProductFromWishListAsync(int productId);
        Task<OperationResultModel> AddReStockNotificationAsync(AddReStockNotificationDto dto);
        Task<OperationResultModel> AddProductToCartAsync(AddCartItemDto dto);
        Task UpdateCartItemQuantityAsync(int cartItemId, UpdateCartItemDto dto);
        Task RemoveProductFromCartAsync(int cartItemId);
    }
}
