using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class WishlistItem : ModelBase
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }


        public Wishlist Wishlist { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
