using EVDealerSales.BO.Enums;

namespace EVDealerSales.BO.DTOs.PaymentDTOs
{
    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }

        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }

        public decimal InvoiceTotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance => InvoiceTotalAmount - PaidAmount;
    }
}