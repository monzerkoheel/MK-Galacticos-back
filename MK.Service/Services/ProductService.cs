using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using MK.Repository.Specifications;
using MK.Repository.Specifications.Products_Specifications;
using MK.Service.HelperServices.EmailSevrices;
using System.Linq.Expressions;

namespace MK.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentLanguage _currentLanguage;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProductService(IUnitOfWork unitOfWork, 
            ICurrentLanguage currentLanguage, 
            IMapper mapper, 
            IFileService fileService,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager) 
        {
            _unitOfWork = unitOfWork;
            _currentLanguage = currentLanguage;
            _mapper = mapper;
            _fileService = fileService;
            _emailService = emailService;
            _currentUser = currentUserService;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<OperationResultModel> CreateProductAsync(CreateProductDto dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            await CheckSlug(dto.Slug);
            await CheckSKU(dto.SKU);
            ValidateTranslations(dto.Translations);
            ValidateStocks(dto.Stocks);
            dto.Images = ValidateImages(dto.Images);
            ICollection<ProductImageDto> imagesDto = new List<ProductImageDto>(); 
            foreach (var image in dto.Images) 
            {
                var imageFolder = "uploads/images/products/male";
                if (image.Gender == ProductGender.Women)
                    imageFolder = "uploads/images/products/female";
                var imageUrl = await _fileService.SaveImageAsync(image.Image, imageFolder);
                imagesDto.Add(new ProductImageDto
                {
                    ImageUrl = imageUrl,
                    IsMain = image.IsMain,
                    DisplayOrder = image.DisplayOrder,
                    Gender = image.Gender,
                });
            }

            var product = _mapper.Map<Product>(dto);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.Images = _mapper.Map<ICollection<ProductImage>>(imagesDto);
            try
            {
                await _productRepo.AddAsync(product);
                var result = await _unitOfWork.CompleteAsync();
                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);
            }
            catch (Exception ex)
            { 
                foreach (var image in product.Images)
                    _fileService.DeleteImageIfExists(image.ImageUrl);
                throw;
            }
            
            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.CreatedSuccessfully, nameof(Product))
            };
        }

        public async Task<OperationResultModel> EditProductAsync(int productId, EditProductDto dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));
            
            if (dto.SKU == null &&
                dto.ProductType == null &&
                dto.AvailableFor == null &&
                dto.Visibility == null &&
                dto.Price == null &&
                dto.SalePrice == -1 &&
                dto.LowStockThreshold == null &&
                dto.IsCustomPrintAvailable == null)
                throw new BadRequestException(Messages.NoDataToUpdate);

            if (dto.SKU is not null)
                await CheckNewSKU(productId, dto.SKU);
            
            _mapper.Map(dto, product);
            if(dto.SalePrice != -1)
                product.SalePrice = dto.SalePrice;
            if (dto.Visibility is not null && dto.Visibility == Visibility.Inactive)
            {
                var cartItemsSpec = new BaseSpecification<CartItem>(x => x.ProductId == productId);
                var cartItems = await _unitOfWork.Repository<CartItem>().ListAsync(cartItemsSpec);
                var wishlistItemsSpec = new BaseSpecification<WishlistItem>(x => x.ProductId == productId);
                var wishlistItems = await _unitOfWork.Repository<WishlistItem>().ListAsync(wishlistItemsSpec);
                _unitOfWork.Repository<CartItem>().DeleteRange(cartItems);
                _unitOfWork.Repository<WishlistItem>().DeleteRange(wishlistItems);
            }
            product.UpdatedAt = DateTime.UtcNow;

            _productRepo.Update(product);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.UpdatedSuccessfully, "Product")
            };
        }

        public async Task<OperationResultModel> EditProductDesignAsync(int productId, EditProductDesignDto dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var productSpec = new ProductWithTranslationsSpecification(x => x.Id == productId);
            productSpec.AddInclude(x => x.Images);
            var product = await _productRepo.GetFirstOrDefaultAsync(productSpec);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            if (dto.Slug == null &&
                dto.IsFeatured == null &&
                dto.IsNewArrival == null &&
                (dto.Translations == null || !dto.Translations.Any()) &&
                (dto.DeletedImageIds == null || !dto.DeletedImageIds.Any()) &&
                (dto.Images == null || !dto.Images.Any()) &&
                (dto.NewImages == null || !dto.NewImages.Any()))
                throw new BadRequestException(Messages.NoDataToUpdate);

            if (dto.Slug is not null)
            {
                await CheckNewSlug(productId, dto.Slug);
                product.Slug = dto.Slug;
            }

            if (dto.IsFeatured is not null)
                product.IsFeatured = dto.IsFeatured.Value;

            if (dto.IsNewArrival is not null)
                product.IsNewArrival = dto.IsNewArrival.Value;

            if (dto.Translations is not null && dto.Translations.Any())
            {
                var checkTranslations = dto.Translations.GroupBy(x => x.Language).Any(x => x.Count() > 1);
                if (checkTranslations)
                    throw new BadRequestException(Messages.DuplicateTranslations);

                foreach (var translationDto in dto.Translations)
                {
                    var translation = product.Translations.FirstOrDefault(x => x.Language == translationDto.Language);

                    if (translation is null)
                        throw new BadRequestException(string.Format(Messages.TranslationNotFound, translationDto.Language));

                    _mapper.Map(translationDto, translation);
                }
            }
            
            List<string> deletedImagesUrls = new List<string>();
            List<string> newImagesUrls = new List<string>();
            try
            {
                var productImages = product.Images;

                if (dto.DeletedImageIds is not null && dto.DeletedImageIds.Any())
                {
                    foreach (var imageId in dto.DeletedImageIds)
                    {
                        var image = productImages.FirstOrDefault(x => x.Id == imageId);
                        if (image is not null)
                        {
                            deletedImagesUrls.Add(image.ImageUrl);
                            productImages.Remove(image);
                            _unitOfWork.Repository<ProductImage>().Delete(image);
                        }
                    }
                }

                if (dto.Images is not null && dto.Images.Any())
                {
                    foreach (var imageDto in dto.Images)
                    {
                        var image = await _unitOfWork.Repository<ProductImage>().GetByIdAsync(imageDto.Id);
                        if (image is null)
                        {
                            if (image.ProductId != productId)
                                throw new BadRequestException("Image not belongs to this product");
                            throw new NotFoundException(string.Format(Messages.NotFound, "Image"));
                        }
                        _mapper.Map(imageDto, image);
                    }
                }

                if (dto.NewImages is not null && dto.NewImages.Any())
                {
                    foreach (var image in dto.NewImages)
                    {
                        var imageFolder = "uploads/images/products/male";
                        if (image.Gender == ProductGender.Women)
                            imageFolder = "uploads/images/products/female";
                        var imageUrl = await _fileService.SaveImageAsync(image.Image, imageFolder);

                        newImagesUrls.Add(imageUrl);
                        productImages.Add(new ProductImage
                        {
                            ProductId = productId,
                            ImageUrl = imageUrl,
                            IsMain = image.IsMain,
                            DisplayOrder = image.DisplayOrder,
                            Gender = image.Gender,
                        });
                    }
                }

                await _unitOfWork.BeginTransactionAsync(); 
                try
                {
                    productImages = HandelProductMainImage(productImages);
                    product.Images = productImages;
                    _productRepo.Update(product);
                    var result = await _unitOfWork.CompleteAsync();
                    if (result < 1) 
                        throw new Exception(Messages.SomethingWentWrong);

                    product.Images = ValidateProductImages(productImages);
                    product.UpdatedAt = DateTime.UtcNow;

                    _productRepo.Update(product);
                    result = await _unitOfWork.CompleteAsync();
                    if (result < 1) 
                        throw new Exception(Messages.SomethingWentWrong);

                    await _unitOfWork.CommitTransactionAsync();

                    foreach (var deleteImage in deletedImagesUrls)
                        _fileService.DeleteImageIfExists(deleteImage);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception)
            {
                foreach (var addImageUrl in newImagesUrls)
                    _fileService.DeleteImageIfExists(addImageUrl);
                throw;
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.UpdatedSuccessfully, "Product")
            };
        }

        public async Task<OperationResultModel> EditProductCollectionsAsync(int productId, EditProductCollectionsDto dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var productSpec = new BaseSpecification<Product>(x => x.Id == productId);
            productSpec.AddInclude(x => x.ProductCollections);
            var product = await _productRepo.GetFirstOrDefaultAsync(productSpec);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            if (!dto.DeletedCollectionsIds.Any() && !dto.AddedCollectionsIds.Any())
                throw new BadRequestException(Messages.NoDataToUpdate);


            if (dto.DeletedCollectionsIds.Any())
            {
                var deletedCollections = product.ProductCollections.Where(x => dto.DeletedCollectionsIds.Contains(x.CollectionId)).ToList();
                if (deletedCollections.Any())
                    _unitOfWork.Repository<ProductCollection>().DeleteRange(deletedCollections);
            }
            
            if (dto.AddedCollectionsIds.Any())
            {
                var collectionsSpec = new BaseSpecification<Collection>(x => dto.AddedCollectionsIds.Contains(x.Id));
                var collections = await _unitOfWork.Repository<Collection>().ListAsync(collectionsSpec);
                if (collections.Any())
                {
                    var collectionsId = collections.Select(x => x.Id).ToList();
                    var existingCollections = product.ProductCollections.Select(x => x.CollectionId).ToList();
                    var newCollections = collectionsId.Except(existingCollections);
                    foreach (var collectionId in newCollections)
                    {
                        product.ProductCollections.Add(new ProductCollection
                        {
                            ProductId = productId,
                            CollectionId = collectionId
                        });
                    }
                }
            }

            product.UpdatedAt = DateTime.UtcNow;
            _productRepo.Update(product);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.UpdatedSuccessfully, "Product")
            };

        }

        public async Task<OperationResultModel> EditProductPrintsAsync(int productId, EditProductPrintsDto dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var productSpec = new BaseSpecification<Product>(x => x.Id == productId);
            productSpec.AddInclude(x => x.Prints);
            var product = await _productRepo.GetFirstOrDefaultAsync(productSpec);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            if (!dto.DeletedPrintsIds.Any() && !dto.AddedPrints.Any())
                throw new BadRequestException(Messages.NoDataToUpdate);

            if (dto.DeletedPrintsIds.Any())
            {
                var deletedPrints = product.Prints.Where(x => dto.DeletedPrintsIds.Contains(x.Id)).ToList();
                if (deletedPrints.Any())
                    _unitOfWork.Repository<ProductPrint>().DeleteRange(deletedPrints);
            }

            if (dto.AddedPrints.Any())
            {
                var addedPrints = _mapper.Map<ICollection<ProductPrint>>(dto.AddedPrints);
                foreach (var print in addedPrints)
                {
                    print.ProductId = productId;
                    product.Prints.Add(print);
                }
            }

            product.UpdatedAt = DateTime.UtcNow;
            _productRepo.Update(product);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.UpdatedSuccessfully, "Product")
            };
        }

        public async Task<OperationResultModel> EditProductStockAsync(int productId, EditProductStocksDto dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var productSpec = new BaseSpecification<Product>(x => x.Id == productId);
            productSpec.AddInclude(x => x.Stocks);
            productSpec.AddInclude(x => x.Translations);
            productSpec.AddInclude(x => x.Images);
            var product = await _productRepo.GetFirstOrDefaultAsync(productSpec);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            if (!dto.DeletedStockIds.Any() && !dto.AddedStocks.Any() && !dto.RestockStock.Any())
                throw new BadRequestException(Messages.NoDataToUpdate);

            if (dto.DeletedStockIds.Any())
            {
                var deletedStocks = product.Stocks.Where(x => dto.DeletedStockIds.Contains(x.Id)).ToList();
                if (deletedStocks.Any())
                {
                    foreach (var stock in deletedStocks)
                        product.Stocks.Remove(stock);

                    _unitOfWork.Repository<ProductStock>().DeleteRange(deletedStocks);
                    await _unitOfWork.CompleteAsync();
                }
            }

            if (dto.AddedStocks.Any())
            {
                ValidateStocks(dto.AddedStocks);
                var addedStocks = _mapper.Map<ICollection<ProductStock>>(dto.AddedStocks);
                var existingStocks = product.Stocks.ToList();
                foreach (var stock in addedStocks)
                {
                    if (existingStocks.Any(x => x.Size == stock.Size && x.StockType == stock.StockType))
                        throw new BadRequestException(string.Format(Messages.DuplicateStocks, stock.Size.ToString(), stock.StockType.ToString()));
                    
                    stock.ProductId = productId;
                    product.Stocks.Add(stock);
                }
            }

            if (dto.RestockStock.Any())
            {
                foreach (var restock in dto.RestockStock)
                {
                    var stock = product.Stocks.FirstOrDefault(x => x.Id == restock.Id);
                    if (stock is null)
                        throw new NotFoundException(string.Format(Messages.NotFound, "Stock"));
                    if (stock.ProductId != productId)
                        throw new BadRequestException("Stock not belongs to this product");
                    if (stock.StockQuantity + restock.StockQuantity < 0)
                        throw new BadRequestException("Stock quantity cannot be negative");
                    stock.StockQuantity += restock.StockQuantity;
                    if (restock.StockQuantity > 0)
                    {
                        var notificationSpec = new BaseSpecification<AvailabilityNotification>(x => x.ProductId == stock.ProductId 
                                                                                            && x.ProductSize == stock.Size
                                                                                            && x.Type == AvailabilityNotificationType.ReStockProduct
                                                                                            && x.IsSent == false);
                        var notifications = await _unitOfWork.Repository<AvailabilityNotification>().ListAsync(notificationSpec);
                        foreach (var notification in notifications)
                        {
                            var user = await _userManager.FindByIdAsync(notification.UserId);
                            if (user is null || string.IsNullOrWhiteSpace(user.Email))
                                continue;
                            var productName = product.Translations.FirstOrDefault(x => x.Language == Language.English)?.Name ?? string.Empty;
                            var productImageUrl = product.Images.FirstOrDefault(x => x.IsMain)?.ImageUrl ?? string.Empty;
                            await SendBackInStockEmailAsync(user.Email, productName, productImageUrl, product.Id);
                            notification.IsSent = true;
                            notification.SentAt = DateTime.UtcNow;
                            _unitOfWork.Repository<AvailabilityNotification>().Update(notification);
                        }
                    }
                }
                var totalStockQuantity = product.Stocks.Sum(x => x.StockQuantity);
                if (totalStockQuantity == 0)
                    product.IsOutOfStock = true;
                else if (totalStockQuantity <= product.LowStockThreshold)
                    product.IsOutOfStock = false;
                else
                    product.IsOutOfStock = false;
            }

            product.UpdatedAt = DateTime.UtcNow;
            _productRepo.Update(product);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.UpdatedSuccessfully, "Product")
            };
        }

        public async Task<OperationResultModel> DeleteProductAsync(int productId)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));
            var CartItemsSpec = new BaseSpecification<CartItem>(x => x.ProductId == productId);
            var WishlistItemsSpec = new BaseSpecification<WishlistItem>(x => x.ProductId == productId);
            var AvailabilityNotificationsSpec = new BaseSpecification<AvailabilityNotification>(x => x.ProductId == productId);
            var OrderItemsSpec = new BaseSpecification<OrderItem>(x => x.ProductId == productId);
            var cartItems = await _unitOfWork.Repository<CartItem>().ListAsync(CartItemsSpec);
            var wishlistItems = await _unitOfWork.Repository<WishlistItem>().ListAsync(WishlistItemsSpec);
            var availabilityNotifications = await _unitOfWork.Repository<AvailabilityNotification>().ListAsync(AvailabilityNotificationsSpec);
            var orderItems = await _unitOfWork.Repository<OrderItem>().ListAsync(OrderItemsSpec);

            _unitOfWork.Repository<CartItem>().DeleteRange(cartItems);
            _unitOfWork.Repository<WishlistItem>().DeleteRange(wishlistItems);
            _unitOfWork.Repository<AvailabilityNotification>().DeleteRange(availabilityNotifications);
            foreach (var orderItem in orderItems)
            {
                orderItem.ProductId = null;
                _unitOfWork.Repository<OrderItem>().Update(orderItem);
            }

            try
            {
                _productRepo.Delete(product);
                await _unitOfWork.CompleteAsync();
                foreach (var image in product.Images)
                    _fileService.DeleteImageIfExists(image.ImageUrl);
            }
            catch (Exception ex)
            {
                throw new Exception(Messages.Product_CannotDeleteWithRelatedData, ex);
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.DeletedSuccessfully, "Product")
            };
        }

        public async Task<PaginationModel<ProductsDto>> GetAllProductsAsync(ProductSpecPrams specPrams)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var spec = new AllProductsSpecification(specPrams);
            var products = await _productRepo.ListAsync(spec);
            var pageIndex = specPrams.PageIndex;
            var pageSize = specPrams.PageSize;
            specPrams.PageSize = specPrams.PageIndex = 0;
            var countSpec = new AllProductsSpecification(specPrams);
            var totalCount = await _productRepo.CountAsync(countSpec);
            var data = _mapper.Map<IReadOnlyList<ProductsDto>>(products);
            return new PaginationModel<ProductsDto>(pageIndex, pageSize, totalCount, data);
        }

        public async Task<ProductsStatisticsDto> GetProductsStatisticsAsync()
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var productsSpec = new BaseSpecification<Product>();
            productsSpec.Includes.Add(x => x.Stocks);
            var products = await _productRepo.ListAsync(productsSpec);
            var productsStatisticsDto = new ProductsStatisticsDto
            {
                TotalProducts = products.Count(),
                ActiveProducts = products.Count(x => x.Visibility == Visibility.Active),
                OutOfStockProducts = products.Count(x => x.IsOutOfStock && x.Stocks.Sum(s => s.StockQuantity) == 0),
                LowStockProducts = products.Count(x => !x.IsOutOfStock && 
                                                        x.Stocks.Sum(s => s.StockQuantity) > 0 &&
                                                        x.Stocks.Sum(s => s.StockQuantity) <= x.LowStockThreshold)
            };
            return productsStatisticsDto;
        }

        public async Task<PaginationModel<ActiveProductsDto>> GetAllActiveProductsAsync(ActiveProductsSpecPrams specPrams)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var spec = new AllActiveProductsSpecification(specPrams, _currentLanguage.Language);
            var products = await _productRepo.ListAsync(spec);
            
            var pageIndex = specPrams.PageIndex;
            var pageSize = specPrams.PageSize;
            specPrams.PageSize = specPrams.PageIndex = 0;
            var countSpec = new AllActiveProductsSpecification(specPrams, _currentLanguage.Language);
            var totalCount = await _productRepo.CountAsync(countSpec);
            var data = _mapper.Map<IReadOnlyList<ActiveProductsDto>>(products);
            return new PaginationModel<ActiveProductsDto>(pageIndex, pageSize, totalCount, data);
        }

        public async Task<ActiveProductDto> GetActiveProductByIdAsync(int productId)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var spec = new ProductWithAllDataSpecification(x => x.Id == productId && x.Visibility == Visibility.Active);
            var product = await _productRepo.GetFirstOrDefaultAsync(spec);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));
            
            var result = new ActiveProductDto();
            result.Id = product.Id;
            result.Name = product.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Name ?? string.Empty;
            result.Description = product.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Description ?? string.Empty;
            result.AvailableFor = product.AvailableFor;
            result.Price = product.Price;
            result.SalePrice = product.SalePrice;
            result.IsCustomPrintAvailable = product.IsCustomPrintAvailable;
            result.IsOutOfStock = product.IsOutOfStock;
            result.AverageRating  = product.AverageRating;
            result.RatingCount = product.RatingCount;

            result.MaleImages = _mapper.Map<List<ActiveProductImageDto>>(product.Images.Where(x => x.Gender == ProductGender.Men).OrderBy(x => x.DisplayOrder));
            if (product.AvailableFor != ProductAvailability.Men)
                result.FemaleImages = _mapper.Map<List<ActiveProductImageDto>>(product.Images.Where(x => x.Gender == ProductGender.Women).OrderBy(x => x.DisplayOrder));

            if (!product.IsOutOfStock)
                result.Stock = _mapper.Map<List<ProductStockDto>>(product.Stocks);

            result.AvailablePrints = _mapper.Map<List<ProductPrintDto>>(product.Prints);
            return result;
        }

        public async Task<IReadOnlyList<ActiveProductDto>> GetSuggestedProductsAsync(int productId, int ProductCount)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var spec = new ProductWithTranslationsSpecification(x => x.Id == productId && x.Visibility == Visibility.Active);
            spec.AddInclude(x => x.ProductCollections);
            var product = await _productRepo.GetFirstOrDefaultAsync(spec);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            var collectionIds = product.ProductCollections.Select(x => x.CollectionId).ToList();

            var suggestedSpec = new ProductWithAllDataSpecification(
                x => x.Id != productId
                    && x.ProductCollections.Any(pc =>
                            collectionIds.Contains(pc.CollectionId))
                    && x.Visibility == Visibility.Active
                    && x.IsOutOfStock != true);
            suggestedSpec.AddOrderBy(x => x.Id);
            suggestedSpec.ApplyPagination(0, 20);
            var suggestedProducts = await _productRepo.ListAsync(suggestedSpec);
            suggestedProducts = suggestedProducts.OrderBy(x => Guid.NewGuid()).ToList();
            suggestedProducts = suggestedProducts.Take(ProductCount).ToList();
            var finalResult = new List<ActiveProductDto>();
            foreach(var suggestedProduct in suggestedProducts)
            {
                var result = new ActiveProductDto();
                result.Id = suggestedProduct.Id;
                result.Name = suggestedProduct.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Name ?? string.Empty;
                result.Description = suggestedProduct.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Description ?? string.Empty;
                result.AvailableFor = suggestedProduct.AvailableFor;
                result.Price = suggestedProduct.Price;
                result.SalePrice = suggestedProduct.SalePrice;
                result.IsCustomPrintAvailable = suggestedProduct.IsCustomPrintAvailable;
                result.IsOutOfStock = suggestedProduct.IsOutOfStock;
                result.AverageRating = suggestedProduct.AverageRating;
                result.RatingCount = suggestedProduct.RatingCount;

                result.MaleImages = _mapper.Map<List<ActiveProductImageDto>>(suggestedProduct.Images.Where(x => x.Gender == ProductGender.Men).OrderBy(x => x.DisplayOrder));
                if (suggestedProduct.AvailableFor != ProductAvailability.Men)
                    result.FemaleImages = _mapper.Map<List<ActiveProductImageDto>>(suggestedProduct.Images.Where(x => x.Gender == ProductGender.Women).OrderBy(x => x.DisplayOrder));
                if (!suggestedProduct.IsOutOfStock)
                    result.Stock = _mapper.Map<List<ProductStockDto>>(suggestedProduct.Stocks);

                result.AvailablePrints = _mapper.Map<List<ProductPrintDto>>(suggestedProduct.Prints);
                finalResult.Add(result);
            }
           
            return finalResult;
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var productSpec = new ProductWithAllDataSpecification(x => x.Id == productId);
            var product = await _productRepo.GetFirstOrDefaultAsync(productSpec);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            var result = _mapper.Map<ProductDto>(product);

            // Collections
            var collectionsSpec = new BaseSpecification<Collection>(x => x.ProductCollections.Any(pc => pc.ProductId == productId));
            collectionsSpec.AddInclude(x => x.Translations);
            var collections = await _unitOfWork.Repository<Collection>().ListAsync(collectionsSpec);
            result.ProductCollections = _mapper.Map<List<ProductCollectionDto>>(collections);

            // Orders (newest first)
            var orderItemsSpec = new BaseSpecification<OrderItem>(x => x.ProductId == productId);
            orderItemsSpec.AddInclude(x => x.Order);
            orderItemsSpec.AddOrderByDes(x => x.Order.CreatedAt);
            orderItemsSpec.ApplyPagination(0, 20);
            var orderItems = await _unitOfWork.Repository<OrderItem>().ListAsync(orderItemsSpec);
            result.Orders = _mapper.Map<List<ProductOrdersDto>>(orderItems);

            // Ratings (newest first)
            var ratingsSpec = new BaseSpecification<ProductRating>(x => x.ProductId == productId);
            ratingsSpec.AddInclude(x => x.User);
            ratingsSpec.AddOrderByDes(x => x.UpdatedAt);
            ratingsSpec.ApplyPagination(0, 20);
            var ratings = await _unitOfWork.Repository<ProductRating>().ListAsync(ratingsSpec);
            result.Ratings = _mapper.Map<List<ManagmentProductRatingDto>>(ratings);

            // Counts
            result.WishlistItemsCount = await _unitOfWork.Repository<WishlistItem>()
                .CountAsync(new BaseSpecification<WishlistItem>(x => x.ProductId == productId));

            result.CartItemsCount = await _unitOfWork.Repository<CartItem>()
                .CountAsync(new BaseSpecification<CartItem>(x => x.ProductId == productId));

            result.WaitingReStockNotificationsCount = await _unitOfWork.Repository<AvailabilityNotification>()
                .CountAsync(new BaseSpecification<AvailabilityNotification>(x =>
                    x.ProductId == productId &&
                    x.Type == AvailabilityNotificationType.ReStockProduct &&
                    !x.IsSent));

            return result;
        }

        public async Task<PaginationModel<ProductOrdersDto>> GetProductOrdersAsync (int productId, PaginationRequest dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            var orderSpec = new BaseSpecification<OrderItem>(x => x.ProductId == productId);
            orderSpec.AddInclude(x => x.Order);
            orderSpec.AddOrderByDes(x => x.Order.CreatedAt);
            var totalCount = await _unitOfWork.Repository<OrderItem>().CountAsync(orderSpec);
            orderSpec.ApplyPagination((dto.PageSize * (dto.PageIndex - 1 )), dto.PageSize);
            var orders = await _unitOfWork.Repository<OrderItem>().ListAsync(orderSpec);
            var result = _mapper.Map<List<ProductOrdersDto>>(orders);
            return new PaginationModel<ProductOrdersDto>(
                dto.PageIndex, 
                dto.PageSize,
                totalCount,
                result);
        }

        public async Task<PaginationModel<ManagmentProductRatingDto>> GetProductRatingsAsync(int productId, PaginationRequest dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            var ratingsSpec = new BaseSpecification<ProductRating>(x => x.ProductId == productId);
            ratingsSpec.AddInclude(x => x.User);
            ratingsSpec.AddOrderByDes(x => x.UpdatedAt);
            var totalCount = await _unitOfWork.Repository<ProductRating>().CountAsync(ratingsSpec);
            ratingsSpec.ApplyPagination((dto.PageSize * (dto.PageIndex - 1 )), dto.PageSize);
            var ratings = await _unitOfWork.Repository<ProductRating>().ListAsync(ratingsSpec);
            var result = _mapper.Map<List<ManagmentProductRatingDto>>(ratings);
            return new PaginationModel<ManagmentProductRatingDto>(
                dto.PageIndex,
                dto.PageSize,
                totalCount,
                result);
        }

        public async Task<PaginationModel<AuditLogDto>> GetProductAuditLogsAsync(int productId, PaginationRequest dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(Product)));

            var productIdStr = productId.ToString();

            var stockIds = await GetEntityIdsAsync<ProductStock>(x => x.ProductId == productId);
            var imageIds = await GetEntityIdsAsync<ProductImage>(x => x.ProductId == productId);
            var printIds = await GetEntityIdsAsync<ProductPrint>(x => x.ProductId == productId);
            var translationIds = await GetEntityIdsAsync<ProductTranslation>(x => x.ProductId == productId);
            var collectionIds = await GetEntityIdsAsync<ProductCollection>(x => x.ProductId == productId);

            var auditLogSpec = new AuditLogSpecification(x =>
                (
                    (x.EntityName == nameof(Product) && x.EntityId == productIdStr) ||
                    (x.EntityName == nameof(ProductStock) && stockIds.Contains(x.EntityId!)) ||
                    (x.EntityName == nameof(ProductImage) && imageIds.Contains(x.EntityId!)) ||
                    (x.EntityName == nameof(ProductPrint) && printIds.Contains(x.EntityId!)) ||
                    (x.EntityName == nameof(ProductTranslation) && translationIds.Contains(x.EntityId!)) ||
                    (x.EntityName == nameof(ProductCollection) && collectionIds.Contains(x.EntityId!))
                )
                && (x.Action != AuditAction.Update || x.OldValues != null || x.NewValues != null));
            var totalCount = await _unitOfWork.Repository<AuditLog>().CountAsync(auditLogSpec);
            auditLogSpec.ApplyPagination((dto.PageSize * (dto.PageIndex - 1 )), dto.PageSize);
            var auditLogs = await _unitOfWork.Repository<AuditLog>().ListAsync(auditLogSpec);
            var result = _mapper.Map<List<AuditLogDto>>(auditLogs);
            return new PaginationModel<AuditLogDto>(
                dto.PageIndex,
                dto.PageSize,
                totalCount,
                result);
        }

        public async Task<OperationResultModel> AddProductToWishListAsync(int productId)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null || product.Visibility != Visibility.Active)
                throw new BadRequestException(string.Format(Messages.NotFound, nameof(Product)));

            if(_currentUser.UserId is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var wishlistSpec = new BaseSpecification<Wishlist>(x => x.UserId == _currentUser.UserId);
            var wishlist = await _unitOfWork.Repository<Wishlist>().GetFirstOrDefaultAsync(wishlistSpec);
            if (wishlist is null)
            {
                wishlist = new Wishlist
                {
                    UserId = _currentUser.UserId
                };
                await _unitOfWork.Repository<Wishlist>().AddAsync(wishlist);
                var result = await _unitOfWork.CompleteAsync();
                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);
                wishlist = await _unitOfWork.Repository<Wishlist>().GetFirstOrDefaultAsync(wishlistSpec);
            }
            var wishlistItemSpec = new BaseSpecification<WishlistItem>(x => x.WishlistId == wishlist!.Id && x.ProductId == productId);
            var wishlistItemCheck = await _unitOfWork.Repository<WishlistItem>().AnyAsync(wishlistItemSpec);
            if (wishlistItemCheck)
                throw new BadRequestException(Messages.Product_IsAlreadyExit);

            var wishlistItem = new WishlistItem
            {
                WishlistId = wishlist!.Id,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<WishlistItem>().AddAsync(wishlistItem);
            var result02 = await _unitOfWork.CompleteAsync();
            if (result02 < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.AddedSuccessfully
            };
        }

        public async Task<OperationResultModel> RemoveProductFromWishListAsync(int productId)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(productId);
            if (product is null || product.Visibility != Visibility.Active)
                throw new BadRequestException(string.Format(Messages.NotFound, nameof(Product)));

            if(_currentUser.UserId is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var wishlistSpec = new BaseSpecification<Wishlist>(x => x.UserId == _currentUser.UserId);
            var wishlist = await _unitOfWork.Repository<Wishlist>().GetFirstOrDefaultAsync(wishlistSpec);
            if (wishlist is null)
                throw new BadRequestException(Messages.Wishlist_NotFound);
            var wishlistItemSpec = new BaseSpecification<WishlistItem>(x => x.WishlistId == wishlist.Id && x.ProductId == productId);
            var wishlistItem = await _unitOfWork.Repository<WishlistItem>().GetFirstOrDefaultAsync(wishlistItemSpec);
            if (wishlistItem is null)
                throw new BadRequestException(Messages.Product_IsNotInWishlist);
            

            _unitOfWork.Repository<WishlistItem>().Delete(wishlistItem);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.RemovedSuccessfully
            };
        }

        public async Task<OperationResultModel> AddReStockNotificationAsync(AddReStockNotificationDto dto)
        {
            var _productRepo = _unitOfWork.Repository<Product>();
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product is null || product.Visibility != Visibility.Active)
                throw new BadRequestException(string.Format(Messages.NotFound, nameof(Product)));

            var notificationSpec = new BaseSpecification<AvailabilityNotification>(x => x.ProductId == dto.ProductId 
                                                                                    && x.ProductSize == dto.ProductSize 
                                                                                    && x.Type == AvailabilityNotificationType.ReStockProduct
                                                                                    && x.UserId == _currentUser.UserId
                                                                                    && x.IsSent == false);
            var notificationCheck = await _unitOfWork.Repository<AvailabilityNotification>().AnyAsync(notificationSpec);
            if (notificationCheck)
                throw new BadRequestException(Messages.Notification_IsAlreadyExit);

            var notification = new AvailabilityNotification
            {
                UserId = _currentUser.UserId,
                ProductId = dto.ProductId,
                ProductSize = dto.ProductSize,
                Type = AvailabilityNotificationType.ReStockProduct,
                Message = Messages.BackInStockNotification,
                IsSent = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<AvailabilityNotification>().AddAsync(notification);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.BackInStockNotification
            };
        }

        public async Task<OperationResultModel> AddProductToCartAsync(AddCartItemDto dto)
        {
            if (dto.Quantity < 1)
                throw new BadRequestException(Messages.QuantityMustBeGreaterThanZero);

            if (dto.IsCustomPrint && string.IsNullOrWhiteSpace(dto.CustomPrintName) && dto.CustomPrintNumber is null)
                throw new BadRequestException(Messages.CartItem_CustomPrintDataRequired);

            var productSpec = new ProductWithTranslationsSpecification(x => x.Id == dto.ProductId && x.Visibility == Visibility.Active);
            productSpec.AddInclude(x => x.Stocks);
            var product = await _unitOfWork.Repository<Product>().GetFirstOrDefaultAsync(productSpec);
            if (product is null)
                throw new BadRequestException(string.Format(Messages.NotFound, nameof(Product)));

            if (product.IsOutOfStock)
                throw new BadRequestException(Messages.CartItem_IsOutOfStock);

            var stock = product.Stocks.FirstOrDefault(x => x.Size == dto.Size && x.StockType == dto.Gender);
            if (stock is null || stock.StockQuantity < dto.Quantity)
                throw new BadRequestException(Messages.CartItem_NotEnoughStock);

            var cartSpec = new BaseSpecification<Cart>(x => x.UserId == _currentUser.UserId);
            var cart = await _unitOfWork.Repository<Cart>().GetFirstOrDefaultAsync(cartSpec);
            if (cart is null)
            {
                cart = new Cart
                {
                    UserId = _currentUser.UserId,
                    CreatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.Repository<Cart>().AddAsync(cart);
                var result = await _unitOfWork.CompleteAsync();
                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);
                cart = await _unitOfWork.Repository<Cart>().GetFirstOrDefaultAsync(cartSpec);
            }

            var cartItemSpec = new BaseSpecification<CartItem>(
                x => x.CartId == cart!.Id
                    && x.ProductId == dto.ProductId
                    && x.Size == dto.Size
                    && x.Gender == dto.Gender
                    && x.ProductPrintId == dto.ProductPrintId
                    && x.CustomPrintName == dto.CustomPrintName
                    && x.CustomPrintNumber == dto.CustomPrintNumber
            );
            if (await _unitOfWork.Repository<CartItem>().AnyAsync(cartItemSpec))
                throw new BadRequestException(Messages.CartItem_IsAlreadyExit);

            var cartItem = _mapper.Map<CartItem>(dto);
            cartItem.CartId = cart!.Id;
            
            await _unitOfWork.Repository<CartItem>().AddAsync(cartItem);
            var result02 = await _unitOfWork.CompleteAsync();
            if (result02 < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.AddedSuccessfully
            };
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, UpdateCartItemDto dto)
        {
            if (dto.Quantity < 1)
                throw new BadRequestException(Messages.QuantityMustBeGreaterThanZero);
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(cartItemId);
            if (cartItem is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(CartItem)));

            var cartSpec = new BaseSpecification<Cart>(x => x.Id == cartItem.CartId && x.UserId == _currentUser.UserId);
            var cart = await _unitOfWork.Repository<Cart>().GetFirstOrDefaultAsync(cartSpec);
            if (cart is null)
                throw new BadRequestException(string.Format(Messages.NotFound, nameof(Cart)));

            var productSpec = new ProductWithTranslationsSpecification(x => x.Id == cartItem.ProductId);
            productSpec.AddInclude(x => x.Stocks);
            var product = await _unitOfWork.Repository<Product>().GetFirstOrDefaultAsync(productSpec);

            if (product is null)
                throw new BadRequestException(string.Format(Messages.NotFound, nameof(Product)));

            if (product.IsOutOfStock)
                throw new BadRequestException(Messages.CartItem_IsOutOfStock);

            var stock = product.Stocks.FirstOrDefault(x => x.Size == cartItem.Size && x.StockType == cartItem.Gender);

            if (stock is null || stock.StockQuantity < dto.Quantity)
                throw new BadRequestException(Messages.CartItem_NotEnoughStock);

            cartItem.Quantity = dto.Quantity;
            _unitOfWork.Repository<CartItem>().Update(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Cart>().Update(cart);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);
            return;
        }

        public async Task RemoveProductFromCartAsync(int cartItemId)
        {
            var cartItemSpec = new BaseSpecification<CartItem>(x => x.Id == cartItemId);
            cartItemSpec.AddInclude(x => x.Cart);
            var cartItem = await _unitOfWork.Repository<CartItem>().GetFirstOrDefaultAsync(cartItemSpec);
            if (cartItem is null)
                return;
            if (cartItem.Cart is null || cartItem.Cart.UserId != _currentUser.UserId)
                throw new BadRequestException(Messages.CartItem_NotBelongToUser);

            var cart = cartItem.Cart;
            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Cart>().Update(cart);
            _unitOfWork.Repository<CartItem>().Delete(cartItem);
            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);
            return;
        }


        #region Helper Methods
        private async Task CheckSKU(string sku)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var checkSUK = await _productRepo.GetFirstOrDefaultAsync(new ProductWithTranslationsSpecification(x => x.SKU == sku));
            if (checkSUK is not null)
            {
                var productName = checkSUK.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Name ?? string.Empty;
                throw new BadRequestException(string.Format(Messages.DuplicateField, nameof(Product.SKU), productName));
            }
        }
        private async Task CheckSlug(string slug)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var checkSlug = await _productRepo.GetFirstOrDefaultAsync(new ProductWithTranslationsSpecification(x => x.Slug == slug));
            if (checkSlug is not null)
            {
                var productName = checkSlug.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Name ?? string.Empty;
                throw new BadRequestException(string.Format(Messages.DuplicateField, nameof(Product.Slug), productName));
            }
        }
        private async Task CheckNewSKU(int productId, string sku)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var checkSUK = await _productRepo.GetFirstOrDefaultAsync(new ProductWithTranslationsSpecification(x => x.SKU == sku && x.Id != productId));
            if (checkSUK is not null)
            {
                var productName = checkSUK.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Name ?? string.Empty;
                throw new BadRequestException(string.Format(Messages.DuplicateField, nameof(Product.SKU), productName));
            }
        }
        private async Task CheckNewSlug(int productId, string slug)
        {
            var _productRepo = _unitOfWork.Repository<Product>();

            var checkSlug = await _productRepo.GetFirstOrDefaultAsync(new ProductWithTranslationsSpecification(x => x.Slug == slug && x.Id != productId));
            if (checkSlug is not null)
            {
                var productName = checkSlug.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Name ?? string.Empty;
                throw new BadRequestException(string.Format(Messages.DuplicateField, nameof(Product.Slug), productName));
            }
        }
        private void ValidateTranslations(ICollection<AddProductTranslationDto> translations)
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
        private void ValidateStocks(ICollection<AddProductStockDto> stocks)
        {
            var duplicatedStock = stocks.GroupBy(x => new { x.Size, x.StockType }).FirstOrDefault(x => x.Count() > 1);

            if (duplicatedStock is not null)
                throw new BadRequestException(string.Format(Messages.DuplicateStocks, duplicatedStock.Key.Size.ToString(), duplicatedStock.Key.StockType.ToString()));
        }
        private ICollection<AddProductImageDto> ValidateImages(ICollection<AddProductImageDto> images)
        {
            ICollection<AddProductImageDto> orderedImagesList = new List<AddProductImageDto>();
            foreach (var genderGroup in images.GroupBy(x => x.Gender))
            {
                var mainImages = genderGroup.Where(x => x.IsMain).ToList();
                if (mainImages.Count > 1)
                    throw new BadRequestException(Messages.MultipleMainImages);
                var mainImage = mainImages.FirstOrDefault();
                if (mainImage is null)
                    throw new BadRequestException(string.Format(Messages.MainImageRequired, genderGroup.Key));

                if (mainImage is not null)
                {
                    if (genderGroup.Any(x => !x.IsMain && x.DisplayOrder == 1))
                        throw new BadRequestException(Messages.NonMainImageCannotBeFirst);

                    mainImage.DisplayOrder = 1;
                    orderedImagesList.Add(mainImage);
                }

                var orderedImages = genderGroup.Where(x => !x.IsMain).OrderBy(x => x.DisplayOrder).ToList();
                for (var i = 0; i < orderedImages.Count; i++)
                {
                    orderedImages[i].DisplayOrder = i + 2;
                    orderedImagesList.Add(orderedImages[i]);
                }
            }
            return orderedImagesList;
        }
        private ICollection<ProductImage> ValidateProductImages(ICollection<ProductImage> images)
        {
            ICollection<ProductImage> orderedImagesList = new List<ProductImage>();
            foreach (var genderGroup in images.GroupBy(x => x.Gender))
            {

                var mainImage = genderGroup.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                if (mainImage is not null)
                {
                    mainImage.DisplayOrder = 1;
                    mainImage.IsMain = true;
                    orderedImagesList.Add(mainImage);
                }
              
                var orderedImages = genderGroup.Where(x => x != mainImage).OrderBy(x => x.DisplayOrder).ToList();
                for (var i = 0; i < orderedImages.Count; i++)
                {
                    orderedImages[i].DisplayOrder = i + 2;
                    orderedImagesList.Add(orderedImages[i]);
                }
            }
            return orderedImagesList;
        }
        private ICollection<ProductImage> HandelProductMainImage(ICollection<ProductImage> images)
        {
            ICollection<ProductImage> ImagesList = new List<ProductImage>();
            foreach (var genderGroup in images.GroupBy(x => x.Gender))
            {
                var mainImages = genderGroup.Where(x => x.IsMain).ToList();

                if (mainImages.Count > 1)
                    throw new BadRequestException(Messages.MultipleMainImages);
                var mainImage = mainImages.FirstOrDefault();
                if (mainImage is null)
                    mainImage = genderGroup.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                foreach (var image in genderGroup)
                {
                    image.IsMain = false;
                    image.DisplayOrder++;
                    ImagesList.Add(image);
                }

                mainImage.DisplayOrder = 1;
                ImagesList.Add(mainImage);
            }
            return ImagesList;
        }
        private async Task<List<string>> GetEntityIdsAsync<T>(Expression<Func<T, bool>> criteria) where T : ModelBase
        {
            var items = await _unitOfWork.Repository<T>().ListAsync(new BaseSpecification<T>(criteria));
            return items.Select(x => x.Id.ToString()).ToList();
        }
        private async Task SendBackInStockEmailAsync(string email, string productName, string imageUrl, int productId)
        {
            var productUrl = $"{_configuration["FrontendUrl"]}/active-product/{productId}";
            var emailBody = BackInStockTemplate.Generate(productName, imageUrl, productUrl);

            await _emailService.SendEmailAsync(
                email,
                "Back in Stock - MK Galácticos",
                emailBody);
        }

        #endregion
    }
}
