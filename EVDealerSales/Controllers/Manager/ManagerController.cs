using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.UserDTOs;
using EVDealerSales.Models.Enums;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.WebMVC.Controllers.Manager
{
    [Authorize(Policy = "ManagerPolicy")]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;
        private readonly ILogger _logger;
        private const int DefaultPageSize = 10;

        public ManagerController(IManagerService managerService, ILogger<ManagerController> logger)
        {
            _managerService = managerService;
            _logger = logger;
        }

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
    }
}
