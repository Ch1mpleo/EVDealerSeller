using EVDealerSales.BO.DTOs.TestDriveDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVDealerSales.WebMVC.Controllers
{
    public class TestDriveController : Controller
    {
        private readonly ITestDriveService _testDriveService;
        private readonly IStaffService _staffService;   // để lấy customer
        private readonly IVehicleService _vehicleService; // để lấy vehicle
        private readonly ILogger _logger;

        public TestDriveController(
            ITestDriveService testDriveService,
            IStaffService staffService,
            IVehicleService vehicleService,
            ILogger<TestDriveController> logger)
        {
            _testDriveService = testDriveService;
            _staffService = staffService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        // B1: hiển thị form Schedule Test Drive (dùng email đã nhập ở flow trước)
        [HttpGet]
        public async Task<IActionResult> Schedule(string email, Guid? vehicleId = null)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("CheckCustomerEmail", "Staff");

            var customers = await _staffService.GetCustomersAsync(1, 1, email);
            var customer = customers.FirstOrDefault();
            if (customer == null)
                return RedirectToAction("AddCustomer", "Staff");

            var vehicles = await _vehicleService.GetAllVehicleAsync(null, null, false, 1, 100);

            var model = new CreateTestDriveDto
            {
                CustomerId = customer.Id,
                VehicleId = vehicleId ?? Guid.Empty
            };

            ViewBag.Customer = customer;    // để hiện tên/email
            ViewBag.Vehicles = vehicles;    // để fill dropdown
            return View(model);
        }

        // B2: post tạo test drive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(CreateTestDriveDto dto)
        {
            if (!ModelState.IsValid || dto.ScheduledDates == null || !dto.ScheduledDates.Any())
            {
                var vehicles = await _vehicleService.GetAllVehicleAsync(null, null, false, 1, 100);
                ViewBag.Vehicles = vehicles;
                ModelState.AddModelError("", "Please select at least one scheduled date.");
                return View(dto);
            }

            try
            {
                var results = await _testDriveService.CreateTestDriveAsync(dto); // trả về List<TestDriveDto>
                TempData["SuccessMessage"] = "Test drives scheduled successfully!";

                // nếu tạo nhiều lịch, chuyển sang danh sách
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling test drive");
                ModelState.AddModelError("", ex.Message);

                var vehicles = await _vehicleService.GetAllVehicleAsync(null, null, false, 1, 100);
                ViewBag.Vehicles = vehicles;
                return View(dto);
            }
        }

        // B3: danh sách test drive
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? sortBy, bool isDescending = false, int page = 1, int pageSize = 10)
        {
            var result = await _testDriveService.GetAllTestDrivesAsync(search, sortBy, isDescending, page, pageSize);

            // Lấy tất cả customerId và vehicleId xuất hiện trong danh sách
            var customerIds = result.Select(x => x.CustomerId).Distinct().ToList();
            var vehicleIds = result.Select(x => x.VehicleId).Distinct().ToList();

            // Lấy thông tin khách hàng
            var customers = new Dictionary<Guid, string>();
            foreach (var id in customerIds)
            {
                var customer = await _staffService.GetCustomerByIdAsync(id);
                if (customer != null)
                    customers[id] = $"{customer.FirstName} {customer.LastName}";
            }
            ViewBag.CustomerNames = customers;

            // Lấy thông tin xe
            var vehicles = new Dictionary<Guid, string>();
            foreach (var id in vehicleIds)
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle != null)
                    vehicles[id] = $"{vehicle.ModelName} ({vehicle.ModelYear})";
            }
            ViewBag.VehicleNames = vehicles;

            return View(result);
        }

        // B4: chi tiết test drive
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var list = await _testDriveService.GetAllTestDrivesAsync(null, null, false, 1, int.MaxValue);
            var testDrive = list.FirstOrDefault(td => td.Id == id);
            if (testDrive == null) return NotFound();

            // Lấy tên khách hàng
            var customer = await _staffService.GetCustomerByIdAsync(testDrive.CustomerId);
            ViewBag.CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : testDrive.CustomerId.ToString();

            // Lấy tên xe
            var vehicle = await _vehicleService.GetVehicleByIdAsync(testDrive.VehicleId);
            ViewBag.Vehicle = vehicle;
            ViewBag.VehicleName = vehicle != null ? $"{vehicle.ModelName} ({vehicle.ModelYear})" : testDrive.VehicleId.ToString();

            return View(testDrive);
        }

        // B5: cập nhật test drive
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var list = await _testDriveService.GetAllTestDrivesAsync(null, null, false, 1, int.MaxValue);
            var testDrive = list.FirstOrDefault(td => td.Id == id);
            if (testDrive == null) return NotFound();

            var dto = new UpdateTestDriveDto
            {
                Id = testDrive.Id,
                ScheduledAt = testDrive.ScheduledAt,
                Status = testDrive.Status,
                Notes = testDrive.Notes,
                StaffId = testDrive.StaffId
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateTestDriveDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var success = await _testDriveService.UpdateTestDriveAsync(dto);
            if (!success) return NotFound();

            TempData["SuccessMessage"] = "Test drive updated successfully!";
            return RedirectToAction("Details", new { id = dto.Id });
        }

        // B6: cập nhật status (Confirm / Completed / Cancel)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, TestDriveStatus status)
        {
            var success = await _testDriveService.UpdateTestDriveStatusAsync(id, status);
            if (!success) return NotFound();

            TempData["SuccessMessage"] = "Status updated successfully!";
            return RedirectToAction("Details", new { id });
        }
    }
}
