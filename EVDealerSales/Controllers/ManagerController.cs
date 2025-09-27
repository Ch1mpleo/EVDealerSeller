using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.CustomerDTOs;
using EVDealerSales.Models.DTOs.UserDTOs;
using EVDealerSales.Models.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.WebMVC.Controllers
{
    [Authorize(Roles = "DealerManager")]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;
        private readonly ILogger<ManagerController> _logger;
        private const int DefaultPageSize = 10;

        public ManagerController(IManagerService managerService, ILogger<ManagerController> logger)
        {
            _managerService = managerService;
            _logger = logger;
        }

        #region Dashboard

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Employee Management

        // GET: Manager/Employees
        public async Task<IActionResult> Employees(int page = 1, int pageSize = DefaultPageSize, string searchTerm = null)
        {
            try
            {
                var employees = await _managerService.GetEmployeesAsync(page, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees list");
                TempData["ErrorMessage"] = "There was an error retrieving the employees list.";
                return View(new Pagination<GetEmployeeDto>(new List<GetEmployeeDto>(), 0, page, pageSize));
            }
        }

        // GET: Manager/EmployeeDetails/{id}
        public async Task<IActionResult> EmployeeDetails(Guid id)
        {
            try
            {
                var employee = await _managerService.GetEmployeeByIdAsync(id);
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee details for ID: {EmployeeId}", id);
                TempData["ErrorMessage"] = "Employee not found or there was an error retrieving details.";
                return RedirectToAction(nameof(Employees));
            }
        }

        // GET: Manager/CreateEmployee
        public IActionResult CreateEmployee()
        {
            ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Select(r => new SelectListItem
                {
                    Text = r.ToString(),
                    Value = ((int)r).ToString()
                });
            return View(new CreateEmployeeDto { IsActive = true });
        }

        // POST: Manager/CreateEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                    .Cast<RoleType>()
                    .Select(r => new SelectListItem
                    {
                        Text = r.ToString(),
                        Value = ((int)r).ToString()
                    });
                return View(model);
            }

            try
            {
                await _managerService.AddEmployeeAsync(model);
                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction(nameof(Employees));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                TempData["ErrorMessage"] = ex.Data.Contains("StatusCode") && (int)ex.Data["StatusCode"] == 409
                    ? "An employee with this email already exists."
                    : "There was an error creating the employee.";

                ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                    .Cast<RoleType>()
                    .Select(r => new SelectListItem
                    {
                        Text = r.ToString(),
                        Value = ((int)r).ToString()
                    });
                return View(model);
            }
        }

        // GET: Manager/EditEmployee/{id}
        public async Task<IActionResult> EditEmployee(Guid id)
        {
            try
            {
                var employee = await _managerService.GetEmployeeByIdAsync(id);
                ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                    .Cast<RoleType>()
                    .Select(r => new SelectListItem
                    {
                        Text = r.ToString(),
                        Value = ((int)r).ToString(),
                        Selected = r == employee.Role
                    });
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee for editing. ID: {EmployeeId}", id);
                TempData["ErrorMessage"] = "Employee not found or there was an error retrieving details.";
                return RedirectToAction(nameof(Employees));
            }
        }

        // POST: Manager/EditEmployee/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(Guid id, GetEmployeeDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                    .Cast<RoleType>()
                    .Select(r => new SelectListItem
                    {
                        Text = r.ToString(),
                        Value = ((int)r).ToString(),
                        Selected = r == model.Role
                    });
                return View(model);
            }

            try
            {
                await _managerService.UpdateEmployeeAsync(id, model);
                TempData["SuccessMessage"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Employees));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee. ID: {EmployeeId}", id);
                TempData["ErrorMessage"] = "There was an error updating the employee.";

                ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                    .Cast<RoleType>()
                    .Select(r => new SelectListItem
                    {
                        Text = r.ToString(),
                        Value = ((int)r).ToString(),
                        Selected = r == model.Role
                    });
                return View(model);
            }
        }

        // POST: Manager/ToggleEmployeeStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEmployeeStatus(Guid id)
        {
            try
            {
                await _managerService.EmployeeIsActiveAsync(id);
                TempData["SuccessMessage"] = "Employee status updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling employee status. ID: {EmployeeId}", id);
                TempData["ErrorMessage"] = "There was an error updating the employee status.";
            }

            return RedirectToAction(nameof(Employees));
        }

        #endregion

        #region Customer Management

        // GET: Manager/Customers
        public async Task<IActionResult> Customers(int page = 1, int pageSize = DefaultPageSize, string searchTerm = null)
        {
            try
            {
                var customers = await _managerService.GetCustomersAsync(page, pageSize, searchTerm);
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
                var customer = await _managerService.GetCustomerByIdAsync(id);
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
                var customer = await _managerService.GetCustomerByIdAsync(id);
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
                await _managerService.UpdateCustomerAsync(id, model);
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
                await _managerService.DeleteCustomerAsync(id);
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
