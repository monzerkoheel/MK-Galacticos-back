using Microsoft.AspNetCore.Http;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Collections
{
    public class EditCollectionDto
    {
        public IFormFile? Image { get; set; }
        public string? Slug { get; set; }
        public Visibility? Status { get; set; }
        public int? DisplayOrder { get; set; }
        public List<EditCollectionTranslationDto>? Translations { get; set; }
    }
}
