using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.CustomerDTOs;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVDealerSales.WebMVC.Controllers
{
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly ILogger _logger;
        private const int DefaultPageSize = 10;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public IActionResult Customers()
        {
            return View();
        }

        #region Customer Management

        // GET: Manager/Customers
        public async Task<IActionResult> Customers(int page = 1, int pageSize = DefaultPageSize, string searchTerm = null)
        {
            try
            {
                var customers = await _staffService.GetCustomersAsync(page, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers list");
                TempData["ErrorMessage"] = "There was an error retrieving the customers list.";
                return View(new Pagination<GetCustomerDto>(new List<GetCustomerDto>(), 0, page, pageSize));
            }
        }

        // GET: Manager/CustomerDetails/{id}
        public async Task<IActionResult> CustomerDetails(Guid id)
        {
            try
            {
                var customer = await _staffService.GetCustomerByIdAsync(id);
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer details for ID: {CustomerId}", id);
                TempData["ErrorMessage"] = "Customer not found or there was an error retrieving details.";
                return RedirectToAction(nameof(Customers));
            }
        }

        // GET: Manager/EditCustomer/{id}
        public async Task<IActionResult> EditCustomer(Guid id)
        {
            try
            {
                var customer = await _staffService.GetCustomerByIdAsync(id);
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer for editing. ID: {CustomerId}", id);
                TempData["ErrorMessage"] = "Customer not found or there was an error retrieving details.";
                return RedirectToAction(nameof(Customers));
            }
        }

        // POST: Manager/EditCustomer/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(Guid id, GetCustomerDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _staffService.UpdateCustomerAsync(id, model);
                TempData["SuccessMessage"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Customers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer. ID: {CustomerId}", id);
                TempData["ErrorMessage"] = "There was an error updating the customer.";
                return View(model);
            }
        }

        // POST: Manager/DeleteCustomer/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                await _staffService.DeleteCustomerAsync(id);
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer. ID: {CustomerId}", id);
                TempData["ErrorMessage"] = "There was an error deleting the customer.";
            }

            return RedirectToAction(nameof(Customers));
        }

        #endregion
    }
}
