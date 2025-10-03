using EVDealerSales.BO.DTOs.OrderDTOs;
using EVDealerSales.BO.DTOs.OrderItemDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.WebMVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IQuoteService _quoteService;
        private readonly ILogger _logger;

        public OrderController(
            IOrderService orderService,
            IQuoteService quoteService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _quoteService = quoteService;
            _logger = logger;
        }

        // List
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        // Detail
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // Create
        [HttpGet]
        public async Task<IActionResult> Create(Guid? quoteId = null)
        {
            await PopulateQuoteSelectionsAsync(quoteId);

            var dto = new OrderDto
            {
                QuoteId = quoteId ?? Guid.Empty,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = new OrderItemDto { Quantity = 1 }
            };

            if (quoteId.HasValue)
            {
                var quote = await _quoteService.GetQuoteByIdAsync(quoteId.Value);
                if (quote != null)
                {
                    dto.Items.VehicleId = quote.VehicleId;
                    dto.Items.Name = quote.VehicleModel;
                    dto.Items.UnitPrice = quote.FinalPrice ?? quote.QuotedPrice;
                    dto.Items.LineTotal = dto.Items.UnitPrice * dto.Items.Quantity;
                }
            }

            return View(dto);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateQuoteSelectionsAsync(dto.QuoteId);
                return View(dto);
            }

            try
            {
                var created = await _orderService.CreateOrderAsync(dto);
                TempData["SuccessMessage"] = "Order created successfully.";
                return RedirectToAction(nameof(Detail), new { id = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateQuoteSelectionsAsync(dto.QuoteId);
                return View(dto);
            }
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _orderService.DeleteOrderAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order deleted successfully."
                : "Failed to delete the order.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateQuoteSelectionsAsync(Guid? selectedQuoteId = null)
        {
            // Pull a large page of quotes to populate the dropdown
            //var quotes = await _quoteService.GetAllQuotesAsync(1, 1000, null);
            var quotes = await _quoteService.GetAllQuotesNoPaginAsync();

            ViewBag.Quotes = quotes.Select(q => new SelectListItem
            {
                Text = $"{q.CustomerName} - {q.VehicleModel} [{q.Status}]",
                Value = q.Id.ToString(),
                Selected = selectedQuoteId.HasValue && q.Id == selectedQuoteId.Value
            }).ToList();

            ViewBag.StatusItems = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() })
                .ToList();
        }
    }
}