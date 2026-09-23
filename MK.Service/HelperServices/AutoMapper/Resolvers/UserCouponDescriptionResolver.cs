using AutoMapper;
using MK.Core.DTOs.Account.Information;
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
    public class UserCouponDescriptionResolver : IValueResolver<UserCoupon, UserCouponDto, string>
    {
        public string Resolve(UserCoupon source, UserCouponDto destination, string destMember, ResolutionContext context)
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "ar" => source.Coupon.Translations.FirstOrDefault(x => x.Language == Language.Arabic)?.Description ?? string.Empty,

                "es" => source.Coupon.Translations.FirstOrDefault(x => x.Language == Language.Spanish)?.Description ?? string.Empty,

                _ => source.Coupon.Translations.FirstOrDefault(x => x.Language == Language.English)?.Description ?? string.Empty
            };
        }
    }
}
