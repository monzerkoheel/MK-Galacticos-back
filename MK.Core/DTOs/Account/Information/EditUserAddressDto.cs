using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class EditUserAddressDto
    {
        [Required]
        public int Id { get; set; }

        public string? AddressName { get; set; }
        public string? Country { get; set; }
        public string? Governorate { get; set; }
        public string? Area { get; set; }
        public string? DetailedAddress { get; set; }
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
    }
}
