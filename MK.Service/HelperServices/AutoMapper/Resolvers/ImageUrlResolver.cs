using AutoMapper;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Information;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class ImageUrlResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, string?, string?>
    {
        private readonly IFileService _fileService;

        public ImageUrlResolver(IFileService fileService)
        {
            _fileService = fileService;
        }

        public string? Resolve(TSource source, TDestination destination, string? sourceMember, string? destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(sourceMember))
                return null;

            return _fileService.GetFullUrl(sourceMember);
        }
    }
}
