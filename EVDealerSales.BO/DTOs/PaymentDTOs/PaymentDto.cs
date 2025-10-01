using EVDealerSales.BO.Enums;

namespace EVDealerSales.BO.DTOs.PaymentDTOs
{
    public class PaymentDto
    {
        public Guid InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
    }
}