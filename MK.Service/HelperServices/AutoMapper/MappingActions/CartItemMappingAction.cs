using AutoMapper;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Products;
using MK.Core.Localization;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.MappingActions
{
    public class CartItemMappingAction : IMappingAction<CartItem, CartItemDto>
    {
        private readonly ICurrentLanguage _currentLanguage;
        private readonly IFileService _fileService;

        public CartItemMappingAction(ICurrentLanguage currentLanguage, IFileService fileService)
        {
            _currentLanguage = currentLanguage;
            _fileService = fileService;
        }

        public void Process(CartItem source, CartItemDto destination, ResolutionContext context)
        {
            destination.ProductName = source.Product.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.Name ?? string.Empty;

            var imageUrl = source.Product.Images.FirstOrDefault(x => x.IsMain && x.Gender == source.Gender)?.ImageUrl;
            destination.ProductMainImageUrl = _fileService.GetFullUrl(imageUrl ?? string.Empty);
            destination.PrintType = GetPrintType(source);
        }

        private ProductPrintType GetPrintType(CartItem cartItem)
        {
            if (cartItem.IsCustomPrint)
                return ProductPrintType.Custom;

            if (cartItem.ProductPrintId.HasValue)
                return ProductPrintType.Players;

            return ProductPrintType.None;
        }
    }
}
