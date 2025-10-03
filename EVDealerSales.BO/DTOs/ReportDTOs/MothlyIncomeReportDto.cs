namespace EVDealerSales.BO.DTOs.ReportDTOs
{
    public class MonthlyIncomeReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalIncome { get; set; }
    }
}
