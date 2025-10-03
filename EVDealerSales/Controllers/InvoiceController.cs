using EVDealerSales.BO.DTOs.InvoiceDTOs;
using EVDealerSales.BO.DTOs.OrderDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.WebMVC.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        public InvoiceController(
            IInvoiceService invoiceService,
            IOrderService orderService,
            ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _orderService = orderService;
            _logger = logger;
        }

        // List
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _invoiceService.GetAllInvoicesAsync();
            return View(result);
        }

        // Detail
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // Create
        [HttpGet]
        public async Task<IActionResult> Create(Guid? orderId = null)
        {
            await PopulateSelectionsAsync(orderId);
            var dto = new InvoiceDto
            {
                OrderId = orderId ?? Guid.Empty,
                Status = InvoiceStatus.Unpaid,
                DueDate = DateTime.UtcNow.Date.AddDays(7)
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(dto.OrderId);
                return View(dto);
            }

            try
            {
                var created = await _invoiceService.CreateInvoiceAsync(dto);
                TempData["SuccessMessage"] = "Invoice created successfully.";
                return RedirectToAction(nameof(Detail), new { id = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(dto.OrderId);
                return View(dto);
            }
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _invoiceService.DeleteInvoiceAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Invoice deleted successfully."
                : "Failed to delete the invoice.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectionsAsync(Guid? selectedOrderId = null)
        {
            var orders = await _orderService.GetAllOrdersAsync();

            var ordersWithNoInvoice = new List<ListOrderDto>();

            foreach (var order in orders)
            {
                var existingInvoice = await _invoiceService.GetInvoiceByOrderIdAsync(order.Id);
                if ((existingInvoice == null || (selectedOrderId.HasValue && order.Id == selectedOrderId.Value)) && order.Status == OrderStatus.Pending.ToString())
                {
                    ordersWithNoInvoice.Add(order);
                }
            }

            ViewBag.Orders = ordersWithNoInvoice.Select(o => new SelectListItem
            {
                Text = $"{o.CustomerName} - {o.VehicleModel} - {o.OrderDate:yyyy-MM-dd} ({o.Status})",
                Value = o.Id.ToString(),
                Selected = selectedOrderId.HasValue && o.Id == selectedOrderId.Value
            }).ToList();

            ViewBag.StatusItems = Enum.GetValues(typeof(InvoiceStatus))
                .Cast<InvoiceStatus>()
                .Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() })
                .ToList();
        }
    }
}