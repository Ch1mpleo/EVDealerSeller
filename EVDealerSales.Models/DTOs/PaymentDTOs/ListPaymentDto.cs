using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.PaymentDTOs
{
    public class ListPaymentDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
    }
}