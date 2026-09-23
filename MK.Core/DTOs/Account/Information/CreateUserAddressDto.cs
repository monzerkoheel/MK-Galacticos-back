using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class CreateUserAddressDto
    {
        [Required]
        public string AddressName { get; set; }

        [Required]
        public string Country { get; set; }
        [Required]
        public string Governorate { get; set; }
        [Required]
        public string Area { get; set; }
        [Required]
        public string DetailedAddress { get; set; } = null!;

        [Required]
        public string RecipientName { get; set; }
        [Required]
        public string PhoneNumber { get; set; } = null!;

        public string? Notes { get; set; }
    }
}
