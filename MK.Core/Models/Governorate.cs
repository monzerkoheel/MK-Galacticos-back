using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class Governorate : ModelBase
    {

        public int CountryId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string NameEs { get; set; } = null!;
        public Country Country { get; set; } = null!;

        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    }
}
