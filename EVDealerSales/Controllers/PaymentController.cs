using EVDealerSales.BO.DTOs.PaymentDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.WebMVC.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger _logger;

        public PaymentController(
            IPaymentService paymentService,
            IInvoiceService invoiceService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        // List
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _paymentService.GetAllPaymentsAsync();
            return View(result);
        }

        // Detail
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        // Create
        [HttpGet]
        public async Task<IActionResult> Create(Guid? invoiceId = null)
        {
            await PopulateSelectionsAsync(invoiceId);
            var dto = new PaymentDto
            {
                InvoiceId = invoiceId ?? Guid.Empty,
                PaymentDate = DateTime.UtcNow.Date,
                Status = PaymentStatus.Paid
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(dto.InvoiceId);
                return View(dto);
            }

            try
            {
                var created = await _paymentService.CreatePaymentAsync(dto);
                TempData["SuccessMessage"] = "Payment created successfully.";
                return RedirectToAction(nameof(Detail), new { id = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(dto.InvoiceId);
                return View(dto);
            }
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _paymentService.DeletePaymentAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Payment deleted successfully."
                : "Failed to delete the payment.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectionsAsync(Guid? selectedInvoiceId = null)
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            ViewBag.Invoices = invoices.Select(i => new SelectListItem
            {
                Text = $"{i.InvoiceNumber} - {i.CustomerName} - {(i.DueDate?.ToString("yyyy-MM-dd") ?? "-")} - {i.Status} - {i.TotalAmount:C}",
                Value = i.Id.ToString(),
                Selected = selectedInvoiceId.HasValue && i.Id == selectedInvoiceId.Value
            }).ToList();

            ViewBag.StatusItems = Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() })
                .ToList();
        }
    }
}