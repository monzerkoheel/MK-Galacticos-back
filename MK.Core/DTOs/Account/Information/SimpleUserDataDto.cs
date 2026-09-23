using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class SimpleUserDataDto
    {
        public string UserName { get; set; } = null!;
        public string UserRole { get; set; } = null!;
        public string? UserImage { get; set; } = null!;

    }
}
