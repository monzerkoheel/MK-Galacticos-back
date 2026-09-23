using Microsoft.AspNetCore.Http;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Collections
{
    public class CreateCollectionDto
    {
        [Required]
        public IFormFile Image { get; set; } = null!;

        public Visibility Status = Visibility.Inactive;
        [Required]
        public int DisplayOrder { get; set; }
        [Required]
        public string Slug { get; set; } = null!;

        public List<CollectionTranslationDto> Translations { get; set; } = new List<CollectionTranslationDto>();
    }
}
