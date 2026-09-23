using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class Collection : ModelBase
    {
        public string ImageUrl { get; set; }
        public Visibility Status { get; set; }
        public string Slug { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<CollectionTranslation> Translations { get; set; } = new List<CollectionTranslation>();
        public ICollection<ProductCollection> ProductCollections { get; set; } = new List<ProductCollection>();
    }
}
