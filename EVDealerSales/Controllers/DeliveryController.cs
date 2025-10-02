using EVDealerSales.BO.DTOs.DeliveryDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.WebMVC.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly IDeliveryService _deliveryService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        public DeliveryController(
            IDeliveryService deliveryService,
            IOrderService orderService,
            ILogger<DeliveryController> logger)
        {
            _deliveryService = deliveryService;
            _orderService = orderService;
            _logger = logger;
        }

        // List
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var deliveries = await _deliveryService.GetAllDeliveriesAsync();
            return View(deliveries);
        }

        // Detail
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
            if (delivery == null) return NotFound();
            return View(delivery);
        }

        // Create
        [HttpGet]
        public async Task<IActionResult> Create(Guid? orderId = null)
        {
            await PopulateSelectionsAsync(orderId);
            var dto = new DeliveryDto
            {
                OrderId = orderId ?? Guid.Empty,
                PlannedDate = DateTime.UtcNow.Date.AddDays(1),
                Status = DeliveryStatus.Scheduled
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeliveryDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(dto.OrderId);
                return View(dto);
            }

            try
            {
                var created = await _deliveryService.CreateDeliveryAsync(dto);
                TempData["SuccessMessage"] = "Delivery created successfully.";
                return RedirectToAction(nameof(Detail), new { id = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delivery");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(dto.OrderId);
                return View(dto);
            }
        }

        // Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
            if (delivery == null) return NotFound();

            // Order is not changeable; keep hidden
            var dto = new DeliveryDto
            {
                OrderId = delivery.OrderId,
                PlannedDate = delivery.PlannedDate,
                ActualDate = delivery.ActualDate,
                Status = delivery.Status
            };

            await PopulateSelectionsAsync(delivery.OrderId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DeliveryDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(dto.OrderId);
                return View(dto);
            }

            try
            {
                var ok = await _deliveryService.UpdateDeliveryAsync(id, dto);
                TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                    ? "Delivery updated successfully."
                    : "No changes were applied.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(dto.OrderId);
                return View(dto);
            }
        }

        // Update status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, DeliveryStatus status)
        {
            var ok = await _deliveryService.UpdateDeliveryStatusAsync(id, status);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Delivery status updated successfully."
                : "Failed to update delivery status.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _deliveryService.DeleteDeliveryAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Delivery deleted successfully."
                : "Failed to delete the delivery.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectionsAsync(Guid? selectedOrderId = null)
        {
            var orders = await _orderService.GetAllOrdersAsync();
            ViewBag.Orders = orders.Select(o => new SelectListItem
            {
                Text = $"{o.CustomerName} - {o.VehicleModel} - {o.OrderDate:yyyy-MM-dd}",
                Value = o.Id.ToString(),
                Selected = selectedOrderId.HasValue && o.Id == selectedOrderId.Value
            }).ToList();

            ViewBag.StatusItems = Enum.GetValues(typeof(DeliveryStatus))
                .Cast<DeliveryStatus>()
                .Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() })
                .ToList();
        }
    }
}