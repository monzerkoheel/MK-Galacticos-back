using AutoMapper;
using MK.Core.DTOs.Coupons;
using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class CouponDescriptionResolver: IValueResolver<Coupon, CouponDto, string>
    {
        public string Resolve(Coupon source,CouponDto destination,string destMember,ResolutionContext context)
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "ar" => source.Translations.FirstOrDefault(x => x.Language == Language.Arabic)?.Description ?? string.Empty,

                "es" => source.Translations.FirstOrDefault(x => x.Language == Language.Spanish)?.Description ?? string.Empty,

                _ => source.Translations.FirstOrDefault(x => x.Language == Language.English)?.Description ?? string.Empty
            };
        }
    }
}
