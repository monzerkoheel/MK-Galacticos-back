using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Coupons
{
    public class EditCouponTranslationDto
    {
        public Language Language { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
