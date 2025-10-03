using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVDealerSales.WebMVC.Controllers
{
    [Authorize(Policy = "ManagerPolicy")]
    public class ReportController : Controller
    {
        private readonly ILogger _logger;
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // GET: /Report/Dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View("~/Views/Report/Dashboard.cshtml");
        }

        // GET: /Report/TestDriveByModel
        [HttpGet]
        public async Task<IActionResult> TestDriveByModel()
        {
            try
            {
                var data = await _reportService.GetTestDriveByModelReportAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Test Drive by Model report.");
                return StatusCode(500, "An error occurred while generating the Test Drive report.");
            }
        }

        // GET: /Report/IncomeByStaff
        [HttpGet]
        public async Task<IActionResult> IncomeByStaff()
        {
            try
            {
                var data = await _reportService.GetIncomeByStaffReportAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Income by Staff report.");
                return StatusCode(500, "An error occurred while generating the Income by Staff report.");
            }
        }

        // GET: /Report/MonthlyIncome?year=2025
        [HttpGet]
        public async Task<IActionResult> MonthlyIncome(int year)
        {
            try
            {
                if (year <= 0)
                {
                    return BadRequest("Invalid year parameter.");
                }

                var data = await _reportService.GetMonthlyIncomeReportAsync(year);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate Monthly Income report for year {year}.");
                return StatusCode(500, "An error occurred while generating the Monthly Income report.");
            }
        }
    }
}