using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class Wishlist : ModelBase
    {
        public string UserId { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
