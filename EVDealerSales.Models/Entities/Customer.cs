namespace EVDealerSales.Models.Entities
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }

        // Navigation
        public ICollection<TestDrive> TestDrives { get; set; }
        public ICollection<Quote> Quotes { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
