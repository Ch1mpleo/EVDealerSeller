namespace EVDealerSales.BO.DTOs.ReportDTOs
{
    public class IncomeByStaffReportDto
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
    }
}
