using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class UserAddressDto
    {
        public int Id { get; set; }

        public string AddressName { get; set; }
        public string Country { get; set; }
        public string Governorate { get; set; }
        public string Area { get; set; }
        public string DetailedAddress { get; set; } = null!;

        public string RecipientName { get; set; }
        public string PhoneNumber { get; set; } = null!;

        public string? Notes { get; set; }
    }
}
