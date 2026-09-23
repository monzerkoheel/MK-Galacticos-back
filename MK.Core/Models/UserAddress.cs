using MK.Core.HelperModels;

namespace MK.Core.Models
{
    public class UserAddress : ModelBase
    {
        public string UserId { get; set; } = null!;

        public string AddressName { get; set; }

        public string Country { get; set; }

        public string Governorate { get; set; }

        public string Area { get; set; }

        public string RecipientName { get; set; }

        public string DetailedAddress { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string? Notes { get; set; }


        public ApplicationUser User { get; set; } = null!;

    }
}
