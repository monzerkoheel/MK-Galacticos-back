using MK.Core.Models;
using MK.Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MK.Repository.Specifications.OrderSpecifications
{
    public class OrderWithAllDetailsSpecification : BaseSpecification<Order>
    {
        public OrderWithAllDetailsSpecification(Expression<Func<Order, bool>> criteria) : base(criteria)
        {
            EnableSplitQuery();
            AddInclude(x => x.User);

            AddInclude(x => x.Shipping);
            AddInclude("Shipping.ShippingMethod");

            AddInclude(x => x.Items);
            AddInclude("Items.Product");
            AddInclude("Items.Product.Images");
            AddInclude("Items.ProductPrint");
            AddInclude("Items.Translations");

            AddInclude(x => x.Payment);
            AddInclude("Payment.PaymentMethod");
            AddInclude("Payment.PaymentMethod.Translations");

            AddInclude(x => x.UserCoupon);

            AddInclude(x => x.LegendCard);
        }
    }
}
