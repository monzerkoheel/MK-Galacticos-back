using MK.Core.Contracts.ServicesContracts;

using MK.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace MK.Service.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly IFileService _fileService;
        public InvoiceService(HttpClient httpClient, IFileService fileService)
        {
            _httpClient = httpClient;
            _fileService = fileService;
        }

        public async Task<byte[]> GenerateInvoiceAsync(Order order, string languageCode)
        {
            var isArabic = languageCode.Equals("ar",StringComparison.OrdinalIgnoreCase);

            var texts = GetTexts(languageCode);

            // Load product images before generating the PDF
            var productImages = new Dictionary<int, byte[]?>();

            foreach (var item in order.Items)
            {
                if (string.IsNullOrWhiteSpace(item.ProductImageUrl))
                    continue;

                try
                {
                    var imageBytes = await _httpClient.GetByteArrayAsync(_fileService.GetFullUrl(item.ProductImageUrl));

                    productImages[item.Id] = imageBytes;
                }
                catch
                {
                    productImages[item.Id] = null;
                }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    // Arabic = RTL
                    if (isArabic)
                        page.ContentFromRightToLeft();
                    else
                        page.ContentFromLeftToRight();

                    page.DefaultTextStyle(style =>
                        style
                            .FontSize(10)
                            .FontFamily(
                                "Lato",
                                "Noto Sans Arabic"));

                    // ==========================================
                    // HEADER
                    // ==========================================

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("MK GALÁCTICOS")
                                .FontSize(26)
                                .Bold()
                                .FontColor("#D4AF37");

                            column.Item()
                                .Text(texts.BrandSubtitle)
                                .FontSize(10)
                                .FontColor("#777777");

                            column.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#D4AF37");
                        });

                    // ==========================================
                    // CONTENT
                    // ==========================================

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // ==================================
                            // INVOICE INFO
                            // ==================================

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(info =>
                                        {
                                            info.Item()
                                                .Text(texts.Invoice)
                                                .FontSize(22)
                                                .Bold();

                                            info.Item()
                                                .Text($"#{order.OrderNumber}")
                                                .FontSize(12);
                                        });

                                    row.RelativeItem()
                                        .AlignRight()
                                        .Column(info =>
                                        {
                                            info.Item()
                                                .Text(
                                                    $"{texts.Date}: " +
                                                    $"{order.CreatedAt:dd/MM/yyyy}");

                                            info.Item()
                                                .Text(
                                                    $"{texts.Payment}: " +
                                                    $"{GetPaymentStatus(
                                                        order.Payment.Status,
                                                        texts)}");
                                        });
                                });

                            // ==================================
                            // CUSTOMER + SHIPPING
                            // ==================================

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Background("#151515")
                                        .Padding(15)
                                        .Column(customer =>
                                        {
                                            customer.Item()
                                                .Text(texts.BillTo)
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor("#D4AF37");

                                            customer.Item()
                                                .PaddingTop(5)
                                                .Text(order.User.FullName)
                                                .FontColor("#FFFFFF");

                                            customer.Item()
                                                .Text(order.User.Email)
                                                .FontColor("#CCCCCC");
                                        });

                                    row.ConstantItem(15);

                                    row.RelativeItem()
                                        .Background("#151515")
                                        .Padding(15)
                                        .Column(shipping =>
                                        {
                                            shipping.Item()
                                                .Text(texts.ShippingTo)
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor("#D4AF37");

                                            shipping.Item()
                                                .PaddingTop(5)
                                                .Text(
                                                    order.Shipping.RecipientName
                                                    ?? order.User.FullName)
                                                .FontColor("#FFFFFF");

                                            shipping.Item()
                                                .Text(
                                                    $"{order.Shipping.Area}, " +
                                                    $"{order.Shipping.Governorate}, " +
                                                    $"{order.Shipping.Country}")
                                                .FontColor("#CCCCCC");

                                            shipping.Item()
                                                .Text(
                                                    order.Shipping.DetailedAddress)
                                                .FontColor("#CCCCCC");

                                            shipping.Item()
                                                .Text(
                                                    order.Shipping.PhoneNumber)
                                                .FontColor("#CCCCCC");
                                        });
                                });

                            // ==================================
                            // ORDER ITEMS
                            // ==================================

                            column.Item()
                                .Text(texts.OrderItems)
                                .FontSize(14)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        // IMAGE
                                        columns.ConstantColumn(70);

                                        // PRODUCT
                                        columns.RelativeColumn(4);

                                        // QUANTITY
                                        columns.ConstantColumn(50);

                                        // PRICE
                                        columns.ConstantColumn(80);

                                        // TOTAL
                                        columns.ConstantColumn(90);
                                    });

                                    // ------------------------------
                                    // TABLE HEADER
                                    // ------------------------------

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background("#151515")
                                            .Padding(8)
                                            .AlignMiddle()
                                            .Text(texts.Image)
                                            .FontColor("#D4AF37")
                                            .Bold();

                                        header.Cell()
                                            .Background("#151515")
                                            .Padding(8)
                                            .AlignMiddle()
                                            .Text(texts.Product)
                                            .FontColor("#D4AF37")
                                            .Bold();

                                        header.Cell()
                                            .Background("#151515")
                                            .Padding(8)
                                            .AlignMiddle()
                                            .Text(texts.Quantity)
                                            .FontColor("#D4AF37")
                                            .Bold();

                                        header.Cell()
                                            .Background("#151515")
                                            .Padding(8)
                                            .AlignMiddle()
                                            .Text(texts.Price)
                                            .FontColor("#D4AF37")
                                            .Bold();

                                        header.Cell()
                                            .Background("#151515")
                                            .Padding(8)
                                            .AlignMiddle()
                                            .Text(texts.Total)
                                            .FontColor("#D4AF37")
                                            .Bold();
                                    });

                                    // ------------------------------
                                    // PRODUCTS
                                    // ------------------------------

                                    foreach (var item in order.Items)
                                    {
                                        var productName =
                                            item.Translations
                                                .FirstOrDefault()
                                                ?.ProductName
                                            ?? texts.Product;

                                        // ------------------------------
                                        // IMAGE
                                        // ------------------------------

                                        if (productImages.TryGetValue(
                                                item.Id,
                                                out var imageBytes) &&
                                            imageBytes is not null)
                                        {
                                            table.Cell()
                                                .MinHeight(65)
                                                .Padding(5)
                                                .AlignMiddle()
                                                .AlignCenter()
                                                .Image(imageBytes)
                                                .FitArea();
                                        }
                                        else
                                        {
                                            table.Cell()
                                                .MinHeight(65)
                                                .Padding(5)
                                                .AlignMiddle()
                                                .AlignCenter()
                                                .Text("-");
                                        }

                                        // ------------------------------
                                        // PRODUCT NAME
                                        // ------------------------------

                                        table.Cell()
                                            .MinHeight(65)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .Text(productName);

                                        // ------------------------------
                                        // QUANTITY
                                        // ------------------------------

                                        table.Cell()
                                            .MinHeight(65)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .AlignCenter()
                                            .Text(item.Quantity.ToString());

                                        // ------------------------------
                                        // UNIT PRICE
                                        // ------------------------------

                                        table.Cell()
                                            .MinHeight(65)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .AlignRight()
                                            .Text(
                                                $"{item.UnitPrice:N2} EGP");

                                        // ------------------------------
                                        // TOTAL
                                        // ------------------------------

                                        table.Cell()
                                            .MinHeight(65)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .AlignRight()
                                            .Text(
                                                $"{item.TotalPrice:N2} EGP");
                                    }
                                });

                            // ==================================
                            // SUMMARY
                            // ==================================

                            column.Item()
                                .AlignRight()
                                .Width(250)
                                .Column(summary =>
                                {
                                    summary.Spacing(5);

                                    summary.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text(texts.Subtotal);

                                            row.ConstantItem(100)
                                                .AlignRight()
                                                .Text(
                                                    $"{order.Subtotal:N2} EGP");
                                        });

                                    summary.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text(texts.Shipping);

                                            row.ConstantItem(100)
                                                .AlignRight()
                                                .Text(
                                                    $"{order.ShippingCost:N2} EGP");
                                        });

                                    if (order.DiscountAmount.HasValue)
                                    {
                                        summary.Item()
                                            .Row(row =>
                                            {
                                                row.RelativeItem()
                                                    .Text(texts.Discount);

                                                row.ConstantItem(100)
                                                    .AlignRight()
                                                    .Text(
                                                        $"-{order.DiscountAmount.Value:N2} EGP");
                                            });
                                    }

                                    summary.Item()
                                        .PaddingTop(5)
                                        .LineHorizontal(1)
                                        .LineColor("#D4AF37");

                                    summary.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text(texts.GrandTotal)
                                                .Bold()
                                                .FontSize(14);

                                            row.ConstantItem(100)
                                                .AlignRight()
                                                .Text(
                                                    $"{order.TotalAmount:N2} EGP")
                                                .Bold()
                                                .FontSize(14)
                                                .FontColor("#D4AF37");
                                        });
                                });

                            // ==================================
                            // PAYMENT INFORMATION
                            // ==================================

                            column.Item()
                                .Background("#151515")
                                .Padding(15)
                                .Column(payment =>
                                {
                                    payment.Item()
                                        .Text(texts.PaymentInformation)
                                        .FontSize(10)
                                        .Bold()
                                        .FontColor("#D4AF37");

                                    payment.Item()
                                        .PaddingTop(5)
                                        .Text(
                                            $"{texts.Status}: " +
                                            $"{GetPaymentStatus(
                                                order.Payment.Status,
                                                texts)}")
                                        .FontColor("#FFFFFF");

                                    if (!string.IsNullOrWhiteSpace(
                                            order.Payment.TransactionId))
                                    {
                                        payment.Item()
                                            .Text(
                                                $"{texts.TransactionId}: " +
                                                $"{order.Payment.TransactionId}")
                                            .FontColor("#CCCCCC");
                                    }
                                });
                        });

                    // ==========================================
                    // FOOTER
                    // ==========================================

                    page.Footer()
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item()
                                .LineHorizontal(1)
                                .LineColor("#D4AF37");

                            column.Item()
                                .PaddingTop(8)
                                .Text(texts.ThankYou)
                                .FontSize(10)
                                .FontColor("#777777");
                        });
                });
            });

            return document.GeneratePdf();
        }

        // =====================================================
        // TRANSLATIONS
        // =====================================================

        private static InvoiceTexts GetTexts(string languageCode)
        {
            return languageCode.ToLowerInvariant() switch
            {
                "ar" => new InvoiceTexts
                {
                    BrandSubtitle = "كرة القدم بهوية",
                    Invoice = "فاتورة",
                    Date = "التاريخ",
                    Payment = "الدفع",
                    BillTo = "بيانات العميل",
                    ShippingTo = "عنوان الشحن",
                    OrderItems = "المنتجات",
                    Image = "الصورة",
                    Product = "المنتج",
                    Quantity = "الكمية",
                    Price = "السعر",
                    Total = "الإجمالي",
                    Subtotal = "المجموع الفرعي",
                    Shipping = "الشحن",
                    Discount = "الخصم",
                    GrandTotal = "الإجمالي النهائي",
                    PaymentInformation = "معلومات الدفع",
                    Status = "الحالة",
                    TransactionId = "رقم العملية",
                    Paid = "مدفوع",
                    Pending = "قيد الانتظار",
                    Failed = "فشل",
                    Cancelled = "ملغي",
                    Refunded = "مسترد",
                    ThankYou = "شكرًا لتسوقك مع MK Galácticos."
                },

                "es" => new InvoiceTexts
                {
                    BrandSubtitle = "Fútbol con identidad",
                    Invoice = "FACTURA",
                    Date = "Fecha",
                    Payment = "Pago",
                    BillTo = "FACTURAR A",
                    ShippingTo = "ENVIAR A",
                    OrderItems = "ARTÍCULOS DEL PEDIDO",
                    Image = "Imagen",
                    Product = "Producto",
                    Quantity = "Cantidad",
                    Price = "Precio",
                    Total = "Total",
                    Subtotal = "Subtotal",
                    Shipping = "Envío",
                    Discount = "Descuento",
                    GrandTotal = "TOTAL",
                    PaymentInformation = "INFORMACIÓN DEL PAGO",
                    Status = "Estado",
                    TransactionId = "ID de transacción",
                    Paid = "Pagado",
                    Pending = "Pendiente",
                    Failed = "Fallido",
                    Cancelled = "Cancelado",
                    Refunded = "Reembolsado",
                    ThankYou = "Gracias por comprar en MK Galácticos."
                },

                _ => new InvoiceTexts
                {
                    BrandSubtitle = "FOOTBALL WITH IDENTITY",
                    Invoice = "INVOICE",
                    Date = "Date",
                    Payment = "Payment",
                    BillTo = "BILL TO",
                    ShippingTo = "SHIPPING TO",
                    OrderItems = "ORDER ITEMS",
                    Image = "IMAGE",
                    Product = "PRODUCT",
                    Quantity = "QTY",
                    Price = "PRICE",
                    Total = "TOTAL",
                    Subtotal = "Subtotal",
                    Shipping = "Shipping",
                    Discount = "Discount",
                    GrandTotal = "TOTAL",
                    PaymentInformation = "PAYMENT INFORMATION",
                    Status = "Status",
                    TransactionId = "Transaction ID",
                    Paid = "Paid",
                    Pending = "Pending",
                    Failed = "Failed",
                    Cancelled = "Cancelled",
                    Refunded = "Refunded",
                    ThankYou = "Thank you for shopping with MK Galácticos."
                }
            };
        }

        private static string GetPaymentStatus(PaymentStatus status, InvoiceTexts texts)
        {
            return status switch
            {
                PaymentStatus.Paid => texts.Paid,
                PaymentStatus.Pending => texts.Pending,
                PaymentStatus.Failed => texts.Failed,
                PaymentStatus.Cancelled => texts.Cancelled,
                PaymentStatus.Refunded => texts.Refunded,
                _ => status.ToString()
            };
        }
        private class InvoiceTexts
        {
            public string BrandSubtitle { get; set; } = null!;
            public string Invoice { get; set; } = null!;
            public string Date { get; set; } = null!;
            public string Payment { get; set; } = null!;
            public string BillTo { get; set; } = null!;
            public string ShippingTo { get; set; } = null!;
            public string OrderItems { get; set; } = null!;
            public string Image { get; set; } = null!;
            public string Product { get; set; } = null!;
            public string Quantity { get; set; } = null!;
            public string Price { get; set; } = null!;
            public string Total { get; set; } = null!;
            public string Subtotal { get; set; } = null!;
            public string Shipping { get; set; } = null!;
            public string Discount { get; set; } = null!;
            public string GrandTotal { get; set; } = null!;
            public string PaymentInformation { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string TransactionId { get; set; } = null!;
            public string Paid { get; set; } = null!;
            public string Pending { get; set; } = null!;
            public string Failed { get; set; } = null!;
            public string Cancelled { get; set; } = null!;
            public string Refunded { get; set; } = null!;
            public string ThankYou { get; set; } = null!;
        }
    }
}
