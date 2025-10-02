using EVDealerSales.BO.DTOs.ReportDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using Microsoft.Extensions.Logging;


namespace EVDealerSales.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<List<IncomeByStaffReportDto>> GetIncomeByStaffReportAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync(
           o => o.Status == OrderStatus.Confirmed && !o.IsDeleted,
           o => o.Staff
       );
            var result = orders
                .GroupBy(o => new { o.StaffId, o.Staff.FullName })
                .Select(g => new IncomeByStaffReportDto
                {
                    StaffId = g.Key.StaffId,
                    StaffName = g.Key.FullName,
                    TotalIncome = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalIncome)
                .ToList();
            return result;
        }

        public async Task<List<MonthlyIncomeReportDto>> GetMonthlyIncomeReportAsync(int year)
        {
            var orders = await _unitOfWork.Orders.GetAllAsync(
            o => o.Status == OrderStatus.Confirmed && o.OrderDate.Year == year && !o.IsDeleted
        );
            var result = orders
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new MonthlyIncomeReportDto
                {
                    Year = year,
                    Month = g.Key,
                    TotalIncome = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToList();
            return result;
        }

        public async Task<List<TestDriveByModelReportDto>> GetTestDriveByModelReportAsync()
        {
            var testDrives = await _unitOfWork.TestDrives.GetAllAsync(td => !td.IsDeleted, td => td.Vehicle);
            var result = testDrives
                .GroupBy(td => new { td.Vehicle.ModelName, td.Vehicle.ModelYear })
                .Select(g => new TestDriveByModelReportDto
                {
                    ModelName = g.Key.ModelName,
                    ModelYear = g.Key.ModelYear ?? 0,
                    TestDriveCount = g.Count()
                })
                .OrderByDescending(x => x.TestDriveCount)
                .ToList();
            return result;
        }
    }
}
