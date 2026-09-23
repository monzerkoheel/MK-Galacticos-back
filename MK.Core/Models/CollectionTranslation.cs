using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class CollectionTranslation : ModelBase
    {
        public int CollectionId { get; set; }

        public Language Language { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public Collection Collection { get; set; } = null!;
    }
}
