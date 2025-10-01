using EVDealerSales.BO.Enums;

namespace EVDealerSales.BO.DTOs.TestDriveDTOs
{
    public class TestDriveDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public TestDriveStatus Status { get; set; }
        public string? Notes { get; set; }
        public Guid? StaffId { get; set; }
    }
}