using AutoMapper;
using MK.Core.DTOs.Account.Information;
using MK.Core.Models;
using System.Globalization;


namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class CountryNameResolver : IValueResolver<Country, CountryDto, string>
    {
        public string Resolve(Country source, CountryDto destination, string destMember, ResolutionContext context)
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
