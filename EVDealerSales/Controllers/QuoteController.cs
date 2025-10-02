using EVDealerSales.BO.DTOs.QuoteDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.WebMVC.Controllers
{
    public class QuoteController : Controller
    {
        private readonly IQuoteService _quoteService;
        private readonly IStaffService _staffService;     // for customers
        private readonly IVehicleService _vehicleService; // for vehicles
        private readonly ILogger _logger;

        private const int DefaultPageSize = 10;

        public QuoteController(
            IQuoteService quoteService,
            IStaffService staffService,
            IVehicleService vehicleService,
            ILogger<QuoteController> logger)
        {
            _quoteService = quoteService;
            _staffService = staffService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        // List
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int page = 1, int pageSize = DefaultPageSize)
        {
            var result = await _quoteService.GetAllQuotesAsync(page, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View(result);
        }

        // Detail
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var quote = await _quoteService.GetQuoteByIdAsync(id);
            if (quote == null) return NotFound();
            return View(quote);
        }

        // Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectionsAsync();
            return View(new QuoteDto { Status = QuoteStatus.Pending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(dto.CustomerId, dto.VehicleId);
                return View(dto);
            }

            try
            {
                await _quoteService.CreateQuoteAsync(dto);
                TempData["SuccessMessage"] = "Quote created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quote");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(dto.CustomerId, dto.VehicleId);
                return View(dto);
            }
        }

        // Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var quote = await _quoteService.GetQuoteByIdAsync(id);
            if (quote == null) return NotFound();

            var dto = new QuoteDto
            {
                Id = quote.Id,
                CustomerId = quote.CustomerId,
                VehicleId = quote.VehicleId,
                QuotedPrice = quote.QuotedPrice,
                FinalPrice = quote.FinalPrice,
                Status = quote.Status,
                ValidUntil = quote.ValidUntil,
                Remarks = quote.Remarks
            };

            await PopulateSelectionsAsync(dto.CustomerId, dto.VehicleId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, QuoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(dto.CustomerId, dto.VehicleId);
                return View(dto);
            }

            try
            {
                await _quoteService.UpdateQuoteAsync(id, dto);
                TempData["SuccessMessage"] = "Quote updated successfully.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quote");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(dto.CustomerId, dto.VehicleId);
                return View(dto);
            }
        }

        // Update status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, QuoteStatus status)
        {
            var ok = await _quoteService.UpdateQuoteStatusAsync(id, status);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Quote status updated successfully."
                : "Failed to update quote status.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _quoteService.DeleteQuoteAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Quote deleted successfully."
                : "Failed to delete the quote.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectionsAsync(Guid? selectedCustomerId = null, Guid? selectedVehicleId = null)
        {
            var customers = await _staffService.CustomerListNameAsync();
            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
                Selected = selectedCustomerId.HasValue && c.Id == selectedCustomerId.Value
            }).ToList();

            var vehicles = await _vehicleService.GetAllVehicleAsync(null, null, false, 1, 1000);
            ViewBag.Vehicles = vehicles.Select(v => new SelectListItem
            {
                Text = $"{v.ModelName}{(string.IsNullOrWhiteSpace(v.TrimName) ? "" : $" ({v.TrimName})")}{(v.ModelYear.HasValue ? $" - {v.ModelYear}" : "")}",
                Value = v.Id.ToString(),
                Selected = selectedVehicleId.HasValue && v.Id == selectedVehicleId.Value
            }).ToList();

            ViewBag.StatusItems = Enum.GetValues(typeof(QuoteStatus))
                .Cast<QuoteStatus>()
                .Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() })
                .ToList();
        }
    }
}