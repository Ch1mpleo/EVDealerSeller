using EVDealerSales.BO.Enums;

namespace EVDealerSales.BO.DTOs.InvoiceDTOs
{
    public class InvoiceDto
    {
        public Guid OrderId { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string Notes { get; set; }
    }
}