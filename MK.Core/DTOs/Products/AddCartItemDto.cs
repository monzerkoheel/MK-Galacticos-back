using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class AddCartItemDto
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public ProductGender Gender { get; set; }
        [Required]
        public ProductSize Size { get; set; }
        public bool IsCustomPrint { get; set; } = false;
        public int? ProductPrintId { get; set; }
        public string? CustomPrintName { get; set; }
        public int? CustomPrintNumber { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
