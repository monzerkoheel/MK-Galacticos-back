using AutoMapper;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.Localization;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.MappingActions
{
    public class ActiveProductsDtoLocalizationAction: IMappingAction<Product, ActiveProductsDto>
    {
        private readonly ICurrentLanguage _currentLanguage;
        private readonly IFileService _fileService;
        public ActiveProductsDtoLocalizationAction(ICurrentLanguage currentLanguage, IFileService fileService)
        {
            _currentLanguage = currentLanguage;
            _fileService = fileService;
        }
        public void Process(Product source, ActiveProductsDto destination, ResolutionContext context)
        {
            var language = _currentLanguage.Language;

            var translation = source.Translations.FirstOrDefault(x => x.Language == language);
            var collectionTranslation = source.ProductCollections.FirstOrDefault()?.Collection?.Translations.FirstOrDefault(x => x.Language == language);
            destination.Name = translation?.Name ?? string.Empty;
            destination.CollectionName = collectionTranslation?.Name ?? string.Empty;

            if (source.AvailableFor == ProductAvailability.Unisex)
                destination.AvailableFor = Enums.ProductAvailability_Unisex;
            else if (source.AvailableFor == ProductAvailability.Women)
                destination.AvailableFor = Enums.ProductAvailability_Women;
            else
                destination.AvailableFor = Enums.ProductAvailability_Men;

            var gender = source.AvailableFor == ProductAvailability.Women ? ProductGender.Women : ProductGender.Men;
            var imageUrl = source.Images.FirstOrDefault(x => x.Gender == gender && x.IsMain)?.ImageUrl;
            destination.MainImageUrl = _fileService.GetFullUrl(imageUrl ?? string.Empty);
        }
    }
}
