using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Orders
{
    public class CreateOrderDto
    {
        public bool HasCoupon { get; set; }
        public string? CouponCode { get; set; }

        public bool HasLegendCard { get; set; }
        public int? LegendCardId { get; set; }


        [Range(1, int.MaxValue)]
        public int ShippingMethodId { get; set; }


        public string? RecipientName { get; set; }

        [Required]
        public string Country { get; set; } = null!;

        [Required]
        public string Governorate { get; set; } = null!;

        [Required]
        public string Area { get; set; } = null!;

        [Required]
        public string DetailedAddress { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;
        public string? Notes { get; set; }



        [Range(1, int.MaxValue)]
        public int PaymentMethodId { get; set; }
    }
}
