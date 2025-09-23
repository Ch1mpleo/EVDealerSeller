namespace EVDealerSales.Models.Entities
{
    public class Feedback : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public Guid? OrderId { get; set; }
        public Order Order { get; set; }
        public string Content { get; set; }
        public Guid? CreatedBy { get; set; }
        public User Creator { get; set; }
        public Guid? ResolvedBy { get; set; }
        public User Resolver { get; set; }
    }
}
