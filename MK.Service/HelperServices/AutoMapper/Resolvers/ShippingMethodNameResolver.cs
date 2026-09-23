using AutoMapper;
using MK.Core.DTOs.ShippingMethods;
using MK.Core.HelperModels;
using MK.Core.Models;
using System.Globalization;
using System.Linq;

namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class ShippingMethodNameResolver : IValueResolver<ShippingMethod, ShippingMethodDto, string>
    {
        public string Resolve(ShippingMethod source, ShippingMethodDto destination, string destMember, ResolutionContext context)
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "ar" => source.Translations.FirstOrDefault(x => x.Language == Language.Arabic)?.Name ?? string.Empty,

                "es" => source.Translations.FirstOrDefault(x => x.Language == Language.Spanish)?.Name ?? string.Empty,

                _ => source.Translations.FirstOrDefault(x => x.Language == Language.English)?.Name ?? string.Empty
            };
        }
    }
}
