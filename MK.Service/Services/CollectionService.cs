using AutoMapper;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Collections;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;

namespace MK.Service.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CollectionService(IFileService fileService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _fileService = fileService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResultModel> CreateCollectionAsync(CreateCollectionDto dto)
        {
            ValidateTranslations(dto.Translations);
            dto.DisplayOrder = await GetAvailableDisplayOrderAsync(dto.DisplayOrder);

            var collection = _mapper.Map<Collection>(dto);
            collection.ImageUrl = await _fileService.SaveImageAsync(dto.Image, "uploads/images/collections");
            collection.CreatedAt = DateTime.UtcNow;
            collection.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Collection>().AddAsync(collection);
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new Exception(Messages.SomethingWentWrong);
            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.CollectionCreatedSuccessfully
            };
        }

        public async Task<OperationResultModel> EditCollectionAsync(int collectionId, EditCollectionDto dto)
        {
            var collectionSpec = new CollectionWithTranslationsSpecification(x => x.Id == collectionId);

            var collection = await _unitOfWork.Repository<Collection>().GetFirstOrDefaultAsync(collectionSpec);

            if (collection == null)
                throw new NotFoundException(Messages.CollectionNotFound);

            if (dto.Image == null &&
                dto.Slug == null &&
                dto.Status == null &&
                dto.DisplayOrder == null &&
               (dto.Translations == null || !dto.Translations.Any()))
            {
                throw new BadRequestException(Messages.NoDataToUpdate);
            }

            if (dto.Slug != null)
                collection.Slug = dto.Slug;

            if (dto.Status != null)
                collection.Status = dto.Status.Value;

            if (dto.DisplayOrder != null && dto.DisplayOrder != collection.DisplayOrder)
                collection.DisplayOrder = await AdjustDisplayOrderAsync(collection, dto.DisplayOrder.Value);

            if (dto.Image != null)
            {
                var newImageUrl = await _fileService.SaveImageAsync(dto.Image, "uploads/images/collections");
                if (!string.IsNullOrEmpty(collection.ImageUrl))
                    _fileService.DeleteImageIfExists(collection.ImageUrl);
                collection.ImageUrl = newImageUrl;
            }

            if (dto.Translations != null)
            {
                foreach (var translationDto in dto.Translations)
                {
                    var translation = collection.Translations.FirstOrDefault(x => x.Language == translationDto.Language);

                    if (translation == null)
                        throw new NotFoundException(string.Format(Messages.TranslationNotFound,translationDto.Language));

                    if (translationDto.Name != null)
                        translation.Name = translationDto.Name;

                    if (translationDto.Description != null)
                        translation.Description = translationDto.Description;
                }
            }

            collection.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Collection>().Update(collection);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.CollectionUpdatedSuccessfully
            };
        }

        public async Task<OperationResultModel> DeleteCollectionAsync(int collectionId)
        {
            var collection = await _unitOfWork.Repository<Collection>().GetByIdAsync(collectionId);

            if (collection == null)
                throw new NotFoundException(Messages.CollectionNotFound);

            _fileService.DeleteImageIfExists(collection.ImageUrl);

            _unitOfWork.Repository<Collection>().Delete(collection);
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.CollectionDeletedSuccessfully
            };
        }

        public async Task<IReadOnlyList<ActiveCollectionDto>?> GetAllActiveCollectionsAsync()
        {
            var collectionSpec = new CollectionWithTranslationsSpecification(x => x.Status == Visibility.Active);
            var collections = await _unitOfWork.Repository<Collection>().ListAsync(collectionSpec);

            return _mapper.Map<IReadOnlyList<ActiveCollectionDto>>(collections);
        }

        public async Task<IReadOnlyList<CollectionDto>?> GetAllCollectionsAsync()
        {
            var collectionSpec = new CollectionWithTranslationsSpecification(x => x.Status != null);
            collectionSpec.AddInclude(x => x.ProductCollections);
            var collections = await _unitOfWork.Repository<Collection>().ListAsync(collectionSpec);

            return _mapper.Map<IReadOnlyList<CollectionDto>>(collections);
        }

        public async Task<CollectionDetailsDto> GetCollectionDetailsByIdAsync(int collectionId)
        {
            var collectionSpec = new CollectionWithTranslationsSpecification(x => x.Id == collectionId);
            collectionSpec.AddInclude(x => x.ProductCollections);
            var collections = await _unitOfWork.Repository<Collection>().GetFirstOrDefaultAsync(collectionSpec);
            if (collections == null)
                throw new NotFoundException(Messages.CollectionNotFound);

            return _mapper.Map<CollectionDetailsDto>(collections);
        }

        public async Task<IReadOnlyList<SimpleCollectionDto>?> GetSimpleCollectionsAsync()
        {
            var collectionSpec = new CollectionWithTranslationsSpecification(x => x.Status != null);
            var collections = await _unitOfWork.Repository<Collection>().ListAsync(collectionSpec);

            return _mapper.Map<IReadOnlyList<SimpleCollectionDto>>(collections);
        }

        public async Task<PaginationModel<ActiveProductsDto>> GetCollectionProductsAsync(int collectionId, PaginationRequest dto)
        {
            var collections = await _unitOfWork.Repository<Collection>().GetByIdAsync(collectionId);
            if (collections == null)
                throw new NotFoundException(Messages.CollectionNotFound);

            var productSpec = new ProductWithAllDataSpecification(x => x.ProductCollections.Any(pc => pc.CollectionId == collectionId) && x.Visibility == Visibility.Active);
            productSpec.AddOrderBy(x => x.Id);
            var productsCount = await _unitOfWork.Repository<Product>().CountAsync(productSpec);
            productSpec.ApplyPagination((dto.PageIndex - 1) * dto.PageSize, dto.PageSize);
            var products = await _unitOfWork.Repository<Product>().ListAsync(productSpec);
            var data = _mapper.Map<IReadOnlyList<ActiveProductsDto>>(products);
            return new PaginationModel<ActiveProductsDto>
            (
                dto.PageIndex,
                dto.PageSize,
                productsCount,
                data
            );
        }

        public async Task<PaginationModel<ActiveProductsDto>> GetWomenCollectionAsync(PaginationRequest dto)
        {
            var productSpec = new ProductWithAllDataSpecification(x => x.AvailableFor != ProductAvailability.Men && x.Visibility == Visibility.Active);
            productSpec.AddOrderBy(x => x.Id);
            var productsCount = await _unitOfWork.Repository<Product>().CountAsync(productSpec);
            productSpec.ApplyPagination((dto.PageIndex - 1) * dto.PageSize, dto.PageSize);
            var products = await _unitOfWork.Repository<Product>().ListAsync(productSpec);

            var data = new List<ActiveProductsDto>();

            foreach (var product in products)
            {
                var productDto = _mapper.Map<ActiveProductsDto>(product);

                if (product.AvailableFor == ProductAvailability.Unisex)
                {
                    var mainImage = product.Images.FirstOrDefault(x => x.IsMain && x.Gender == ProductGender.Women);
                    if (mainImage != null)
                        productDto.MainImageUrl = _fileService.GetFullUrl(mainImage.ImageUrl);
                }

                data.Add(productDto);
            }


            return new PaginationModel<ActiveProductsDto>
            (
                dto.PageIndex,
                dto.PageSize,
                productsCount,
                data
            );
        }



        #region Helper Methods
        private void ValidateTranslations(ICollection<CollectionTranslationDto> translations)
        {
            if (translations is null || translations.Count < 3)
                throw new BadRequestException(Messages.AllTranslationsRequired);

            var duplicatedLanguages = translations.GroupBy(x => x.Language).Any(x => x.Count() > 1);

            if (duplicatedLanguages)
                throw new BadRequestException(Messages.DuplicateTranslations);

            foreach (var translation in translations)
            {
                if (string.IsNullOrWhiteSpace(translation.Name))
                    throw new BadRequestException(string.Format(Messages.TranslationRequired, translation.Language, nameof(translation.Name)));

                if (string.IsNullOrWhiteSpace(translation.Description))
                    throw new BadRequestException(string.Format(Messages.TranslationRequired, translation.Language, nameof(translation.Description)));
            }
        }
        private async Task<int> GetAvailableDisplayOrderAsync(int requestedOrder)
        {
            var collections = await _unitOfWork.Repository<Collection>().GetAllAsync();

            if (!collections.Any())
                return 1;

            var maxOrder = collections.Max(x => x.DisplayOrder);

            if (requestedOrder > maxOrder + 1)
                return maxOrder + 1;

            if (!collections.Any(x => x.DisplayOrder == requestedOrder))
                return requestedOrder;

            foreach (var collection in collections
                         .Where(x => x.DisplayOrder >= requestedOrder)
                         .OrderByDescending(x => x.DisplayOrder))
            {
                collection.DisplayOrder++;
            }

            return requestedOrder;
        }
        private async Task<int> AdjustDisplayOrderAsync(Collection collection, int newDisplayOrder)
        {
            var oldDisplayOrder = collection.DisplayOrder;

            if (oldDisplayOrder == newDisplayOrder)
                return oldDisplayOrder;

            var collectionsSpec = new BaseSpecification<Collection>(x => x.Id != collection.Id);
            var collections = await _unitOfWork.Repository<Collection>().ListAsync(collectionsSpec);

            if (!collections.Any())
            {
                return 1;
            }

            var maxDisplayOrder = collections.Max(x => x.DisplayOrder);

            if (newDisplayOrder > oldDisplayOrder)
            {
                if (newDisplayOrder >= maxDisplayOrder)
                    newDisplayOrder = maxDisplayOrder;

                foreach (var item in collections.Where(x => x.DisplayOrder > oldDisplayOrder && 
                                                       x.DisplayOrder <= newDisplayOrder)
                                                .OrderBy(x => x.DisplayOrder))
                {
                    item.DisplayOrder--;
                }
            }
            else
            {
                if (newDisplayOrder < 1)
                    newDisplayOrder = 1;

                foreach (var item in collections.Where(x => x.DisplayOrder >= newDisplayOrder &&
                                                       x.DisplayOrder < oldDisplayOrder)
                                                .OrderByDescending(x => x.DisplayOrder))
                {
                    item.DisplayOrder++;
                }
            }

            return newDisplayOrder;
        }

        #endregion
    }
}
