using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MK.Core.HelperModels;

namespace MK.Repository.Data.Configurations
{
    public class LanguageConverter : ValueConverter<Language, string>
    {
        public LanguageConverter()
            : base(
                language => language == Language.English ? "en" :
                            language == Language.Arabic ? "ar" :
                            "es",

                value => value == "en" ? Language.English :
                         value == "ar" ? Language.Arabic :
                         Language.Spanish)
        {
        }
    }
}
