using MK.Core.DTOs.Collections;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface ICollectionService
    {
        Task<OperationResultModel> CreateCollectionAsync(CreateCollectionDto dto);
        Task<OperationResultModel> EditCollectionAsync(int collectionId, EditCollectionDto dto);
        Task<OperationResultModel> DeleteCollectionAsync(int collectionId);
        Task<CollectionDetailsDto> GetCollectionDetailsByIdAsync(int collectionId);
        Task<IReadOnlyList<CollectionDto>?> GetAllCollectionsAsync();
        Task<IReadOnlyList<ActiveCollectionDto>?> GetAllActiveCollectionsAsync();
        Task<IReadOnlyList<SimpleCollectionDto>?> GetSimpleCollectionsAsync();
        Task<PaginationModel<ActiveProductsDto>> GetCollectionProductsAsync(int collectionId, PaginationRequest dto);
        Task<PaginationModel<ActiveProductsDto>> GetWomenCollectionAsync(PaginationRequest dto);
    }
}
