namespace EVDealerSales.Models.Entities
{
    public class Vehicle : BaseEntity
    {
        public string ModelName { get; set; }
        public string TrimName { get; set; }
        public int? ModelYear { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<TestDrive> TestDrives { get; set; }
        public ICollection<Quote> Quotes { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
