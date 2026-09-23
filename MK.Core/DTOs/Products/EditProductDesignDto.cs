using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class EditProductDesignDto
    {
        public string? Slug { get; set; }

        public bool? IsFeatured { get; set; }
        public bool? IsNewArrival { get; set; }

        public ICollection<EditProductTranslationDto> Translations { get; set; } = new List<EditProductTranslationDto>();

        public ICollection<EditProductImageDto> Images { get; set; } = new List<EditProductImageDto>();
        public ICollection<AddProductImageDto> NewImages { get; set; } = new List<AddProductImageDto>();
        public ICollection<int> DeletedImageIds { get; set; } = new List<int>();
    }
}
