namespace EVDealerSales.BO.DTOs.TestDriveDTOs
{
    public class CreateTestDriveDto
    {
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public List<DateTime> ScheduledDates { get; set; } = new();
        public string? Notes { get; set; }
        public Guid? StaffId { get; set; }
    }
}
