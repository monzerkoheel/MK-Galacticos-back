using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Collections
{
    public class CollectionDetailsDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public Visibility Status { get; set; }
        public string Slug { get; set; } = null!;
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProductsCount { get; set; }

        public ICollection<CollectionTranslationDto> Translations { get; set; } = new List<CollectionTranslationDto>();
        
    }
}
