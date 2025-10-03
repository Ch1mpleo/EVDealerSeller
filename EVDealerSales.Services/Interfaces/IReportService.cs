using EVDealerSales.BO.DTOs.ReportDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVDealerSales.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<TestDriveByModelReportDto>> GetTestDriveByModelReportAsync();
        Task<List<IncomeByStaffReportDto>> GetIncomeByStaffReportAsync();
        Task<List<MonthlyIncomeReportDto>> GetMonthlyIncomeReportAsync(int year);
    }
}
