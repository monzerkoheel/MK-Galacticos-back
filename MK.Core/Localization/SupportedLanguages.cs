using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Localization
{
    public static class SupportedLanguages
    {
        public const string English = "en";
        public const string Arabic = "ar";
        public const string Spanish = "es";

        public static readonly string[] All = [ English, Arabic, Spanish ];

        public const string Default = English;
    }
}
