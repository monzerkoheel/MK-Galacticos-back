using AutoMapper;
using MK.Core.DTOs.Account.Information;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class GovernorateNameResolver : IValueResolver<Governorate, GovernorateDto, string>
    {
        public string Resolve(Governorate source, GovernorateDto destination, string destMember, ResolutionContext context)
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "ar" => source.NameAr,
                "es" => source.NameEs,
                _ => source.NameEn
            };
        }
    }
}
