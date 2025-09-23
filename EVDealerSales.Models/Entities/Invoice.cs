namespace EVDealerSales.Models.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal SubtotalAmount { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; } = 0.00M;
        public InvoiceStatus Status { get; set; }
        public string Notes { get; set; }

        // Navigation
        public ICollection<Payment> Payments { get; set; }
    }
}
