using EVDealerSales.BO.Enums;

namespace EVDealerSales.BO.DTOs.TestDriveDTOs
{
    public class UpdateTestDriveDto
    {
        public Guid Id { get; set; }
        public DateTime ScheduledAt { get; set; }
        public TestDriveStatus Status { get; set; }
        public string? Notes { get; set; }
        public Guid? StaffId { get; set; }
    }
}
