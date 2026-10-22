using AutoMapper;
using MK.Core.DTOs;
using MK.Core.DTOs.Account.Information;
using MK.Core.DTOs.Collections;
using MK.Core.DTOs.Coupons;
using MK.Core.DTOs.FanRequests;
using MK.Core.DTOs.LegendCards;
using MK.Core.DTOs.Orders;
using MK.Core.DTOs.PaymentMethods;
using MK.Core.DTOs.Products;
using MK.Core.DTOs.ShippingMethods;
using MK.Core.HelperModels;
using MK.Core.Models;
using MK.Service.HelperServices.AutoMapper.MappingActions;
using MK.Service.HelperServices.AutoMapper.Resolvers;

namespace MK.Service.HelperServices
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Country, CountryDto>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom<CountryNameResolver>());

            CreateMap<Governorate, GovernorateDto>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom<GovernorateNameResolver>());
            CreateMap<CreateUserAddressDto, UserAddress>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<UserAddress, UserAddressDto>();

            CreateMap<EditUserAddressDto, UserAddress>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ApplicationUser, UserProfileDto>()
                .ForMember(
                    dest => dest.ProfileImageUrl,
                    opt => opt.MapFrom<ImageUrlResolver<ApplicationUser, UserProfileDto>, string?>(src => src.ProfileImageUrl))
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OrdersCount,
                    opt => opt.MapFrom(src => src.Orders.Count))
                .ForMember(dest => dest.WishlistItemsCount,
                    opt => opt.MapFrom(src => src.Wishlist != null
                        ? src.Wishlist.Items.Count
                        : 0))
                .ForMember(dest => dest.FanRequestsCount,
                    opt => opt.MapFrom(src => src.FanRequests.Count));

            CreateMap<AddCouponDto, Coupon>();

            CreateMap<AddCouponTranslationDto, CouponTranslation>()
                .ForMember(dest => dest.CouponId, opt => opt.Ignore())
                .ForMember(dest => dest.Coupon, opt => opt.Ignore());

            CreateMap<EditCouponDto, Coupon>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Translations, opt => opt.Ignore())
                .ForMember(dest => dest.UserCoupons, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
                

            CreateMap<EditCouponTranslationDto, CouponTranslation>()
                .ForMember(dest => dest.CouponId, opt => opt.Ignore())
                .ForMember(dest => dest.Coupon, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Coupon, CouponDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<CouponNameResolver>())
                .ForMember( dest => dest.Description, opt => opt.MapFrom<CouponDescriptionResolver>());

            CreateMap<Coupon, CouponDetailsDto>();

            CreateMap<CouponTranslation, CouponTranslationDto>();

            CreateMap<AuditLog, CouponAuditDto>();

            CreateMap<UserCoupon, UserCouponDto>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom<UserCouponNameResolver>())
                .ForMember(
                    dest => dest.Description,
                    opt => opt.MapFrom<UserCouponDescriptionResolver>())
                .ForMember(
                    dest => dest.CouponType,
                    opt => opt.MapFrom(src => src.Coupon.CouponType))
                .ForMember(
                    dest => dest.DiscountAmount,
                    opt => opt.MapFrom(src => src.Coupon.DiscountAmount))
                .ForMember(
                    dest => dest.DiscountPercentage,
                    opt => opt.MapFrom(src => src.Coupon.DiscountPercentage))
                .ForMember(
                    dest => dest.MinimumOrderAmount,
                    opt => opt.MapFrom(src => src.Coupon.MinimumOrderAmount))
                .ForMember(
                    dest => dest.MaximumDiscountAmount,
                    opt => opt.MapFrom(src => src.Coupon.MaximumDiscountAmount));

            CreateMap<CreateCollectionDto, Collection>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            CreateMap<CollectionTranslationDto, CollectionTranslation>().ReverseMap();

            CreateMap<Collection, ActiveCollectionDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<Collection, ActiveCollectionDto>, string?>(src => src.ImageUrl))
                .ForMember(x => x.Name, opt => opt.Ignore())
                .ForMember(x => x.Description, opt => opt.Ignore())
                .AfterMap<CollectionLocalizationAction>();

            CreateMap<Collection, CollectionDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<Collection, CollectionDto>, string?>(src => src.ImageUrl))
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.ProductsCount, 
                           opt => opt.MapFrom(src => src.ProductCollections.Count))
                .AfterMap<CollectionDtoLocalizationAction>();

            CreateMap<Collection, SimpleCollectionDto>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Translations.FirstOrDefault(x => x.Language == Language.English)!.Name));

            CreateMap<Collection, CollectionDetailsDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<Collection, CollectionDetailsDto>, string?>(src => src.ImageUrl))
                .ForMember(dest => dest.ProductsCount,
                           opt => opt.MapFrom(src => src.ProductCollections.Count));

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                .ForMember(dest => dest.RatingCount, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore());


            CreateMap<AddProductTranslationDto, ProductTranslation>();

            CreateMap<AddProductStockDto, ProductStock>();

            CreateMap<AddProductPrintDto, ProductPrint>();

            CreateMap<AddProductCollectionDto, ProductCollection>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Collection, opt => opt.Ignore());

            CreateMap<ProductImageDto, ProductImage>();

            CreateMap<EditProductDto, Product>()
                .ForMember(dest => dest.SalePrice, opt => opt.Ignore())
                .ForMember(dest => dest.ProductType, opt => opt.PreCondition(src => src.ProductType.HasValue))
                .ForMember(dest => dest.AvailableFor, opt => opt.PreCondition(src => src.AvailableFor.HasValue))
                .ForMember(dest => dest.Visibility, opt => opt.PreCondition(src => src.Visibility.HasValue))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EditProductTranslationDto, ProductTranslation>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EditProductImageDto, ProductImage>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());



            CreateMap<Product, ProductsDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(
                           src => src.Translations.FirstOrDefault(x => x.Language == Language.English)!.Name))
                .ForMember(dest => dest.Collections,
                           opt => opt.MapFrom(
                                  src => src.ProductCollections.Select(x => x.Collection.Translations
                                                                    .FirstOrDefault(t => t.Language == Language.English)!.Name)
                                                                .ToList()))
                .ForMember(dest => dest.PrintType,
                           opt => opt.MapFrom(
                                  src => src.IsCustomPrintAvailable ? ProductPrintType.Custom : src.Prints.Any()
                                                                                                    ? ProductPrintType.Players
                                                                                                    : ProductPrintType.None))
                .ForMember(dest => dest.Prints,
                           opt => opt.MapFrom(src => src.Prints.Select(x => $"{x.Name} {x.Number}").ToList()))
                .ForMember(dest => dest.Stock,
                           opt => opt.MapFrom(src => src.Stocks.Sum(x => x.StockQuantity)))
                .ForMember(dest => dest.MainImageUrl,
                           opt => opt.MapFrom<ProductMainImageUrlResolver>())
                .ForMember(dest => dest.Gender,
                           opt => opt.MapFrom(src => src.AvailableFor))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Visibility));

            CreateMap<Product, ActiveProductsDto>()
                .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.CollectionName, opt => opt.Ignore())
                .ForMember(dest => dest.AvailableFor, opt => opt.Ignore())
                .AfterMap<ActiveProductsDtoLocalizationAction>();

            CreateMap<ProductImage, ActiveProductImageDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<ProductImage, ActiveProductImageDto>, string?>(src => src.ImageUrl));
            CreateMap<ProductPrint, ProductPrintDto>();
            CreateMap<ProductStock, ProductStockDto>();


            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.MaleImages,
                           opt => opt.MapFrom(src => src.Images
                                                        .Where(x => x.Gender == ProductGender.Men)
                                                        .OrderBy(x => x.DisplayOrder)))
                .ForMember(dest => dest.FemaleImages,
                           opt => opt.MapFrom(src => src.Images
                                                        .Where(x => x.Gender == ProductGender.Women)
                                                        .OrderBy(x => x.DisplayOrder)))
                .ForMember(dest => dest.ProductCollections, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Ratings, opt => opt.Ignore())
                .ForMember(dest => dest.WishlistItemsCount, opt => opt.Ignore())
                .ForMember(dest => dest.CartItemsCount, opt => opt.Ignore())
                .ForMember(dest => dest.WaitingReStockNotificationsCount, opt => opt.Ignore());

            CreateMap<ProductTranslation, ProductTranslationDto>();

            CreateMap<ProductStock, ManagmentProductStockDto>();

            CreateMap<ProductImage, ManagmentProductImageDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<ProductImage, ManagmentProductImageDto>, string?>(src => src.ImageUrl));

            CreateMap<Collection, ProductCollectionDto>()
                .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name,
                           opt => opt.MapFrom(src => src.Translations.FirstOrDefault(x => x.Language == Language.English)!.Name));

            CreateMap<OrderItem, ProductOrdersDto>()
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order.OrderNumber))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Order.Status))
                .ForMember(dest => dest.LastUpdateDate, opt => opt.MapFrom(src => src.Order.UpdatedAt));

            CreateMap<ProductRating, ManagmentProductRatingDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<ProductRating, ManagmentProductRatingDto>, string?>(src => src.User.ProfileImageUrl));

            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserFullName,
                           opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.UserRole,
                           opt => opt.MapFrom<AuditLogUserRoleResolver>());

            CreateMap<AddCartItemDto,CartItem>()
                .ForMember(dest => dest.CartId, opt => opt.Ignore());

            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductMainImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ProductPrice,
                    opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductSalePrice,
                    opt => opt.MapFrom(src => src.Product.SalePrice))
                .ForMember(dest => dest.PrintType, opt => opt.Ignore())
                .AfterMap<CartItemMappingAction>();

            CreateMap<CreateLegendCardDto, LegendCard>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateLegendCardDto, LegendCard>()
                .ForMember(dest => dest.Name,
                    opt => opt.Condition(src => src.Name is not null))
                .ForMember(dest => dest.Price,
                    opt => opt.Condition(src => src.Price.HasValue))
                .ForMember(dest => dest.Status,
                    opt => opt.Condition(src => src.Status.HasValue))
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.Ignore());

            CreateMap<LegendCard, LegendCardDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<LegendCard, LegendCardDto>, string?>(src => src.ImageUrl));

            CreateMap<CreateShippingMethodDto, ShippingMethod>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<CreateShippingMethodTranslationDto, ShippingMethodTranslation>()
                .ForMember(dest => dest.ShippingMethodId, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingMethod, opt => opt.Ignore());

            CreateMap<UpdateShippingMethodDto, ShippingMethod>()
                .ForMember(dest => dest.Price, opt => opt.Condition(src => src.Price.HasValue))
                .ForMember(dest => dest.EstimatedDeliveryDays, opt => opt.Condition(src => src.EstimatedDeliveryDays.HasValue))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status.HasValue))
                .ForMember(dest => dest.Translations, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateShippingMethodTranslationDto, ShippingMethodTranslation>()
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name is not null))
                .ForMember(dest => dest.ShippingMethodId, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingMethod, opt => opt.Ignore());

            CreateMap<ShippingMethod, ShippingMethodDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<ShippingMethodNameResolver>());

            CreateMap<ShippingMethod, ShippingMethodDetailsDto>();

            CreateMap<ShippingMethodTranslation, ShippingMethodTranslationDto>();

            CreateMap<PaymentMethod, PaymentMethodDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<PaymentMethodNameResolver>())
                .ForMember(dest => dest.IconImageUrl, 
                            opt => opt.MapFrom<ImageUrlResolver<PaymentMethod, PaymentMethodDto>, string?>(src => src.IconImageUrl));

            CreateMap<PaymentMethod, PaymentMethodDetailsDto>()
                .ForMember(dest => dest.IconImageUrl,
                            opt => opt.MapFrom<ImageUrlResolver<PaymentMethod, PaymentMethodDetailsDto>, string?>(src => src.IconImageUrl));
            CreateMap<PaymentMethodTranslation, PaymentMethodTranslationDto>();

            CreateMap<ProductTranslation, OrderItemTranslation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItemId, opt => opt.Ignore())
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name));

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.CustomerEmail,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.ItemsCount,
                    opt => opt.MapFrom(src => src.Items.Count));

            CreateMap<FanRequest, FanRequestDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<FanRequest, FanRequestDto>, string?>(src => src.ImageUrl))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<FanRequest, FanRequestDto>, string?>(src => src.User.ProfileImageUrl))
                .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore());

            CreateMap<FanRequest, MyFanRequestDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<FanRequest, MyFanRequestDto>, string?>(src => src.ImageUrl));

            CreateMap<FanRequest, ManagementFanRequestDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<FanRequest, ManagementFanRequestDto>, string?>(src => src.ImageUrl))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<FanRequest, ManagementFanRequestDto>, string?>(src => src.User.ProfileImageUrl));

            CreateMap<FanRequest, ManagementFanRequestDetailsDto>()
                .ForMember(dest => dest.ImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<FanRequest, ManagementFanRequestDetailsDto>, string?>(src => src.ImageUrl))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserImageUrl,
                           opt => opt.MapFrom<ImageUrlResolver<FanRequest, ManagementFanRequestDetailsDto>, string?>(src => src.User.ProfileImageUrl));
        }
    }
}
