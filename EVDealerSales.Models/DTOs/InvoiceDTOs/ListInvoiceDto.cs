namespace EVDealerSales.Models.DTOs.InvoiceDTOs
{
    public class ListInvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance => TotalAmount - PaidAmount;
    }
}