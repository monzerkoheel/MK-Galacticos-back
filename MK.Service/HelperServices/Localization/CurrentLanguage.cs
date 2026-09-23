using MK.Core.HelperModels;
using MK.Core.Localization;
using System.Globalization;

namespace MK.Service.HelperServices.Localization
{
    public class CurrentLanguage : ICurrentLanguage
    {
        public string Code => CultureInfo.CurrentUICulture.Name;
        public Language Language =>
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
                {
                    "ar" => Language.Arabic,
                    "es" => Language.Spanish,
                    _ => Language.English
                };
    }
}
