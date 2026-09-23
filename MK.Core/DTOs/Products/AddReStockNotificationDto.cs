using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class AddReStockNotificationDto
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public ProductSize ProductSize { get; set; }
    }
}
