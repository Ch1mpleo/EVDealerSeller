using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.DTOs.InvoiceDTOs
{
    public class InvoiceResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string Notes { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal Balance => TotalAmount - PaidAmount;
    }
}