using EVDealerSales.BO.DTOs.VehicleDTOs;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVDealerSales.WebMVC.Controllers
{
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        #region View Vehicles - Staff
        public async Task<IActionResult> BrowseVehicles(string? search, string? sortBy, bool isDescending = false, int page = 1, int pageSize = 10)
        {
            var vehicles = await _vehicleService.GetAllVehicleAsync(search, sortBy, isDescending, page, pageSize);
            ViewBag.Search = search;
            return View(vehicles);
        }
        public async Task<IActionResult> DetailVehicles(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        #endregion


        #region Manage Vehicles - Manager
        public async Task<IActionResult> Index(string? search, string? sortBy, bool isDescending = false, int page = 1, int pageSize = 10)
        {
            var vehicles = await _vehicleService.GetAllVehicleAsync(search, sortBy, isDescending, page, pageSize);
            ViewBag.Search = search;
            return View(vehicles);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVehicleDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _vehicleService.CreateVehicleAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            var updateDto = new UpdateVehicleDto
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                TrimName = vehicle.TrimName,
                ModelYear = vehicle.ModelYear,
                BasePrice = vehicle.BasePrice,
                IsActive = vehicle.IsActive
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateVehicleDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _vehicleService.UpdateVehicleAsync(id, dto);
            TempData["SuccessMessage"] = "Vehicle updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();

            await _vehicleService.DeleteVehicleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        #endregion

    }
}
