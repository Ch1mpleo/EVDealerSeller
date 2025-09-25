using EVDealerSales.Models.Enums;

namespace EVDealerSales.Models.Entities
{
    public class TestDrive : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public DateTime ScheduledAt { get; set; }
        public TestDriveStatus Status { get; set; }
        public string Notes { get; set; }
        public Guid? StaffId { get; set; }
        public User Staff { get; set; }
    }
}
