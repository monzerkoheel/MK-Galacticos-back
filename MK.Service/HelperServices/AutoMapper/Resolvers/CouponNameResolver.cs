using AutoMapper;
using MK.Core.DTOs.Coupons;
using MK.Core.HelperModels;
using MK.Core.Models;
using System.Globalization;


namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class CouponNameResolver : IValueResolver<Coupon, CouponDto, string>
    {
        public string Resolve(Coupon source,CouponDto destination,string destMember,ResolutionContext context)
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
