using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.HelperModels.Exceptions
{
    public class PaymentIntentCreationFailedException : Exception
    {
        public int OrderId { get; }
        public string OrderNumber { get; }

        public PaymentIntentCreationFailedException(int orderId, string orderNumber, string message) : base(message)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
        }
    }
}
