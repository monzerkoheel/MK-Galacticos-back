using AutoMapper;
using MK.Core.DTOs.Collections;
using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.MappingActions
{
    public class CollectionDtoLocalizationAction : IMappingAction<Collection, CollectionDto>
    {
        public void Process(Collection source, CollectionDto destination, ResolutionContext context)
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "ar" => Language.Arabic,
                "es" => Language.Spanish,
                _ => Language.English
            };

            var translation = source.Translations.FirstOrDefault(x => x.Language == language);

            destination.Name = translation?.Name ?? string.Empty;        
        }
    }
}
