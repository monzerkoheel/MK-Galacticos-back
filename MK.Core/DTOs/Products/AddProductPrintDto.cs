using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class AddProductPrintDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int Number { get; set; }
    }
}
