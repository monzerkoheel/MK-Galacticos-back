using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Localization
{
    public interface ICurrentLanguage
    {
        string Code { get; }
        Language Language { get; }
    }
}
