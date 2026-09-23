using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class EditProductCollectionsDto
    {
        public ICollection<int> DeletedCollectionsIds { get; set; } = new List<int>();
        public ICollection<int> AddedCollectionsIds { get; set; } = new List<int>();
    }
}
