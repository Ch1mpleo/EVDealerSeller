using EVDealerSales.Models.DTOs.VehicleDTOs;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace EVDealerSales.WebMVC.Controllers
{
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> Index(string? search, string? sortBy, bool isDescending = false, int page = 1, int pageSize = 10)
        {
            var vehicles = await _vehicleService.GetAllVehicleAsync(search, sortBy, isDescending, page, pageSize);
            ViewBag.Search = search;
            return View(vehicles);
        }

        public async Task<IActionResult> DetailsPage(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        public IActionResult CreatePage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePage(CreateVehicleDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
             try
            {
                await _vehicleService.CreateVehicleAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return View(dto);
            }
        }

        public async Task<IActionResult> EditPage(Guid id)
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
                ImageUrl = vehicle.ImageUrl,
                BatteryCapacity = vehicle.BatteryCapacity,
                RangeKM = vehicle.RangeKM,
                ChargingTime = vehicle.ChargingTime,
                TopSpeed = vehicle.TopSpeed,
                IsActive = vehicle.IsActive
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPage(Guid id, UpdateVehicleDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _vehicleService.UpdateVehicleAsync(id, dto);
            TempData["SuccessMessage"] = "Vehicle updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeletePage(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();

            await _vehicleService.DeleteVehicleAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
