using AutoMapper;
using MK.Core.DTOs.PaymentMethods;
using MK.Core.DTOs.ShippingMethods;
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
    public class PaymentMethodNameResolver : IValueResolver<PaymentMethod, PaymentMethodDto, string>
    {
        public string Resolve(PaymentMethod source, PaymentMethodDto destination, string destMember, ResolutionContext context)
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
