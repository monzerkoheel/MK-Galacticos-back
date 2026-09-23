using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class EditUserInformationsDto
    {
        public string? FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }
    }
}
