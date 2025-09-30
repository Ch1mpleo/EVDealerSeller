using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.Entities
{
    public class Order : BaseEntity
    {
        public Guid QuoteId { get; set; }
        public Quote Quote { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public Guid StaffId { get; set; }
        public User Staff { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public string DiscountNote { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; }
        public Contract Contract { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
        public Delivery Delivery { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
