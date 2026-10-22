using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Orders
{
    public class ChangeOrderStateDto
    {
        public OrderStatus NewStatus { get; set; }
    }
}
