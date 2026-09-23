using AutoMapper;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Products;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class ProductMainImageUrlResolver : IValueResolver<Product, ProductsDto, string>
    {
        private readonly IFileService _fileService;

        public ProductMainImageUrlResolver(IFileService fileService)
        {
            _fileService = fileService;
        }

        public string Resolve(Product source, ProductsDto destination, string destMember, ResolutionContext context)
        {
            var gender = source.AvailableFor == ProductAvailability.Women ? ProductGender.Women : ProductGender.Men;
            var imageUrl = source.Images.FirstOrDefault(x => x.Gender == gender && x.IsMain)?.ImageUrl;
            return _fileService.GetFullUrl(imageUrl ?? string.Empty);
        }
    }
}
