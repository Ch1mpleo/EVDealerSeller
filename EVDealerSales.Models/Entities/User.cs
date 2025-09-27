using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public RoleType Role { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Quote> Quotes { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<TestDrive> TestDrives { get; set; }
        public ICollection<Feedback> CreatedFeedbacks { get; set; }
        public ICollection<Feedback> ResolvedFeedbacks { get; set; }
    }
}
